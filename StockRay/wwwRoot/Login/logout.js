import { csrfFetch } from "../Login/csrf.js";

export function logout() {
    csrfFetch("/logout", { method: "POST" })
        .catch(() => {})
        .finally(() => window.location.href = "/PublicDash");
}

export async function updateAuthUI() {
    try {
        const response = await fetch("/account", { credentials: "same-origin" });
        document.body.classList.toggle("logged-in", response.ok);
    } catch {
        document.body.classList.remove("logged-in");
    }
}
