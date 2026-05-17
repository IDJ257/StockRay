//OTVRAT

import { error } from "./PrivateDashError.js"
import { connect } from "../SignalRConnect.js"

const symbolsById = new Map();
const cardsById = new Map();
const authMess = "You are not authorized to see the contents of the page Please register or login. Click close for redirect"


const root = document.getElementById("privateDash");

const formatNumber = (value) => {
    if (value === null || value === undefined || Number.isNaN(value)) {
        return "--";
    }
    return Number(value).toFixed(2);
};

const render = () => {
    root.innerHTML = "";

    symbolsById.forEach((symbol, id) => {
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

        cardsById.set(id, card);
        root.appendChild(card);
    });
};

const handleSignalUpdate = (updates) => {


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

const loadPrivateSymbols = async () => {


    const response = await fetch("/GetSymbols", {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("stockrayJWT")}`
        },
    })


    

    if (!response.ok) {

        if (response.status === 401) {


             const err = error(authMess, () => {
                root.innerHTML = "";
                localStorage.removeItem("stockrayJWT");
                window.location.href = "/PublicDash/";
            })

            root.appendChild(err);
            return;
        }

        //error("", () => {
        //    root.innerHTML = "";
        //})
       
    }

    const data = await response.json();

    return data ?? [];

}


document.addEventListener("DOMContentLoaded", async () => {


    const connection = connect();

    const privateSymbols = await loadPrivateSymbols();
    if (!privateSymbols) return;

    privateSymbols.forEach(s => {
        symbolsById.set(s.id, s);
    })

    render();

    await connection.start(handleSignalUpdate);

    const groups = privateSymbols.map(s => s.name);

    await connection.joinGroups(groups);
    

});