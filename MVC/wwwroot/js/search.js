
class Search {
    #searchContainer;
    #searchInput;
    #resultsContainer;
    #results;

    constructor(searchContainer, searchInput, resultsContainer) {
        this.#searchContainer = searchContainer;
        this.#searchInput = searchInput;
        this.#resultsContainer = resultsContainer;
        this.#results = null;
    }

    HookupEvents = () => {
        if (!this.#searchContainer || !this.#searchInput || !this.#resultsContainer) {
            console.log("failed to find search classes");
            return;
        }

        console.log("Found search classes!");
        console.log("Binding Events!");

        document.addEventListener("DOMContentLoaded", () => {
            let debounceTimer;
            this.#searchInput.addEventListener("input", (e) => {
                if (e.target.length === 1) this.displayResults();

                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => {
                    const query = e.target.value.trim();
                    if (query.length > 0) {
                        this.performSearch(query);
                    } else {
                        this.clearResults();
                    }
                }, 150);
            });

            this.#searchInput.addEventListener("focusin", (e) => {
                if (!this.#results) return;

                this.displayResults(this.#results);
            });

            document.addEventListener("click", (e) => {
                if (!this.#searchContainer.contains(e.target)) {
                    this.clearResults();
                }
            });
        });
    }

    performSearch = (query) => {
        console.log("fetching...");
        fetch("/Search/Search", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ query: query })
        })
            .then(response => response.json())
            .then(results => {
                this.displayResults(results);
            });
    }

    displayResults = (results) => {
        this.clearResults();
        this.#results = results;
        console.log(results);

        if (!results || results.length === 0) return;

        this.#resultsContainer.style.display = "block";

        results.forEach(result => {
            const item = document.createElement("div");
            item.classList.add("search-result-item");

            const img = document.createElement("img");
            img.classList.add("search-result-img");
            img.src = result.matchedOn === "Name" ? "/icons/Name.svg" : "/icons/User.svg";
            img.alt = "icon";
            item.appendChild(img);

            const p = document.createElement("p");
            p.classList.add("search-result-name");
            p.textContent = result.displayName;
            item.appendChild(p);

            item.addEventListener("click", () => {
            });

            this.#resultsContainer.appendChild(item);
        });
    }

    clearResults = () => {
        console.log("clearing results...");
        this.#resultsContainer.innerHTML = "";
        this.#resultsContainer.style.display = "none";
    }
}

(() => {
    const searchContainer = document.querySelector(".search-container");
    const searchInput = document.querySelector(".search-input");
    const resultsContainer = document.querySelector(".search-results");

    const search = new Search(searchContainer, searchInput, resultsContainer);
    search.HookupEvents();

})()

