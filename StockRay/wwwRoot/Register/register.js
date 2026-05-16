const form = document.getElementById("registerForm");
const message = document.getElementById("authMessage");

const setMessage = (text, isError = false) => {
    message.textContent = text;
    message.classList.toggle("error", isError);
};

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    setMessage("");

    const payload = {
        name: form.name.value,
        password: form.password.value,
        email: form.email.value
    };

    try {
        const response = await fetch("/register", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            setMessage("Registration failed. Try again.", true);
            return;
        }

        setMessage("Registration successful. You can now login.");
        form.reset();
    } catch {
        setMessage("Unable to register right now.", true);
    }
});