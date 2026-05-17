
export function error(message, onClose) {


    const element = document.createElement("div");

    element.className = "error-blimp";


    //ne e mnogo hubavo ama sh svurhsi rabota za sega.
    element.innerHTML = `
      <div class="error-text">
            <div class="error-title">Something went wrong</div>
            <div class="error-message">
               ${message}
            </div>
        </div>

        <button class="error-close-btn">Close</button>
    
    `

    element.addEventListener("click", () => {
        if (onClose) {
            onClose();
        }
    })

    return element;
    

}