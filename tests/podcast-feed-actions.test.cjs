const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const vm = require('node:vm');

const source = readFileSync(join(__dirname, '../DotCast.App/wwwroot/js/ui.js'), 'utf8');
const feedUrl = 'https://example.com/library/book/user/rss?presignedUrl=expiry.signature&format=rss';

function deferred() {
    let resolve, reject;
    const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
    return { promise, resolve, reject };
}

function setup({ clipboardAvailable = true, rejectClipboard = false } = {}) {
    const response = deferred();
    const output = { hidden: true };
    const input = {
        value: '', focused: false, selected: false,
        focus() { this.focused = true; },
        select() { this.selected = true; },
        setSelectionRange(start, end) { this.selection = [start, end]; }
    };
    const status = { textContent: '' };
    const elements = { '[data-feed-output]': output, '[data-feed-url]': input, '[data-feed-status]': status };
    const button = {
        disabled: false,
        dataset: {
            feedEndpoint: '/api/audiobooks/book/podcast-feed', preparing: 'Preparing',
            copied: 'Copied', copyManually: 'Copy manually', failed: 'Failed'
        },
        closest() { return { querySelector: selector => elements[selector] }; }
    };
    const state = { activeGesture: false, fetches: [], writes: 0, clipboardText: null };
    const clipboard = {
        write(items) {
            assert.equal(state.activeGesture, true, 'Clipboard access must start before the click handler yields.');
            state.writes++;
            if (rejectClipboard) return Promise.reject(new Error('NotAllowedError'));
            return items[0].data['text/plain'].then(async blob => { state.clipboardText = await blob.text(); });
        }
    };
    class ClipboardItem {
        constructor(data) { this.data = data; }
    }
    const context = {
        window: { ClipboardItem }, ClipboardItem, Blob,
        navigator: clipboardAvailable ? { clipboard } : {},
        document: { addEventListener() {} },
        fetch(url, options) { state.fetches.push({ url, options }); return response.promise; }
    };
    vm.runInNewContext(source, context);

    function click() {
        state.activeGesture = true;
        const result = context.window.DotCastUi.copyPodcastFeed(button);
        state.activeGesture = false;
        return result;
    }
    function respond(url = feedUrl) { response.resolve({ ok: true, text: async () => url }); }
    return { button, input, output, status, state, response, click, respond };
}

test('a delayed server response is copied without losing the original user gesture', async () => {
    const ui = setup();
    assert.equal(ui.state.fetches.length, 0, 'Rendering must not generate a personal feed.');
    const pending = ui.click();
    assert.equal(ui.state.fetches.length, 1);
    assert.equal(ui.state.writes, 1, 'write() must already be pending while the server has not responded.');
    assert.equal(ui.input.value, '');
    assert.equal(ui.button.disabled, true);
    assert.equal(ui.status.textContent, 'Preparing');
    const request = ui.state.fetches[0];
    assert.equal(request.options.credentials, 'same-origin');
    assert.equal(request.options.cache, 'no-store');
    assert.equal(request.options.redirect, 'error');
    ui.respond();
    await pending;
    assert.equal(ui.state.clipboardText, feedUrl);
    assert.equal(ui.status.textContent, 'Copied');
    assert.equal(ui.button.disabled, false);
});

test('clipboard denial exposes and selects the complete URL for manual copying', async () => {
    const ui = setup({ rejectClipboard: true });
    const pending = ui.click();
    ui.respond();
    await pending;
    assert.equal(ui.output.hidden, false);
    assert.equal(ui.input.value, feedUrl);
    assert.equal(ui.status.textContent, 'Copy manually');
    assert.equal(ui.input.focused, true);
    assert.equal(ui.input.selected, true);
    assert.deepEqual(ui.input.selection, [0, feedUrl.length]);
});

test('unavailable clipboard APIs still provide a manual fallback', async () => {
    const ui = setup({ clipboardAvailable: false });
    const pending = ui.click();
    ui.respond();
    await pending;
    assert.equal(ui.state.writes, 0);
    assert.equal(ui.input.value, feedUrl);
    assert.equal(ui.status.textContent, 'Copy manually');
});

test('HTTP failure clears stale URLs and never reports a successful copy', async () => {
    const ui = setup();
    ui.input.value = 'previous-user-feed';
    ui.output.hidden = false;
    const pending = ui.click();
    assert.equal(ui.input.value, '');
    ui.response.resolve({ ok: false, status: 401 });
    await pending;
    assert.equal(ui.output.hidden, true);
    assert.equal(ui.status.textContent, 'Failed');
    assert.equal(ui.state.clipboardText, null);
    assert.equal(ui.button.disabled, false);
});

