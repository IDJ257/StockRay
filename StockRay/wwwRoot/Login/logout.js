export function logout() {
    setLoggedOut();
    updateAuthUI();
    window.location.href = "/PublicDash";
}

//Remove LogOutButton if no JWT
//TODO:improve it because JWT can expire but still sit in the localstorage.
export function updateAuthUI() {
    const token = localStorage.getItem("stockrayJWT");
    document.body.classList.toggle("logged-in", !!token);
}

function setLoggedOut() {
    localStorage.removeItem("stockrayJWT");
}