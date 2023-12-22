// Function to upload file using the pre-signed URL

class UploadHelpers {
    static dotNetHelper;

    static setDotNetHelper(value) {
        UploadHelpers.dotNetHelper = value;
    }
}

window.UploadHelpers = UploadHelpers;
async function uploadFile(file, presignedUrl) {
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
                document.getElementById(`progress_${file.name}`).value = progress;
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
