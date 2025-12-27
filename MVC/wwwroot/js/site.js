(() => {
    class NotifyUI {
        #element
        #timeout
        #mainElement

        constructor() {
            this.#element = null;
            this.#timeout = null;
            this.#mainElement = document.querySelector(".content-wrapper");
        }

        showSuccess(message) {
            this.createElement(message, "notify-success notification", "/icons/Check.svg")
        }

        showError(message) {
            this.createElement(message, "notify-error notification", "/icons/Error.svg")
        }

        createElement(message, cssClass, imgUrl) {
            if (this.#element) {
                this.#element.remove();
                this.#element = null
                clearTimeout(this.#timeout)
            }
            console.log("cc")

            const div = document.createElement("div");
            div.classList = cssClass

            const icon = document.createElement("img")
            icon.src = imgUrl
            div.appendChild(icon)

            const text = document.createElement("p")
            text.textContent = message
            div.appendChild(text)

            this.#mainElement.appendChild(div)
            this.#element = div;

            this.#timeout = setTimeout(() => this.#element.style.opacity = 0, 3000)
        }

    }

    const originalFetch = window.fetch;
    const ui = new NotifyUI()

    window.fetch = function(...args) {
        const [url, options] = args;


        if (options && options.method && options.method.toUpperCase() === "POST") {
            return originalFetch.apply(this, args)
                .then(async response => {
                    try {

                        const clonedResponse = response.clone();

                        const contentType = response.headers.get('content-type');
                        if (contentType && contentType.includes('application/json')) {
                            const data = await clonedResponse.json();
                            console.log(data)

                            //our api Structure
                            if (data && typeof data === 'object') {

                                if (data.notify === true) {
                                    if (data.success) {
                                        ui.showSuccess(data.message || "Success");
                                    } else {
                                        ui.showError(data.message || "Error");
                                    }
                                }

                                if (data.RedirectUrl) {
                                    setTimeout(() => {
                                        window.location.href = data.RedirectUrl;
                                    }, data.Notify ? 1000 : 0);
                                }
                            }
                        }
                    } catch (error) {
                        console.debug('Non-JSON response or parse error:', error);
                    }

                    return response;
                })
                .catch(error => {
                    console.error('Fetch error:', error);
                    throw error;
                });
        }

        // Pass through if not POST
        return originalFetch.apply(this, args);
    };
})();


