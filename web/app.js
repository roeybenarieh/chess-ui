// Browser port of chess air_client/*.cs. Same protocol as the desktop client; the
// browser reaches the server's raw TCP port through websockify at /ws (see docker-compose.yml).
import { createRsa } from './rsa.js'

const $ = id => document.getElementById(id)
const el = (tag, props) => Object.assign(document.createElement(tag), props)

// ---------- Program.cs: connection, message routing ----------
let ws, rsa, connecting = false
let token = null // session id from the server after login; sent on reconnect so the server continues the same session
let received = '' // every message ends with '\n'; a WebSocket frame can hold several messages or part of one
const handlers = {}
const send = msg => { if (ws && ws.readyState === WebSocket.OPEN) ws.send(new TextEncoder().encode(msg + '\n')) }

// the connection can drop (phone suspends the page, network change): reconnect and resume the session
async function connect() {
    if (connecting) return
    connecting = true
    rsa = await createRsa()
    const sock = new WebSocket(`${location.protocol === 'https:' ? 'wss' : 'ws'}://${location.host}/ws`, ['binary'])
    sock.binaryType = 'arraybuffer'
    ws = sock
    received = ''
    sock.onopen = () => {
        connecting = false
        send('public key:' + rsa.xml) // the server encrypts everything it sends with this key
        if (token) send('###resume###' + token)
        $('status').hidden = true
    }
    sock.onclose = () => {
        if (sock !== ws) return // replaced by a newer connection
        connecting = false
        $('status').textContent = 'reconnecting to the server...'
        $('status').hidden = false
        setTimeout(connect, 2000)
    }
    sock.onmessage = e => {
        if (sock !== ws) return
        const messages = (received + new TextDecoder().decode(e.data)).split('\n')
        received = messages.pop()
        for (const text of messages) {
            if (text.startsWith('captcha%')) { handlers[document.querySelector('.form:not([hidden])').id](text); continue }
            if (text.startsWith('public key:')) continue
            const msg = rsa.decrypt(text)
            if (msg.startsWith('session:')) token = msg.slice(8)
            else if (msg === 'resume failed') {
                token = null
                $('status').textContent = 'your session expired, refresh the page to log in again'
                $('status').hidden = false
            }
            else if (msg !== 'resumed') handlers[document.querySelector('.form:not([hidden])').id](msg) // the form can change mid-frame
        }
    }
}

// a suspended phone browser leaves its socket dead: when the page is shown again, replace the socket and resume
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState !== 'visible' || connecting) return
    const old = ws
    ws = null
    if (old) old.close()
    connect()
})

function show(id) {
    for (const f of document.querySelectorAll('.form')) f.hidden = f.id !== id
    fit()
}

// scale the fixed-size "window" to fit the browser
function fit() {
    const f = document.querySelector('.form:not([hidden])')
    f.style.zoom = Math.min(innerWidth / parseInt(f.style.width), innerHeight / parseInt(f.style.height))
}
addEventListener('resize', fit)

// ---------- login.cs ----------
const loginOut = s => $('l-out').textContent = s
const captcha = [] // 'pos' / 'neg' per captcha, unset = negative
const badEdge = (re, ...fields) => fields.some(f => re.test(f))

