window.DotCastUi = {
    dialogs: new Map(),
    openDialog(id, helper) {
        const dialog = document.getElementById(id);
        if (!dialog || dialog.open) return;
        const trigger = document.activeElement;
        const trap = event => {
            if (event.key !== 'Tab') return;
            const controls = [...dialog.querySelectorAll('button, a[href], input, select, textarea, summary, [tabindex]')]
                .filter(element => !element.disabled && element.tabIndex >= 0 && element.getClientRects().length);
            const first = controls[0], last = controls.at(-1);
            if (!first) { event.preventDefault(); dialog.focus(); return; }
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        };
        const cancel = event => { event.preventDefault(); helper.invokeMethodAsync('CloseDialog'); };
        const backdrop = event => { if (event.target === dialog) {
            const bounds = dialog.getBoundingClientRect();
            if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom)
                helper.invokeMethodAsync('CloseDialog');
        } };
        dialog.addEventListener('cancel', cancel);
        dialog.addEventListener('click', backdrop);
        dialog.addEventListener('keydown', trap);
        this.dialogs.set(id, { dialog, trigger, cancel, backdrop, trap });
        dialog.showModal();
        (dialog.querySelector('[autofocus], button, input, a[href]') || dialog).focus();
        document.body.classList.add('dialog-open');
    },
    closeDialog(id) {
        const entry = this.dialogs.get(id);
        if (!entry) return;
        entry.dialog.close();
        entry.dialog.removeEventListener('cancel', entry.cancel);
        entry.dialog.removeEventListener('click', entry.backdrop);
        entry.dialog.removeEventListener('keydown', entry.trap);
        this.dialogs.delete(id);
        if (!this.dialogs.size) document.body.classList.remove('dialog-open');
        if (entry.trigger?.isConnected) entry.trigger.focus({ preventScroll: true });
    },
    async copy(text) {
        try { await navigator.clipboard.writeText(text); return true; }
        catch { return false; }
    },
    saveLibrary(url) {
        const rails = [...document.querySelectorAll('[data-rail]')].map(element => [element.dataset.rail, element.scrollLeft]);
        const sections = [...document.querySelectorAll('.library-filters details')].map(element => element.open);
        try { sessionStorage.setItem('dotcast-library', JSON.stringify({ url, scroll: window.scrollY, rails, sections })); } catch { }
    },
    restoreLibrary(url) {
        let state;
        try { state = JSON.parse(sessionStorage.getItem('dotcast-library')); } catch { return; }
        if (state?.url !== url) return;
        document.querySelectorAll('.library-filters details').forEach((element, index) => element.open = !!state.sections[index]);
        document.querySelectorAll('[data-rail]').forEach(element => element.scrollLeft = state.rails.find(([key]) => key === element.dataset.rail)?.[1] || 0);
        requestAnimationFrame(() => window.scrollTo({ top: state.scroll, behavior: 'instant' }));
    },
    jump(id) { document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' }); }
};
document.addEventListener('keydown', event => {
    if (event.target.getAttribute('role') !== 'tab') return;
    const tabs = [...event.target.closest('[role="tablist"]').querySelectorAll('[role="tab"]')];
    let index = tabs.indexOf(event.target);
    if (event.key === 'ArrowRight') index = (index + 1) % tabs.length;
    else if (event.key === 'ArrowLeft') index = (index + tabs.length - 1) % tabs.length;
    else if (event.key === 'Home') index = 0;
    else if (event.key === 'End') index = tabs.length - 1;
    else return;
    event.preventDefault(); tabs[index].focus(); tabs[index].click();
});
