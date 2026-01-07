

// Start of parallax scroll script

const layers = document.querySelectorAll(".layer");

addEventListener("scroll", () => {
    const y = window.scrollY;
    for (let i = 1; i < layers.length; i++) {
        layers[layers.length - i].style.transform = `translateY(${(i * 0.1) * y}px)`;
    }
})

// Start of CV carousel scroll arrows
var scrollContainerCv = document.getElementsByClassName("scroll-container").item(0);
var rightArrow = document.getElementById("slide-1-arrow-right");

rightArrow.addEventListener("click", () => {
    scrollContainerCv.scrollBy(700, 0);

})

document.getElementById("slide-1-arrow-left").addEventListener("click", () => {
    scrollContainerCv.scrollBy(-700, 0);
})

