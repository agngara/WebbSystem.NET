// Start of landing video scroll script

//const section = document.querySelector("section.homepage-landing");
//const video = section.querySelector("video");

//video.pause();

//const scroll = () => {
//    const distance = window.scrollY - section.offsetTop;
//    const totalDistance = section.clientHeight - window.innerHeight;

//    let percentage = distance / totalDistance;
//    percentage = Math.max(0, percentage)
//    percentage = Math.min(percentage, 1)

//    if (video.duration > 0) {
//        video.currentTime = video.duration * percentage;
//    }
//}

//scroll()
//window.addEventListener("scroll", scroll)


// End of landing video scroll script

const layers = document.querySelectorAll(".layer");

addEventListener("scroll", () => {
    const y = window.scrollY;
    for (let i = 1; i < layers.length; i++) {
        layers[layers.length - i].style.transform = `translateY(${(i * 0.1) * y}px)`;
    }
})

// Start of card carousel scroll arrows
var scrollContainerCv = document.getElementsByClassName("scroll-container").item(0);
var rightArrow = document.getElementById("slide-1-arrow-right");

rightArrow.addEventListener("click", () => {
    scrollContainerCv.scrollBy(700, 0);

})

document.getElementById("slide-1-arrow-left").addEventListener("click", () => {
    scrollContainerCv.scrollBy(-700, 0);
})

var scrollContainerCv = document.getElementsByClassName("scroll-container").item(1);
var rightArrow = document.getElementById("slide-1-arrow-right-2");

rightArrow.addEventListener("click", () => {
    scrollContainerCv.scrollBy(700, 0);

})

document.getElementById("slide-1-arrow-left-2").addEventListener("click", () => {
    scrollContainerCv.scrollBy(-700, 0);
})
// End of scroll arrows



