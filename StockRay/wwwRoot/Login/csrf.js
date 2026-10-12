let requestTokenPromise;

async function getRequestToken() {
    if (!requestTokenPromise) {
        requestTokenPromise = fetch("/antiforgery/token", { credentials: "same-origin" })
            .then(async response => {
                if (!response.ok) {
                    throw new Error("Could not get an antiforgery token.");
                }

                const data = await response.json();
                return data.requestToken;
            })
            .catch(error => {
                requestTokenPromise = undefined;
                throw error;
            });
    }

    return requestTokenPromise;
}

export async function csrfFetch(url, options = {}) {
    const requestToken = await getRequestToken();
    const headers = new Headers(options.headers ?? {});
    headers.set("X-CSRF-TOKEN", requestToken);

    return fetch(url, {
        ...options,
        credentials: "same-origin",
        headers
    });
}
