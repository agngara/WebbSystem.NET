// Projects scroll arrows

var scrollContainerProject = document.getElementsByClassName("scroll-container").item(0);
var rightArrow = document.getElementById("slide-1-arrow-right-2");

rightArrow.addEventListener("click", () => {
    scrollContainerProject.scrollBy(700, 0);

})

document.getElementById("slide-1-arrow-left-2").addEventListener("click", () => {
    scrollContainerProject.scrollBy(-700, 0);
})

// Add project popup window

const card = document.getElementById("new-project-card");
const addProjectWindow = document.getElementById("create-project-popup");

card.addEventListener("click", () => {
    addProjectWindow.style.visibility = "visible";
})

var closeButton = document.getElementById("close-add-window");

closeButton.addEventListener("click", () => {
    addProjectWindow.style.visibility = "hidden";
})

// Edit project popup window


const editCard = document.getElementById("edit-project-card");
const editProjectWindow = document.getElementById("edit-project-popup");

card.addEventListener("click", () => {
    addProjectWindow.style.visibility = "visible";
})

var closeButton = document.getElementById("close-edit-window");

closeButton.addEventListener("click", () => {
    addProjectWindow.style.visibility = "hidden";
})
