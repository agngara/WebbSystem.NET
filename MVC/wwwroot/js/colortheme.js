(() => {
    const btn = document.getElementById("color-theme");
    const body = document.documentElement;
    //loadingggg from storage
    const savedTheme = localStorage.getItem("theme");

    savedTheme == null ? initColor("light") : initColor(savedTheme);

    btn.addEventListener("click", () => {
        if (body.classList.contains("light-mode")) setDark();
        else setLight();
    });


    function initColor(color) {
        color == "light" ? setLight() : setDark();
    }

    function setLight() {
        body.classList.add("light-mode")
        localStorage.setItem("theme", "light");
        btn.src = "/icons/Dark.svg"
    }
    function setDark() {
        body.classList.remove("light-mode")
        localStorage.setItem("theme", "dark");
        btn.src = "/icons/Light.svg"
    }
})()
