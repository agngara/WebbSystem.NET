// Start of Projects scroll arrow

var scrollContainerProject = document.getElementsByClassName("scroll-container").item(0);
var rightArrow = document.getElementById("slide-1-arrow-right-2");

rightArrow.addEventListener("click", () => {
    scrollContainerProject.scrollBy(700, 0);

})

document.getElementById("slide-1-arrow-left-2").addEventListener("click", () => {
    scrollContainerProject.scrollBy(-700, 0);
})



