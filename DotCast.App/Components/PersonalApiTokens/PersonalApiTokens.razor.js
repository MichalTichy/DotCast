let requestToken;
async function send(method, body, id) {
    const response = await fetch(id ? "/api/personal-tokens/" + encodeURIComponent(id) : "/api/personal-tokens", {
        method, credentials: "same-origin", cache: "no-store", redirect: "error",
        headers: { "Content-Type": "application/json", ...(requestToken ? { "RequestVerificationToken": requestToken } : {}) },
        body: body ? JSON.stringify(body) : undefined
    });
    if (!response.ok) throw new Error("Token request failed");
    return response.status === 204 ? null : await response.json();
}
export async function list() {
    const result = await send("GET");
    requestToken = result.requestToken;
    return result.tokens;
}
export async function create(name, expiresAt, canWrite) {
    const result = await send("POST", { name, expiresAt, canWrite });
    return result.credential;
}
export async function revoke(id) { await send("DELETE", undefined, id); }
