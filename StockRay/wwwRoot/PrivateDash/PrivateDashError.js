
export function error(message, onClose) {


    const element = document.createElement("div");

    const container = document.createElement("div");
    container.classList.add("error-text");

    element.className = "error-blimp";

    const title = document.createElement("div");
    title.classList.add("error-title");
    title.textContent = "Something went wrong";

    const messageEl = document.createElement("div");
    messageEl.classList.add("error-message");
    messageEl.textContent = message;

    container.appendChild(title);
    container.appendChild(messageEl);

    const closeBtn = document.createElement("button");
    closeBtn.classList.add("error-close-btn");
    closeBtn.textContent = "Close";


    element.replaceChildren();
    element.appendChild(container);
    element.appendChild(closeBtn);

    closeBtn.addEventListener("click", () => {
        if (onClose) {
            onClose();
        }
    })

    return element;
    

}