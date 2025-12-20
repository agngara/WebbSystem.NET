
let animIn = null;
let animOut = null;
let isSmall = true;
const element = document.querySelector(".login-left-box");

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

