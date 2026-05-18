
import { setupLoginModal, openModal } from "../Login/login.js";
import { setupRegisterModal } from "../Register/register.js";
import { connect } from "../SignalRClient/SignalRConnect.js";
import { logout, updateAuthUI } from "../Login/logout.js";

const grid = document.getElementById("dashboardGrid");
const symbolsById = new Map();
const cardsById = new Map();

const formatNumber = (value) => {
    if (value === null || value === undefined || Number.isNaN(value)) {
        return "--";
    }

    return Number(value).toFixed(2);
};


const renderDashboard = () => {
    grid.innerHTML = "";



    //TODO: drop teh string innerHTML cause it's an absolute idiocy.
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
    } catch (e) {
        console.log(e);
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




document.addEventListener("DOMContentLoaded", async () => {


     setupLoginModal();
    setupRegisterModal(() => openModal());
    updateAuthUI();

    const logoutBtn = document.querySelector("[data-logout]");

    logoutBtn?.addEventListener("click", logout);
    
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