// Function to upload file using the pre-signed URL

class UploadHelpers {
    static dotNetHelper;
    static helpers = {};

    static setDotNetHelper(value) {
        UploadHelpers.dotNetHelper = value;
    }

    static register(componentId, dotNetHelper, filePickerId, progressContainerId) {
        UploadHelpers.helpers[componentId] = dotNetHelper;
        const filePicker = document.getElementById(filePickerId);
        if (!filePicker) {
            return;
        }

        filePicker.addEventListener('change', async function (e) {
            const files = e.target.files;
            const fileNames = Array.from(files).map(file => file.name);
            const progressContainer = document.getElementById(progressContainerId);

            try {
                const presignedUrls = await dotNetHelper.invokeMethodAsync('GeneratePresignedUrls', fileNames);

                for (let file of files) {
                    const progressBar = document.createElement("progress");
                    progressBar.id = `${componentId}_progress_${cssEscape(file.name)}`;
                    progressBar.value = 0;
                    progressBar.max = 100;
                    progressContainer.appendChild(progressBar);

                    const presignedUrl = presignedUrls[file.name];
                    if (presignedUrl) {
                        await uploadFile(file, presignedUrl, progressBar.id);
                    } else {
                        console.error("No pre-signed URL for " + file.name);
                    }
                }
            } catch (error) {
                console.error("Error generating or retrieving pre-signed URLs: ", error);
            }
        });
    }
}

window.UploadHelpers = UploadHelpers;
function cssEscape(value) {
    if (window.CSS && window.CSS.escape) {
        return window.CSS.escape(value);
    }

    return value.replace(/[^a-zA-Z0-9_-]/g, "_");
}

async function uploadFile(file, presignedUrl, progressElementId) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open("PUT", presignedUrl);

        const formData = new FormData();
        //formData.append('file', file);

        // Append the file to the form data under the key 'request' or whatever the server expects
        formData.append("request", file, file.name);

        // Update progress bar
        xhr.upload.onprogress = function (event) {
            if (event.lengthComputable) {
                const progress = (event.loaded / event.total) * 100;
                const progressElement = document.getElementById(progressElementId ?? `progress_${file.name}`);
                if (progressElement) {
                    progressElement.value = progress;
                }
            }
        };

        xhr.onload = function () {
            if (xhr.status == 200) {
                resolve("Upload successful!");
            } else {
                reject("Upload failed!");
            }
        };

        xhr.onerror = function () {
            reject("Error in upload!");
        };
        
        xhr.send(formData);
    });
}
