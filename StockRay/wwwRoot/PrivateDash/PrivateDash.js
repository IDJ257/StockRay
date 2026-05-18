//OTVRAT

import { error } from "./PrivateDashError.js"
import { connect } from "../SignalRConnect.js"
let connection;
const symbolsById = new Map();
const cardsById = new Map();
const authMess = "You are not authorized to see the contents of the page Please register or login. Click close for redirect"

//da se populatne sus simvolite koito vechce usera-ima
const selectedForAdd = new Set(); //za sega e global ama nqqma da e



const root = document.getElementById("privateDash");

const formatNumber = (value) => {
    if (value === null || value === undefined || Number.isNaN(value)) {
        return "--";
    }
    return Number(value).toFixed(2);
};

const getAllSymbols = async () => {

    try {
        const response = await fetch("/GetAllSymbols");

        if (!response.ok) {
            //sh vidim
        }

        const data = response.json();


        return data;
    } catch (e) {

    }



}

const buildItems = (symbol) => {

    const li = document.createElement("li");

    li.classList.add("item");

    const name = document.createElement("span");
    name.classList.add("item-name");
    name.textContent = symbol.name;

    const checkBox = document.createElement("input");
    checkBox.type = "checkbox";

    checkBox.addEventListener("click", (e) => {
        e.stopPropagation();
    });

    checkBox.addEventListener("change", (e) => {
        if (e.target.checked) {
            selectedForAdd.add(symbol.id);
        } else {
            selectedForAdd.delete(symbol.id);
        }
    });

    li.addEventListener("click", () => {
        checkBox.checked = !checkBox.checked;
        checkBox.dispatchEvent(new Event("change"));
    });

    li.append(name, checkBox);

    return li;


}

const openAllStocks = async () => {

    try {
        //moje da ne se chaka a dokato se buildva da se runne ama kak i dae.
        const modal = document.getElementById("modal");
        const fragmentContainer = document.createDocumentFragment();
        const ul = document.getElementById("itemList");
        const list = await getAllSymbols();

        list.forEach((symbol) => {
            const listItem = buildItems(symbol)

            fragmentContainer.appendChild(listItem);
        });

        ul.replaceChildren(fragmentContainer);


        modal.classList.remove("hidden")

    } catch (e) {

    }





    //kum itemList da appendvam
}

const closeAllStocks = () => {
    const modal = document.getElementById("modal");
    const ul = document.getElementById("itemList");

    modal.classList.add("hidden");
    selectedForAdd.clear();
    const checkboxes = ul.querySelectorAll("input[type='checkbox']");
    checkboxes.forEach(cb => cb.checked = false);
}

const addSymbols = async () => {

    const root = document.getElementById("stocks");

    try {
        const response = await fetch("/AddSymbol", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("stockrayJWT")}`
            },
            body: JSON.stringify({
                symbolIds: [...selectedForAdd]
            })
        });


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

        data.forEach((s => {

            symbolsById.set(s.id, s);

            const card = createCard(s);

            cardsById.set(s.id, card)

            root.appendChild(card);


        }));

        //CONNECTTTION

        const groups = data.map(s => s.name);

        await connection.joinGroups(groups);

        closeAllStocks();


     

    } catch (e) {

    }



}

const removeSymbol = async (removeSymbol) => {

    const root = document.getElementById("stocks");


    try {

        const response = await fetch("/RemoveSymbol", {
            method: "DELETE",
            headers: {
                "Content-type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("stockrayJWT")}`
            },
            body: JSON.stringify({
                symbolIds: [removeSymbol.id]
            })
        })

        if (!response.ok) {
            //error
        }

        await connection.leaveGroup(removeSymbol.name);

        root.removeChild(cardsById.get(removeSymbol.id));
        

    } catch (e) {
        console.log(e);
    }
   


}


const buildAddBubble = () => {

    const bubbleAdd = document.createElement("div");
    bubbleAdd.className = "bubble-add";


    const bubbleContent = document.createElement("div");
    bubbleContent.className = "bubble-add-content";
    bubbleContent.textContent = "+";

    bubbleAdd.addEventListener("click", () => {
        openAllStocks();
    });


    bubbleAdd.appendChild(bubbleContent);


    return bubbleAdd;
}

const createCard = (symbol) => {
    const card = document.createElement("div");
    card.className = "bubble";


    const btn = document.createElement("button");
    btn.className = "bubble-close";
    btn.textContent = "X";
    card.appendChild(btn);


    const title = document.createElement("h3");
    title.textContent = symbol.name;
    card.appendChild(title);


    const grid = document.createElement("div");
    grid.className = "micro-grid";

    const price = document.createElement("div");
    price.className = "current-price";
    price.textContent = formatNumber(symbol.currentPrice);
    grid.appendChild(price);

    function makePill(label, value, className) {
        const pill = document.createElement("div");
        pill.className = `micro-pill ${className}`;

        const text = document.createTextNode(`${label} `);
        const span = document.createElement("span");
        span.textContent = formatNumber(value);

        pill.appendChild(text);
        pill.appendChild(span);

        return pill;
    }

    grid.appendChild(makePill("Open", symbol.open, "open"));
    grid.appendChild(makePill("High", symbol.high, "high"));
    grid.appendChild(makePill("Low", symbol.low, "low"));

    card.appendChild(grid);

    btn.addEventListener("click", (e) => {
        
        removeSymbol(symbol);
    })

    return card;
}

const render = () => {

    const root = document.getElementById("stocks");

    root.appendChild(buildAddBubble());

    symbolsById.forEach((symbol, key) => {

        const card = createCard(symbol);

        cardsById.set(key, card);

        root.appendChild(card);
    });



};


//duplicate, shte se pravi
const handleSignalUpdate = (updates) => {

    //moje da se spre da se piolzva symbolsById
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

    const root = document.getElementById("mainRoot");

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

document.getElementById("cancelBtn").addEventListener("click", closeAllStocks);
document.getElementById("addBtn").addEventListener("click", addSymbols);

document.addEventListener("DOMContentLoaded", async () => {


    connection = connect();
    
    connection.connection.on("ReceiveGroupUpdate", (updates) => {

        if (!Array.isArray(updates)) return;

        handleSignalUpdate(updates);
    });

    await connection.start();


    const privateSymbols = await loadPrivateSymbols();
    if (!privateSymbols) return;

    privateSymbols.forEach(s => {
        symbolsById.set(s.id, s);
    })

    render();


    const groups = privateSymbols.map(s => s.name);

    await connection.joinGroups(groups);
});

