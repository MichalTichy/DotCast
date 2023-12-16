function uploadFileToExternalService(presignedUrl, fileData, progressCallback) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open("PUT", presignedUrl, true);
        xhr.upload.onprogress = (event) => {
            if (event.lengthComputable) {
                const percentComplete = Math.round((event.loaded / event.total) * 100);
                progressCallback.invokeMethodAsync('ReportProgress', percentComplete);
            }
        };
        xhr.onload = () => xhr.status === 200 ? resolve("Upload successful") : reject("Upload failed");
        xhr.onerror = () => reject("Error during upload");
        xhr.send(new Blob([fileData]));
    });
}