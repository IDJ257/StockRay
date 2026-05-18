
import { setupLoginModal, openModal } from "../Login/login.js";
import { setupRegisterModal } from "../Register/register.js";
import { connect } from "../SignalRConnect.js"

const grid = document.getElementById("dashboardGrid");
const symbolsById = new Map();
const cardsById = new Map();

const formatNumber = (value) => {
    if (value === null || value === undefined || Number.isNaN(value)) {
        return "--";
    }

    return Number(value).toFixed(2);
};

//const normalizeSymbol = (raw) => ({
//    id: raw.id ?? raw.Id,
//    name: raw.name ?? raw.Name ?? "Unknown",
//    open: raw.open ?? raw.Open,
//    high: raw.high ?? raw.High,
//    low: raw.low ?? raw.Low,
//    currentPrice: raw.currentPrice ?? raw.CurrentPrice
//});

const renderDashboard = () => {
    grid.innerHTML = "";

    //while (items.length < 9) {
    //    items.push({
    //        id: `placeholder-${items.length}`,
    //        name: "Loading...",
    //        open: null,
    //        high: null,
    //        low: null,
    //        currentPrice: null,
    //        placeholder: true
    //    });
    //}




    symbolsById.forEach((symbol, key) => {
        const card = document.createElement("div");
        card.className = "bubble";
        card.innerHTML = `
                    <h3>${symbol.name}</h3>
                    <div class="micro-grid">
                        <div class="current-price">${formatNumber(symbol.currentPrice)}</div>
                        <div class="micro-pill open">Open <span>${formatNumber(symbol.open)}</span></div>
                        <div class="micro-pill high">High <span>${formatNumber(symbol.high)}</span></div>
                        <div class="micro-pill low">Low <span>${formatNumber(symbol.low)}</span></div>
                    </div>
                `;


        cardsById.set(key, card);
        grid.appendChild(card);

        if (key === 3) {
            const divider = document.createElement("div");
            divider.className = "divider";
            grid.appendChild(divider);
        }
    });
};

const loadDashboard = async () => {
    try {
        const response = await fetch("/public", { cache: "no-store" });
        if (!response.ok) {
            return;
        }

        const payload = await response.json();
        const payloadArray = payload.symbols ?? [];
        symbolsById.clear();

        payloadArray.forEach(s => {
            symbolsById.set(s.id, s)
        });

        renderDashboard();
    } catch {
        // ignore - landing page will keep placeholders
    }
};

const handleSignalUpdate = (updates) => {

    //UPDATES E LIST<OUTBOUNDSTOCKPRICE>
    updates.forEach(update => {
        if (!symbolsById.has(update.id)) {
            return;
        }
        const existing = symbolsById.get(update.id);
        existing.open = update.open;
        existing.high = update.high;
        existing.low = update.low;
        existing.currentPrice = update.currentPrice;


        const card = cardsById.get(update.id);


        card.querySelector(".micro-pill.open span").textContent =
            formatNumber(update.open);

        card.querySelector(".current-price").textContent =
            formatNumber(update.currentPrice);

        card.querySelector(".micro-pill.high span").textContent =
            formatNumber(update.high);

        card.querySelector(".micro-pill.low span").textContent =
            formatNumber(update.low);

    });
};

/*const setupRegisterModal = () => {
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
                setMessage("Registration failed. Please try again.", true);
                return;
            }

            setMessage("Registration successful. You can log in now.", false);
            setTimeout(() => {
                closeModal();
            }, 800)

            
            
        } catch {
            setMessage("Registration failed. Please try again.", true);
        }
    });
};*/

/*const setupLoginModal = () => {
    const loginLink = document.querySelector("[data-login-open]");
    const loginOverlay = document.getElementById("loginOverlay");
    const loginForm = document.getElementById("loginForm");
    const loginClose = document.getElementById("loginClose");
    const loginMessage = document.getElementById("loginMessage");

    if (!loginLink || !loginOverlay || !loginForm || !loginClose || !loginMessage) {
        return;
    }

    const setMessage = (message, isError) => {
        loginMessage.textContent = message;
        loginMessage.classList.toggle("error", isError);
        loginMessage.classList.toggle("success", !isError && message.length > 0);
    };

    const openModal = () => {
        loginForm.reset();
        setMessage("", false);
        loginOverlay.hidden = false;
        document.body.classList.add("modal-open");
        requestAnimationFrame(() => loginOverlay.classList.add("is-visible"));
    };

    const closeModal = () => {
        loginOverlay.classList.remove("is-visible");
        document.body.classList.remove("modal-open");
        window.setTimeout(() => {
            loginOverlay.hidden = true;
        }, 220);
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
};*/

document.addEventListener("DOMContentLoaded", async () => {


     setupLoginModal();
     setupRegisterModal(() => openModal());
    //tva e v sluchai che neshto go nqma GETtera i za da ne e prazen ekran da go renderne s prazni stoinosti
    //renderDashboard();
    await loadDashboard();
    const publicConnection = connect();

    //ne e  mngoo krasivo
    publicConnection.connection.on("ReceivePublicUpdate", (updates) => {
        if (!Array.isArray(updates)) return;
        handleSignalUpdate(updates);
    })

    await publicConnection.start();

    await publicConnection.joinPublicGroup();
});