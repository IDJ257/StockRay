const form = document.getElementById("loginForm");
const message = document.getElementById("authMessage");

const setMessage = (text, isError = false) => {
    message.textContent = text;
    message.classList.toggle("error", isError);
};

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    setMessage("");

    const payload = {
        userName: form.email.value,
        password: form.password.value
    };

    try {
        const response = await fetch("/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            setMessage("Login failed. Check your credentials.", true);
            return;
        }

        const token = await response.json();
        localStorage.setItem("stockray.jwt", token);
        setMessage("Login successful.");
    } catch {
        setMessage("Unable to login right now.", true);
    }
});