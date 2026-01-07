(() => {
    let animIn = null;
    let animOut = null;
    let isSmall = true;
    const element = document.querySelector(".login-left-box");

    if (!element) return

    const animateIn = () => {
        if (!element) return;
        if (!isSmall) return;
        if (animOut) animOut.cancel();

        isSmall = false;
        animIn = element.animate([
            { opacity: 0, width: "0%", offset: 0 },
            { opacity: 0, width: "35%", offset: 0.2 },
            { opacity: 1, width: "100%", offset: 1 }
        ], {
            duration: 1708,
            fill: "forwards",
            easing: "cubic-bezier(.22,.57,.6,.93)"
        });
    };

    const animateOut = () => {
        if (!element) return;
        if (isSmall) return;
        if (animIn) animIn.cancel();

        isSmall = true;
        element.opacity = 0;
        element.width = 0;
    };

    window.addEventListener("load", () => {
        if (window.innerWidth >= 900) {
            animateIn();
        } else if (element) {
            element.style.opacity = 0;
            element.style.width = "0%";
        }
    });

    window.addEventListener("resize", () => {
        if (!element) return;
        if (window.innerWidth >= 900) {
            animateIn();
        } else {
            animateOut();
        }
    });

})();


(() => {
    window.addErrorListener = (formId) => {

        document.getElementById(formId).addEventListener("submit", async function(e) {
            e.preventDefault();

            const formData = new FormData(this);

            try {
                const response = await fetch(this.action, {
                    method: "POST",
                    body: formData
                });

                const result = await response.json();
                clearErrorText()
                if (result.success) {
                    if (result.redirectUrl) window.location.href = result.redirectUrl;
                } else {

                    for (let [key, message] of Object.entries(result.errors)) {

                        if (key == "password") key = "Password";

                        const errElement = document.getElementById(`error-${key}`);
                        errElement.textContent = message;
                    }
                }
            } catch (error) {
                console.log(error);
            }
        });

        function clearErrorText() {
            const errTexts = document.querySelectorAll(".error-text");

            for (const t of errTexts) {
                t.textContent = "";
            }
        }
    }

    addErrorListener("registerForm")
})();

