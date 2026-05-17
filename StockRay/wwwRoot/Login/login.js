
let loginOverlay;
let loginForm;
let loginMessage;
let setMessage;

export function openModal() {
    loginForm.reset();
    setMessage("", false);
    loginOverlay.hidden = false;
    document.body.classList.add("modal-open");
    requestAnimationFrame(() => loginOverlay.classList.add("is-visible"));
}

export function closeModal() {
    loginOverlay.classList.remove("is-visible");
    document.body.classList.remove("modal-open");
    window.setTimeout(() => {
        loginOverlay.hidden = true;
    }, 220);
}

export function setupLoginModal() {
    loginOverlay = document.getElementById("loginOverlay");
    loginForm = document.getElementById("loginForm");
    loginMessage = document.getElementById("loginMessage");


    const loginClose = document.getElementById("loginClose");
    const loginLink = document.querySelector("[data-login-open]");

    if (!loginLink || !loginOverlay || !loginForm || !loginClose || !loginMessage) {
        return;
    }

    setMessage = (message, isError) => {
        loginMessage.textContent = message;
        loginMessage.classList.toggle("error", isError);
        loginMessage.classList.toggle("success", !isError && message.length > 0);
    };


    loginLink.addEventListener("click", (event) => {
        event.preventDefault();
        openModal();
    });

    loginClose.addEventListener("click", closeModal);

    loginOverlay.addEventListener("click", (event) => {
        if (event.target === loginOverlay) {
            closeModal();
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && !loginOverlay.hidden) {
            closeModal();
        }
    });

    loginForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        const payload = {
            Email: loginForm.elements.email.value,
            Password: loginForm.elements.password.value
        };

        if (!payload.Email || !payload.Password) {
            setMessage("Please enter your email and password.", true);
            return;
        }

        try {
            const response = await fetch("/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            const data = await response.text();

            if (!response.ok) {
                setMessage(`${data}`, true);
                return;
            }



            setMessage("Login successful.", false);
            localStorage.setItem("stockrayJWT", data);

            setTimeout(() => {
                closeModal();
            }, 350)

        } catch {
            setMessage("Login failed. Please try again.", true);
        }
    });
};