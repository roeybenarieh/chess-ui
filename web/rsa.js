// The server encrypts every message it sends (except "public key:" and "captcha%")
// with the client's RSA public key: .NET RSACryptoServiceProvider, 2048 bit,
// PKCS#1 v1.5 padding, UTF-16LE plaintext, ciphertext sent as "b0,b1,...,b255".
// WebCrypto has no PKCS#1 v1.5 decryption, so the private-key math is done with BigInt.

const b64urlToBytes = s => Uint8Array.from(atob(s.replace(/-/g, '+').replace(/_/g, '/')), c => c.charCodeAt(0))
const b64urlToStd = s => btoa(String.fromCharCode(...b64urlToBytes(s)))
const toBig = bytes => BigInt('0x' + (Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('') || '0'))

function modPow(base, exp, mod) {
    let r = 1n
    base %= mod
    for (; exp; exp >>= 1n) {
        if (exp & 1n) r = r * base % mod
        base = base * base % mod
    }
    return r
}

// returns { xml: public key in .NET ToXmlString format, decrypt(text) }
export async function createRsa() {
    const pair = await crypto.subtle.generateKey(
        { name: 'RSA-OAEP', modulusLength: 2048, publicExponent: new Uint8Array([1, 0, 1]), hash: 'SHA-1' },
        true, ['encrypt', 'decrypt'])
    const k = await crypto.subtle.exportKey('jwk', pair.privateKey)
    const [p, q, dp, dq, qi] = [k.p, k.q, k.dp, k.dq, k.qi].map(v => toBig(b64urlToBytes(v)))
    const size = b64urlToBytes(k.n).length

    return {
        xml: `<RSAKeyValue><Modulus>${b64urlToStd(k.n)}</Modulus><Exponent>${b64urlToStd(k.e)}</Exponent></RSAKeyValue>`,
        decrypt(text) {
            const c = toBig(text.split(',').map(Number))
            const m1 = modPow(c, dp, p), m2 = modPow(c, dq, q) // CRT: ~4x faster than c^d mod n
            const m = m2 + ((qi * (m1 - m2)) % p + p) % p * q
            const em = Uint8Array.from(m.toString(16).padStart(size * 2, '0').match(/../g), h => parseInt(h, 16))
            const sep = em.indexOf(0, 2)
            if (em[0] !== 0 || em[1] !== 2 || sep < 10) throw new Error('bad PKCS#1 v1.5 padding')
            return new TextDecoder('utf-16le').decode(em.subarray(sep + 1))
        },
    }
}
