window.UploadHelpers = {
    registrations: new Map(),
    register(id, helper, pickerId, progressId, text) {
        const picker = document.getElementById(pickerId);
        const container = document.getElementById(progressId);
        if (!picker || !container) return;
        const change = async () => {
            const files = Array.from(picker.files);
            if (!files.length) return;
            container.replaceChildren();
            const status = document.createElement('p'); status.setAttribute('role', 'status'); container.append(status);
            if (files.some(file => !file.size)) { status.textContent = text.empty; picker.value = ''; return; }
            if (new Set(files.map(file => file.name)).size !== files.length) { status.textContent = text.duplicate; picker.value = ''; return; }
            picker.disabled = true; status.textContent = text.preparing;
            let completed = 0;
            const finish = async () => {
                if (++completed !== files.length) return;
                status.textContent = text.processing; picker.disabled = false; picker.value = '';
                await helper.invokeMethodAsync('UploadCompleted');
            };
            let urls;
            try { urls = await helper.invokeMethodAsync('GeneratePresignedUrls', files.map(file => file.name)); }
            catch { status.textContent = text.failed; picker.disabled = false; picker.value = ''; return; }
            status.textContent = '';
            for (const file of files) {
                const row = document.createElement('div'); row.className = 'upload-file-row';
                const label = document.createElement('span'); label.textContent = file.name;
                const progress = document.createElement('progress'); progress.max = 100; progress.value = 0; progress.setAttribute('aria-label', file.name);
                const message = document.createElement('span');
                const retry = document.createElement('button'); retry.type = 'button'; retry.className = 'secondary-action'; retry.textContent = text.retry; retry.hidden = true;
                row.append(label, progress, message, retry); container.append(row);
                const transfer = async url => {
                    retry.hidden = true; message.textContent = ''; progress.value = 0;
                    try { if (!url) throw new Error(); await uploadFile(file, url, progress); message.textContent = text.uploaded; await finish(); }
                    catch { message.textContent = text.failed; retry.hidden = false; }
                };
                retry.onclick = async () => {
                    retry.disabled = true;
                    try { const replacement = await helper.invokeMethodAsync('GeneratePresignedUrls', [file.name]); await transfer(replacement[file.name]); }
                    catch { message.textContent = text.failed; }
                    finally { retry.disabled = false; }
                };
                await transfer(urls[file.name]);
            }
        };
        picker.addEventListener('change', change);
        this.registrations.set(id, { picker, change });
    },
    unregister(id) { const entry = this.registrations.get(id); if (entry) entry.picker.removeEventListener('change', entry.change); this.registrations.delete(id); }
};
function uploadFile(file, url, progress) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest(); xhr.open('PUT', url);
        const body = new FormData(); body.append('request', file, file.name);
        xhr.upload.onprogress = event => { if (event.lengthComputable) progress.value = event.loaded / event.total * 100; };
        xhr.onload = () => { if (xhr.status >= 200 && xhr.status < 300) { progress.value = 100; resolve(); } else reject(new Error()); };
        xhr.onerror = () => reject(new Error()); xhr.onabort = () => reject(new Error());
        xhr.send(body);
    });
}
