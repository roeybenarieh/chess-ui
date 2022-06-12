const { test } = require('node:test')
const assert = require('node:assert/strict')
const fs = require('node:fs')
const vm = require('node:vm')

function loginPage() {
    const source = fs.readFileSync(__dirname + '/app.js', 'utf8')
    const section = source.slice(source.indexOf('// ---------- login.cs ----------'), source.indexOf('// ---------- register.cs ----------'))
    const elements = new Map()
    const element = id => {
        if (!elements.has(id)) elements.set(id, { value: '', textContent: '', style: {}, append() {} })
        return elements.get(id)
    }
    element('loginbutton').textContent = 'login'
    element('username').value = 'player'
    element('password').value = 'password'
    const sent = []
    const handlers = {}
    vm.runInNewContext(section, {
        $: element, handlers, send: message => { sent.push(message); return true },
        el: () => ({ style: {} }), show() {}, startChess() {}
    })
    return { element, handlers, sent }
}

test('rapid login clicks send one request until the captcha screen is ready', () => {
    const page = loginPage()
    page.element('loginbutton').onclick()
    page.element('loginbutton').onclick()
    assert.equal(page.sent.length, 1)
    page.handlers.login('login done')
    page.element('password').value = '123456'
    page.element('loginbutton').onclick()
    assert.match(page.sent[1], /^###mail\+capcha###/)
})

test('failed login permits another attempt', () => {
    const page = loginPage()
    page.element('loginbutton').onclick()
    page.handlers.login('login not done')
    page.element('loginbutton').onclick()
    assert.equal(page.sent.length, 2)
})
