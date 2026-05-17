export function setupRegisterModal(onSuccessCallBack) {
    const registerLink = document.querySelector("[data-register-open]");
    const registerOverlay = document.getElementById("registerOverlay");
    const registerForm = document.getElementById("registerForm");
    const registerClose = document.getElementById("registerClose");
    const registerMessage = document.getElementById("registerMessage");

    if (!registerLink || !registerOverlay || !registerForm || !registerClose || !registerMessage) {
        return;
    }

    const setMessage = (message, isError) => {
        registerMessage.textContent = message;
        registerMessage.classList.toggle("error", isError);
        registerMessage.classList.toggle("success", !isError && message.length > 0);
    };

    const openModal = () => {
        registerForm.reset();
        setMessage("", false);
        registerOverlay.hidden = false;
        document.body.classList.add("modal-open");
        requestAnimationFrame(() => registerOverlay.classList.add("is-visible"));
    };

    const closeModal = () => {
        registerOverlay.classList.remove("is-visible");
        document.body.classList.remove("modal-open");
        window.setTimeout(() => {
            registerOverlay.hidden = true;
        }, 220);
    };

    registerLink.addEventListener("click", (event) => {
        event.preventDefault();
        openModal();
    });

    registerClose.addEventListener("click", closeModal);

    registerOverlay.addEventListener("click", (event) => {
        if (event.target === registerOverlay) {
            closeModal();
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && !registerOverlay.hidden) {
            closeModal();
        }
    });

    registerForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        const payload = {
            Name: registerForm.elements.name.value.trim(),
            Email: registerForm.elements.email.value.trim(),
            Password: registerForm.elements.password.value
        };

        if (!payload.Name || !payload.Email || !payload.Password) {
            setMessage("Please complete all fields.", true);
            return;
        }

        try {
            const response = await fetch("/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

           

            if (!response.ok) {
                const data = await response.json();
                setMessage(`${data.message}`, true);
                return;
            }

            setMessage("Registration successful. You can log in now.", false);
            setTimeout(() => {
                closeModal()

                if (onSuccessCallBack) {
                    onSuccessCallBack();
                }

            }, 800)



        } catch {
            setMessage("Registration failed. Please try again.", true);
        }
    });
};