# 06 New Audiobook Page

Goal: move single audiobook upload into its own primary flow.

- Add `/NewAudioBook`.
- Single upload only.
- Use existing id-generation and upload-start messages.
- Show name input, generated id preview, file picker, upload progress, and feedback.
- Improve or wrap the current upload component to avoid global DOM ids.

Acceptance:

- User can enter a name and see an id preview.
- User can choose files and start upload.
- Upload progress and errors are visible.
- Bulk upload is not included.
