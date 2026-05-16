const grid = document.getElementById("dashboardGrid");

let symbols = [];
const symbolsById = new Map();

const formatNumber = (value) => {
    if (value === null || value === undefined || Number.isNaN(value)) {
        return "--";
    }

    return Number(value).toFixed(2);
};

const normalizeSymbol = (raw) => ({
    id: raw.id ?? raw.Id,
    name: raw.name ?? raw.Name ?? "Unknown",
    open: raw.open ?? raw.Open,
    high: raw.high ?? raw.High,
    low: raw.low ?? raw.Low,
    currentPrice: raw.currentPrice ?? raw.CurrentPrice
});

const renderDashboard = () => {
    grid.innerHTML = "";

    const items = symbols;
    while (items.length < 9) {
        items.push({
            id: `placeholder-${items.length}`,
            name: "Loading...",
            open: null,
            high: null,
            low: null,
            currentPrice: null,
            placeholder: true
        });
    }

    items.forEach((symbol, index) => {
        const card = document.createElement("div");
        card.className = "bubble" + (symbol.placeholder ? " placeholder" : "");
        card.innerHTML = `
                    <h3>${symbol.name}</h3>
                    <div class="micro-grid">
                        <div class="micro-pill open">Open <span>${formatNumber(symbol.open)}</span></div>
                        <div class="micro-pill current">Current <span>${formatNumber(symbol.currentPrice)}</span></div>
                        <div class="micro-pill high">High <span>${formatNumber(symbol.high)}</span></div>
                        <div class="micro-pill low">Low <span>${formatNumber(symbol.low)}</span></div>
                    </div>
                `;

        grid.appendChild(card);

        if (index === 2) {
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
        const rawSymbols = payload.symbols ?? payload.Symbols ?? [];
        symbols = rawSymbols.map(normalizeSymbol);
        symbolsById.clear();
        symbols.forEach((symbol) => symbolsById.set(symbol.id, symbol));

        renderDashboard();
    } catch {
        // ignore - landing page will keep placeholders
    }
};

const startHub = async () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/sym-notif")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceivePublicUpdate", (updates) => {
        if (!Array.isArray(updates)) {
            return;
        }

        updates.forEach((update) => {
            const normalized = normalizeSymbol(update);
            if (!symbolsById.has(normalized.id)) {
                return;
            }

            const existing = symbolsById.get(normalized.id);
            existing.open = normalized.open;
            existing.high = normalized.high;
            existing.low = normalized.low;
            existing.currentPrice = normalized.currentPrice;
        });

        renderDashboard();
    });

    await connection.start();
    await connection.invoke("JoinGroup", "Public");
};

document.addEventListener("DOMContentLoaded", async () => {
    renderDashboard();
    await loadDashboard();
    await startHub();
});