test('network and authentication redirect failures are visible and allow retry', async () => {
    const ui = setup();
    const pending = ui.click();
    ui.response.reject(new Error('Failed to fetch'));
    await pending;
    assert.equal(ui.status.textContent, 'Failed');
    assert.equal(ui.output.hidden, true);
    assert.equal(ui.button.disabled, false);
});

test('repeated clicks while preparing create only one feed request', async () => {
    const ui = setup();
    const first = ui.click();
    await ui.click();
    assert.equal(ui.state.fetches.length, 1);
    ui.respond();
    await first;
    assert.equal(ui.status.textContent, 'Copied');
});

function setupPlayer() {
    const response = deferred();
    const output = { hidden: true };
    const link = {
        textContent: '', focused: false,
        removeAttribute(name) { delete this[name]; },
        focus() { this.focused = true; },
        scrollIntoView() {},
        click() { throw new Error('App navigation must wait for a real user tap.'); }
    };
    const status = { textContent: '' };
    const buttons = ['Overcast', 'PocketCasts', 'AntennaPod'].map(player => ({
        disabled: false,
        dataset: {
            playerEndpoint: '/api/audiobooks/book/podcast-feed?player=' + player,
            preparing: 'Preparing', failed: 'Failed', continue: 'Continue to ' + player,
            ready: 'Tap Continue to ' + player
        },
        closest() { return panel; }
    }));
    const elements = { '[data-player-output]': output, '[data-player-link]': link, '[data-feed-status]': status };
    const panel = { querySelector: selector => elements[selector], querySelectorAll: () => buttons };
    const requests = [];
    const context = {
        window: { open() { throw new Error('Opening an app must not create a new tab.'); } },
        document: { addEventListener() {} },
        fetch(url, options) { requests.push({ url, options }); return response.promise; }
    };
    vm.runInNewContext(source, context);
    return { response, output, link, status, buttons, requests, prepare: button => context.window.DotCastUi.preparePodcastPlayer(button) };
}

test('player preparation is lazy and leaves a direct link for a real tap without opening a tab', async () => {
    const ui = setupPlayer();
    assert.equal(ui.requests.length, 0);
    const pending = ui.prepare(ui.buttons[0]);
    assert.equal(ui.requests.length, 1);
    assert.equal(ui.output.hidden, true);
    assert.equal(ui.link.href, undefined);
    assert.equal(ui.status.textContent, 'Preparing');
    const playerUrl = 'overcast://x-callback-url/add?url=' + encodeURIComponent(feedUrl);
    ui.response.resolve({ ok: true, text: async () => playerUrl });
    await pending;
    assert.equal(ui.output.hidden, false);
    assert.equal(ui.link.href, playerUrl);
    assert.equal(ui.link.textContent, 'Continue to Overcast');
    assert.equal(ui.link.focused, true);
    assert.equal(ui.status.textContent, 'Tap Continue to Overcast');
    assert.equal(ui.requests[0].options.redirect, 'error');
    assert.equal(ui.requests[0].options.cache, 'no-store');
});

test('a pending player request prevents another app selection from replacing it out of order', async () => {
    const ui = setupPlayer();
    const pending = ui.prepare(ui.buttons[0]);
    assert.equal(ui.buttons.every(button => button.disabled), true);
    await ui.prepare(ui.buttons[1]);
    assert.equal(ui.requests.length, 1);
    ui.response.resolve({ ok: true, text: async () => 'overcast://x-callback-url/add?url=feed' });
    await pending;
    assert.equal(ui.buttons.every(button => !button.disabled), true);
});

test('a failed player request removes stale launch links and leaves retry available', async () => {
    const ui = setupPlayer();
    ui.link.href = 'overcast://old-feed';
    ui.output.hidden = false;
    const pending = ui.prepare(ui.buttons[0]);
    ui.response.resolve({ ok: false, status: 503 });
    await pending;
    assert.equal(ui.link.href, undefined);
    assert.equal(ui.output.hidden, true);
    assert.equal(ui.status.textContent, 'Failed');
    assert.equal(ui.buttons.every(button => !button.disabled), true);
});

test('player preparation reports network and authentication redirect errors without navigating', async () => {
    const ui = setupPlayer();
    const pending = ui.prepare(ui.buttons[0]);
    ui.response.reject(new Error('Failed to fetch'));
    await pending;
    assert.equal(ui.output.hidden, true);
    assert.equal(ui.link.href, undefined);
    assert.equal(ui.status.textContent, 'Failed');
});
