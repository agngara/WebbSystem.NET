// Textformatter for cutting off long card-texts
var maxCharLength = 100;

const descriptions = document.querySelectorAll(".card-text");
const descriptionscount = descriptions.length;

for (let i = 0; i < descriptionscount; i++) {
    const textLength = descriptions[i].textContent.length;

    if (textLength > maxCharLength) {
        descriptions[i].textContent = descriptions[i].textContent.slice(0, maxCharLength) + "...";
    }
}

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

document.getElementById("slide-1-arrow-right").addEventListener("click", () => {
    scrollContainerCv.scrollBy(700, 0);

})

document.getElementById("slide-1-arrow-left").addEventListener("click", () => {
    scrollContainerCv.scrollBy(-700, 0);
})

// Start of project carousel scroll arrows
const scrollContainerProject = document.getElementById("project-scroll-container");

document.getElementById("slide-1-arrow-right-2").addEventListener("click", () => {
    scrollContainerProject.scrollBy(700, 0);
})

document.getElementById("slide-1-arrow-left-2").addEventListener("click", () => {
    scrollContainerProject.scrollBy(-700, 0);

})


