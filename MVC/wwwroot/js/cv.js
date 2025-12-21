// MVC/wwwroot/js/cv.js - JavaScript för CV-sidan

// Globala räknare
let workExpCount = 0;
let educationCount = 0;
let competenceCount = 0;

// Initialisering när sidan laddas
document.addEventListener('DOMContentLoaded', function () {
    console.log('CV.js laddad!');

    // Hämta initiala värden från data-attribut
    const cvData = document.getElementById('cvData');
    if (cvData) {
        workExpCount = parseInt(cvData.dataset.workExpCount || 0);
        educationCount = parseInt(cvData.dataset.educationCount || 0);
        competenceCount = parseInt(cvData.dataset.competenceCount || 0);
        console.log('Initial counts:', { workExpCount, educationCount, competenceCount });
    }

    // Event listener för när modalen stängs
    const editModal = document.getElementById('editCvModal');
    if (editModal) {
        editModal.addEventListener('hidden.bs.modal', function () {
            console.log('Modal stängd - laddar om sidan');
            setTimeout(() => {
                window.location.reload();
            }, 300);
        });
    }

    // Event listener för formulärets submit (valfritt - AJAX)
    const editForm = document.getElementById('editCvForm');
    if (editForm) {
        editForm.addEventListener('submit', function (e) {
            console.log('Formulär submit');
            // Här kan du lägga till AJAX om du vill
        });
    }

    // Testa att alla knappar fungerar
    console.log('CV.js initierad klar!');
});

// Lägg till arbetserfarenhet
function addWorkExperience() {
    console.log('Lägger till arbetserfarenhet #' + workExpCount);

    const container = document.getElementById('workExperiencesContainer');
    if (!container) {
        console.error('Kunde inte hitta workExperiencesContainer!');
        return;
    }

    const html = `
        <div class="work-experience-entry border p-3 mb-3 rounded">
            <div class="row g-2">
                <div class="col-md-6">
                    <input type="text" class="form-control" 
                           name="WorkExperiences[${workExpCount}].Title" 
                           placeholder="Titel" required>
                </div>
                <div class="col-md-6">
                    <input type="text" class="form-control" 
                           name="WorkExperiences[${workExpCount}].WorkPlace.Name" 
                           placeholder="Företag">
                </div>
                <div class="col-md-6">
                    <input type="date" class="form-control" 
                           name="WorkExperiences[${workExpCount}].StartDate">
                </div>
                <div class="col-md-6">
                    <input type="date" class="form-control" 
                           name="WorkExperiences[${workExpCount}].EndDate">
                </div>
                <div class="col-12">
                    <textarea class="form-control" rows="2"
                              name="WorkExperiences[${workExpCount}].Description"
                              placeholder="Beskrivning"></textarea>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', html);
    workExpCount++;
    console.log('Arbetserfarenhet tillagd. Nytt count:', workExpCount);
}

// Lägg till utbildning
function addEducation() {
    console.log('Lägger till utbildning #' + educationCount);

    const container = document.getElementById('educationsContainer');
    if (!container) {
        console.error('Kunde inte hitta educationsContainer!');
        return;
    }

    const html = `
        <div class="education-entry border p-3 mb-3 rounded">
            <div class="row g-2">
                <div class="col-md-6">
                    <input type="text" class="form-control" 
                           name="Educations[${educationCount}].ProgramOrCourseName" 
                           placeholder="Program/Kursnamn" required>
                </div>
                <div class="col-md-6">
                    <input type="text" class="form-control" 
                           name="Educations[${educationCount}].Institution.Name" 
                           placeholder="Skola/Universitet">
                </div>
                <div class="col-md-6">
                    <input type="date" class="form-control" 
                           name="Educations[${educationCount}].StartDate">
                </div>
                <div class="col-md-6">
                    <input type="date" class="form-control" 
                           name="Educations[${educationCount}].EndDate">
                </div>
                <div class="col-12">
                    <textarea class="form-control" rows="2"
                              name="Educations[${educationCount}].Description"
                              placeholder="Beskrivning"></textarea>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', html);
    educationCount++;
    console.log('Utbildning tillagd. Nytt count:', educationCount);
}

// Lägg till kompetens
function addCompetence() {
    console.log('Lägger till kompetens #' + competenceCount);

    const container = document.getElementById('competencesContainer');
    if (!container) {
        console.error('Kunde inte hitta competencesContainer!');
        return;
    }

    const html = `
        <div class="input-group mb-2">
            <input type="text" class="form-control" 
                   name="Competences[${competenceCount}].Name" 
                   placeholder="Kompetens" required>
            <button type="button" class="btn btn-outline-danger" 
                    onclick="removeCompetence(this)">
                <i class="bi bi-trash"></i>
            </button>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', html);
    competenceCount++;
    console.log('Kompetens tillagd. Nytt count:', competenceCount);
}

// Ta bort kompetens
function removeCompetence(button) {
    console.log('Tar bort kompetens');

    const inputGroup = button.closest('.input-group');
    if (inputGroup) {
        inputGroup.remove();
        console.log('Kompetens borttagen');
    }
}

// Validera formuläret innan submit (valfritt)
function validateCvForm() {
    console.log('Validerar CV-formulär...');

    const form = document.getElementById('editCvForm');
    if (!form) return true;

    // Här kan du lägga till extra validering om du vill
    console.log('Formulär godkänt!');
    return true;
}

// Visa alert-meddelande (valfritt - för framtida användning)
function showCvAlert(type, message) {
    console.log(`CV Alert [${type}]: ${message}`);

    // Skapa eller uppdatera alert-element
    let alertDiv = document.getElementById('cvAlert');
    if (!alertDiv) {
        alertDiv = document.createElement('div');
        alertDiv.id = 'cvAlert';
        alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
        alertDiv.role = 'alert';

        const closeButton = document.createElement('button');
        closeButton.type = 'button';
        closeButton.className = 'btn-close';
        closeButton.setAttribute('data-bs-dismiss', 'alert');
        closeButton.setAttribute('aria-label', 'Close');

        alertDiv.appendChild(document.createTextNode(message + ' '));
        alertDiv.appendChild(closeButton);

        // Lägg till i modalen
        const modalBody = document.querySelector('.modal-body');
        if (modalBody) {
            modalBody.insertBefore(alertDiv, modalBody.firstChild);
        }
    } else {
        alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
        alertDiv.querySelector('.alert-message').textContent = message;
    }
}

// Debug-funktion för att lista alla funktioner
function listCvFunctions() {
    console.log('=== CV.js Functions ===');
    console.log('1. addWorkExperience() - Lägger till arbetserfarenhet');
    console.log('2. addEducation() - Lägger till utbildning');
    console.log('3. addCompetence() - Lägger till kompetens');
    console.log('4. removeCompetence(button) - Tar bort kompetens');
    console.log('5. validateCvForm() - Validerar formulär');
    console.log('6. showCvAlert(type, message) - Visar alert');
    console.log('========================');
}

// Gör funktionerna globalt tillgängliga
window.addWorkExperience = addWorkExperience;
window.addEducation = addEducation;
window.addCompetence = addCompetence;
window.removeCompetence = removeCompetence;
window.validateCvForm = validateCvForm;
window.showCvAlert = showCvAlert;
window.listCvFunctions = listCvFunctions;

console.log('CV.js laddad - Alla funktioner redo!');