$('loginbutton').onclick = () => {
    const user = $('username').value, pass = $('password').value
    if ($('loginbutton').textContent === 'login') {
        loginOut('')
        if (!user || !pass) loginOut([!user && 'username', !pass && 'password'].filter(Boolean).join(' and ') + ' must be inserted!')
        else if (badEdge(/^['/]/, user, pass)) loginOut("you cant put ' or / at the begging of a field")
        else if (badEdge(/['/]$/, user, pass)) loginOut("you cant put ' or / at the end of a field")
        else if (badEdge(/&/, user, pass)) loginOut("you cant put '&' in a field")
        else send('###login###' + user + '&' + pass)
    } else { // verify
        send('###mail+capcha###' + pass + '%' + [0, 1, 2].map(i => captcha[i] === 'pos' ? 'True' : 'False').join('%'))
        loginOut('')
    }
}

$('registration').onclick = () => {
    for (const input of $('register').querySelectorAll('input')) input.value = ''
    $('r-out').textContent = ''
    show('register')
}

handlers.login = t => {
    if (t.startsWith('captcha%')) {
        $('login').append(el('span', { className: 'captcha', textContent: t.slice(10), style: `left:62px;top:${t[8] * 80 + 30}px` }))
    } else if (t === 'login done') {
        for (const id of ['l-ulabel', 'l-user', 'l-lock', 'registration', 'l-title', 'username']) $(id).hidden = true
        Object.assign($('password'), { value: '', type: 'text' })
        Object.assign($('loginbutton'), { textContent: 'verify' })
        Object.assign($('loginbutton').style, { left: '570px', top: '323px' })
        Object.assign($('l-plabel'), { textContent: 'received gmail code:' })
        Object.assign($('l-plabel').style, { whiteSpace: 'normal', width: '250px' })
        $('login').append(el('span', { className: 'captcha', textContent: 'captcha - for each paragraphs mark if its a positive/negative movie review', style: 'left:62px;top:0px;height:20px' }))
        for (let i = 0; i < 3; i++) {
            const pos = el('button', { className: 'cap', textContent: 'positive', style: `left:620px;top:${40 + i * 80}px` })
            const neg = el('button', { className: 'cap', textContent: 'negative', style: `left:690px;top:${40 + i * 80}px` })
            pos.onclick = () => { captcha[i] = 'pos'; pos.style.background = 'green'; neg.style.background = '' }
            neg.onclick = () => { captcha[i] = 'neg'; neg.style.background = 'red'; pos.style.background = '' }
            $('login').append(pos, neg)
        }
    } else if (t === 'code done') {
        loginOut('login completed, starting game...')
        startChess()
    } else if (t === 'code incorect') loginOut('the verefication code is incorrect ')
    else if (t === 'captcha incorect') loginOut('the captcha is incorrect ')
    else if (t === 'coudnt send mail') loginOut('coudnt send mail to your mail adress')
    else if (t === 'login not done') loginOut('username or password are incorrect')
}

// ---------- register.cs ----------
const regOut = s => $('r-out').textContent = s

$('regist').onclick = () => {
    const [user, pass, mail, nick, age, country, city] =
        ['rusername', 'rpassword', 'remail', 'rnickname', 'rage', 'rcountry', 'rcity'].map(id => $(id).value)
    if (!mail.includes('@')) regOut('you must enter a valid mail adress')
    else if (!user || !pass) regOut([!user && 'username', !pass && 'password'].filter(Boolean).join(' and ') + ' must be inserted!')
    else if (user.length < 5) regOut('username must contain at least five characters')
    else if (pass.length < 5) regOut('password must contain at least five characters')
    else if (badEdge(/^['/]/, user, pass)) regOut("you cant put ' or / at the begging of the information")
    else if (badEdge(/['/]$/, user, pass)) regOut("you cant put ' or / at the end of the information")
    else if (!/^\d*$/.test(age)) regOut('age must be a number') // the db column is an integer
    else {
        regOut('')
        send('###regist###' + [user, pass, nick, mail, age, country, city].join('&'))
    }
}

$('back').onclick = () => show('login')

handlers.register = t => {
    if (t === 'regist complited') {
        regOut('registration complete!')
        setTimeout(() => { loginOut(''); show('login') }, 1000)
    } else if (t === 'regist mail incorrect') regOut('server couldnt confirm youre mail')
    else regOut(t)
}

// ---------- chess.cs + promotion_hundler.cs ----------
// piece codes = image index: 0-5 black pawn,knight,bishop,rook,queen,king; 6-11 white
const squares = [], moves = $('moves'), games = $('games'), promo = $('promo')
let board = []          // 64 piece codes (row * 8 + col), null = empty, from this player's point of view
let iswhite = true, myTurn = false, inGame = false
let marked = null       // square of the piece whose potential moves are shown
let viewed = 0          // paging offset of the played-games list

// server coordinates are from white's point of view; black sees the board flipped
const pos = c => iswhite ? +c : 7 - c
const isMine = k => board[k] != null && (iswhite ? board[k] > 5 : board[k] < 6)
const sendMove = (from, i, j, promotion = '') => send('###move###' + pos(from >> 3) + pos(from & 7) + pos(i) + pos(j) + promotion)
const clearPot = () => squares.forEach(b => b.classList.remove('pot'))

function render() {
    squares.forEach((b, k) => b.style.backgroundImage = board[k] == null ? '' : `url(img/${board[k]}.png)`)
}

function startPosition() {
    const up = iswhite ? 0 : 6, down = 6 - up
    const back = iswhite ? [3, 1, 2, 4, 5, 2, 1, 3] : [3, 1, 2, 5, 4, 2, 1, 3]
    board = Array(64).fill(null)
    for (let j = 0; j < 8; j++) {
        board[j] = back[j] + up
        board[8 + j] = up
        board[48 + j] = down
        board[56 + j] = back[j] + down
    }
    render()
}

for (let k = 0; k < 64; k++) {
    const b = el('button', { className: ((k >> 3) + k) % 2 ? 'w' : 'g' })
    b.onclick = () => squareClick(k)
    squares.push(b)
}
$('board').append(...squares)

function squareClick(k) {
    promo.hidden = true
    if (!myTurn) return
    if (marked == null && isMine(k)) {
        marked = k
        send('###pot_move###' + pos(k >> 3) + pos(k & 7)) // ask the server for this piece's legal moves
    } else if (marked != null && squares[k].classList.contains('pot')) {
        if (board[marked] === (iswhite ? 6 : 0) && k < 8) showPromotion(k)
        else sendMove(marked, k >> 3, k & 7)
    } else {
        clearPot()
        marked = null
    }
}

function showPromotion(col) {
    promo.style.left = 350 + 80 * col + 'px'
    promo.replaceChildren(...[4, 3, 2, 1].map(p => { // queen, rook, bishop, knight (= promotion edgecase codes)
        const b = el('button', { style: `background-image:url(img/${p + (iswhite ? 6 : 0)}.png)` })
        b.onclick = () => { sendMove(marked, 0, col, p); promo.hidden = true }
        return b
    }))
    promo.hidden = false
}

function addHistory(m, edge) {
    const L = 'ABCDEFGH'
    const b = el('button', {
        className: myTurn === iswhite ? 'mw' : 'mb', // white moved
        textContent: edge.startsWith('0') ? edge : L[m[1]] + (8 - m[0]) + ',' + L[m[3]] + (8 - m[2]) + edge,
    })
    const snapshot = board.slice()
    b.onclick = () => { clearPot(); promo.hidden = true; board = snapshot.slice(); render() }
    moves.append(b)
    moves.scrollTop = moves.scrollHeight
}

function onMove(m) { // "XYXY#edgecase", X = row, Y = column
    clearPot()
    if (moves.lastElementChild) moves.lastElementChild.click() // back to the current position if browsing history
    const [a, b, c, d] = [0, 1, 2, 3].map(k => pos(m[k]))
    board[c * 8 + d] = board[a * 8 + b]
    board[a * 8 + b] = null
    const edge = m.split('#')[1]
    let note = ''
    if (edge === '5') { // castling: move the rook too
        if (d - b === 2) { board[a * 8 + (iswhite ? 5 : 4)] = board[a * 8 + 7]; board[a * 8 + 7] = null; note = '0 - 0' }
        else { board[a * 8 + (iswhite ? 3 : 2)] = board[a * 8]; board[a * 8] = null; note = '0-0-0' }
    } else if (edge === '9') board[a * 8 + d] = null // en passant
    else if (edge !== '0') { // 1-4 promotion to knight, bishop, rook, queen; 7/8 first king/rook move (a bare "-", like the desktop client)
        note = '-'
        if (edge < '5') {
            board[c * 8 + d] = +edge + (iswhite === myTurn ? 6 : 0)
            note += ' kbrq'[edge]
        }
    }
    render()
    addHistory(m, note)
    marked = null
    myTurn = !myTurn
}

handlers.chess = t => {
    if (t.startsWith('###gamedata###')) { // id$white VS black AT date
        const [id, label] = t.slice(14).split('$')
        games.append(el('button', { textContent: label, onclick: () => { if (!inGame) send('###getgame###' + id) } }))
    } else if (t.startsWith('###start_game###')) { // my nick & opponent nick & my color [& oldgame]
        const [me, opp, color, old] = t.slice(16).split('&')
        iswhite = myTurn = color === 'white'
        startPosition()
        $('outcome').textContent = ''
        if (!old) {
            $('play_friend').hidden = $('playai').hidden = true
            inGame = true
        }
        Object.assign($('me'), { textContent: me, hidden: false })
        Object.assign($('opp'), { textContent: opp, hidden: false })
        moves.replaceChildren()
    } else if (t.startsWith('###move###')) onMove(t.slice(10))
    else if (t.startsWith('###posmoves###')) { // XY,XY,...
        for (const m of t.slice(14).split(',')) squares[pos(m[0]) * 8 + pos(m[1])].classList.add('pot')
    } else if (t.startsWith('###endgame###')) {
        myTurn = inGame = false
        $('play_friend').textContent = 'play vs friend'
        $('play_friend').hidden = $('playai').hidden = false
        $('outcome').textContent = t.slice(13) + '!'
        refreshGames()
    }
}

function refreshGames() {
    games.replaceChildren()
    send('###getplayedgames###' + viewed)
}

function startChess() {
    show('chess')
    startPosition()
    send('###getplayedgames###0')
}

$('resign').onclick = () => { if (inGame) send('###resignation###') }
$('playai').onclick = () => send('###ready_to_play_vs_ai###')
$('play_friend').onclick = () => {
    if ($('play_friend').textContent !== 'play vs friend') return
    $('play_friend').textContent = 'waiting for a friend..(hopefully you have one)'
    send('###ready_to_play_vs_friend###')
}
$('more').onclick = () => { if (games.children.length >= 14) { viewed += 15; refreshGames() } }
$('less').onclick = () => { if (viewed >= 15) { viewed -= 15; refreshGames() } }

show('login')
connect()
