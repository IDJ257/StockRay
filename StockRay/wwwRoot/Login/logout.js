export function logout() {
    setLoggedOut();
    updateAuthUI();
    window.location.href = "/PublicDash";
}

export function updateAuthUI() {
    const token = localStorage.getItem("stockrayJWT");
    document.body.classList.toggle("logged-in", !!token);
}

function setLoggedOut() {
    localStorage.removeItem("stockrayJWT");
}