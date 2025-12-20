// cv.js - JavaScript för CV-redigering med Bootstrap modal

// Globala räknare som håller koll på antal objekt
let workExpCount = 0;
let educationCount = 0;
let competenceCount = 0;

// Initialisering när sidan laddas
document.addEventListener('DOMContentLoaded', function () {
    // Hämta initiala värden från data-attribut
    const cvData = document.getElementById('cvData');
    if (cvData) {
        workExpCount = parseInt(cvData.dataset.workExpCount || 0);
        educationCount = parseInt(cvData.dataset.educationCount || 0);
        competenceCount = parseInt(cvData.dataset.competenceCount || 0);
    }

    // Event listener för när modalen stängs
    const editModal = document.getElementById('editCvModal');
    if (editModal) {
        editModal.addEventListener('hidden.bs.modal', function () {
            // Ladda om sidan för att visa uppdaterat CV
            setTimeout(() => {
                window.location.reload();
            }, 500);
        });
    }

    // Event listener för formulärets submit (AJAX)
    const editForm = document.getElementById('editCvForm');
    if (editForm) {
        editForm.addEventListener('submit', handleFormSubmit);
    }
});

// Lägg till arbetserfarenhet
function addWorkExperience() {
    const container = document.getElementById('workExperiencesContainer');
    if (!container) return;

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
                <div class="col-12 text-end">
                    <button type="button" class="btn btn-sm btn-outline-danger" 
                            onclick="removeWorkExperience(this)">
                        <i class="bi bi-trash"></i> Ta bort
                    </button>
                </div>
            </div>
        </div>
    `;
    container.insertAdjacentHTML('beforeend', html);
    workExpCount++;
}

// Ta bort arbetserfarenhet
function removeWorkExperience(button) {
    const entry = button.closest('.work-experience-entry');
    if (entry) {
        entry.remove();
    }
}

// Lägg till utbildning
function addEducation() {
    const container = document.getElementById('educationsContainer');
    if (!container) return;

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
                <div class="col-12 text-end">
                    <button type="button" class="btn btn-sm btn-outline-danger" 
                            onclick="removeEducation(this)">
                        <i class="bi bi-trash"></i> Ta bort
                    </button>
                </div>
            </div>
        </div>
    `;
    container.insertAdjacentHTML('beforeend', html);
    educationCount++;
}

// Ta bort utbildning
function removeEducation(button) {
    const entry = button.closest('.education-entry');
    if (entry) {
        entry.remove();
    }
}

// Lägg till kompetens
function addCompetence() {
    const container = document.getElementById('competencesContainer');
    if (!container) return;

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
}

// Ta bort kompetens
function removeCompetence(button) {
    const inputGroup = button.closest('.input-group');
    if (inputGroup) {
        inputGroup.remove();
    }
}

// Hantera formulärets submit (AJAX)
async function handleFormSubmit(event) {
    event.preventDefault();

    const form = event.target;
    const formData = new FormData(form);
    const submitButton = form.querySelector('button[type="submit"]');
    const originalButtonText = submitButton.innerHTML;

    try {
        // Visa laddningsindikator
        submitButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Sparar...';
        submitButton.disabled = true;

        // Skicka AJAX-förfrågan
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        const result = await response.json();

        if (result.success) {
            // Visa framgångsmeddelande
            showAlert('success', result.message || 'CV uppdaterat framgångsrikt!');

            // Stäng modalen efter kort delay
            setTimeout(() => {
                const modal = bootstrap.Modal.getInstance(document.getElementById('editCvModal'));
                if (modal) {
                    modal.hide();
                }
            }, 1500);
        } else {
            // Visa felmeddelanden
            if (result.errors) {
                showAlert('danger', 'Vänligen korrigera felen i formuläret.');
            } else {
                showAlert('danger', result.error || 'Ett fel uppstod vid sparandet.');
            }
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('danger', 'Ett nätverksfel uppstod. Försök igen.');
    } finally {
        // Återställ knappen
        submitButton.innerHTML = originalButtonText;
        submitButton.disabled = false;
    }
}

// Visa alert-meddelande
function showAlert(type, message) {
    // Ta bort tidigare alerts
    const existingAlert = document.querySelector('.cv-alert');
    if (existingAlert) {
        existingAlert.remove();
    }

    // Skapa ny alert
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show cv-alert" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Stäng"></button>
        </div>
    `;

    // Lägg till alert i modalen
    const modalBody = document.querySelector('.modal-body');
    if (modalBody) {
        modalBody.insertAdjacentHTML('afterbegin', alertHtml);

        // Ta bort alert automatiskt efter 5 sekunder
        setTimeout(() => {
            const alert = document.querySelector('.cv-alert');
            if (alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    }
}

// Validera datum (slutdatum ska vara efter startdatum)
function validateDates(startDateId, endDateId) {
    const startDate = document.getElementById(startDateId)?.value;
    const endDate = document.getElementById(endDateId)?.value;

    if (startDate && endDate) {
        const start = new Date(startDate);
        const end = new Date(endDate);

        if (end < start) {
            showAlert('warning', 'Slutdatum måste vara efter startdatum.');
            return false;
        }
    }
    return true;
}

// Exportera funktioner till global scope
window.addWorkExperience = addWorkExperience;
window.removeWorkExperience = removeWorkExperience;
window.addEducation = addEducation;
window.removeEducation = removeEducation;
window.addCompetence = addCompetence;
window.removeCompetence = removeCompetence;