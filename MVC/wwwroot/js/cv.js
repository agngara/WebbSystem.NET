const cv = (function () {
    let workExpCount = 0;
    let educationCount = 0;
    let competenceCount = 0;
    let profileImageHandler = null;

    function showAlert(type, message, elementId = 'uploadError') {
        const alertDiv = document.getElementById(elementId);
        if (alertDiv) {
            alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
            alertDiv.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            alertDiv.style.display = 'block';

            if (type === 'success') {
                setTimeout(() => {
                    alertDiv.style.display = 'none';
                }, 5000);
            }
        }
    }

    function validateImageFile(file) {
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png'];
        if (!allowedTypes.includes(file.type)) {
            return 'Endast .jpg, .jpeg och .png filer är tillåtna';
        }

        if (file.size > 5 * 1024 * 1024) {
            return 'Filen är för stor. Max 5MB';
        }

        return null;
    }

    return {
        init: function () {
            console.log('CV.js initierad!');

            const cvData = document.getElementById('cvData');
            if (cvData) {
                workExpCount = parseInt(cvData.dataset.workExpCount || 0);
                educationCount = parseInt(cvData.dataset.educationCount || 0);
                competenceCount = parseInt(cvData.dataset.competenceCount || 0);
                console.log('Initial counts:', { workExpCount, educationCount, competenceCount });
            }

            const editModal = document.getElementById('editCvModal');
            if (editModal) {
                editModal.addEventListener('hidden.bs.modal', function () {
                    console.log('CV modal stängd - laddar om sidan');
                    setTimeout(() => {
                        window.location.reload();
                    }, 300);
                });
            }

            this.initImageUpload();

            console.log('CV.js initierad klar!');
        },

        initImageUpload: function () {
            const profileImageFile = document.getElementById('profileImageFile');
            const imagePreview = document.getElementById('imagePreview');
            const uploadImageBtn = document.getElementById('uploadImageBtn');
            const uploadProgressContainer = document.getElementById('uploadProgressContainer');
            const uploadProgressBar = document.getElementById('uploadProgressBar');
            const cvData = document.getElementById('cvData');
            const uploadUrl = cvData?.dataset.uploadUrl || '/Cv/UploadProfileImage';

            if (profileImageFile && imagePreview) {
                profileImageFile.addEventListener('change', function (e) {
                    const file = e.target.files[0];
                    if (file) {
                        const error = validateImageFile(file);
                        if (error) {
                            showAlert('danger', error);
                            return;
                        }

                        const reader = new FileReader();
                        reader.onload = function (e) {
                            imagePreview.src = e.target.result;
                        };
                        reader.readAsDataURL(file);

                        const uploadError = document.getElementById('uploadError');
                        if (uploadError) {
                            uploadError.style.display = 'none';
                        }
                    }
                });
            }

            if (uploadImageBtn) {
                uploadImageBtn.addEventListener('click', () => {
                    this.uploadProfileImage();
                });
            }

            const uploadModal = document.getElementById('uploadImageModal');
            if (uploadModal) {
                uploadModal.addEventListener('shown.bs.modal', () => {
                    const uploadError = document.getElementById('uploadError');
                    if (uploadError) {
                        uploadError.style.display = 'none';
                    }
                });
            }
        },

        uploadProfileImage: function () {
            const fileInput = document.getElementById('profileImageFile');
            const file = fileInput.files[0];
            const uploadProgressContainer = document.getElementById('uploadProgressContainer');
            const uploadProgressBar = document.getElementById('uploadProgressBar');
            const cvData = document.getElementById('cvData');
            const uploadUrl = cvData?.dataset.uploadUrl || '/Cv/UploadProfileImage';

            if (!file) {
                showAlert('danger', 'Välj en bild först');
                return;
            }

            const error = validateImageFile(file);
            if (error) {
                showAlert('danger', error);
                return;
            }

            if (uploadProgressContainer) {
                uploadProgressContainer.style.display = 'block';
                uploadProgressBar.style.width = '0%';
                uploadProgressBar.textContent = '0%';
            }

            const formData = new FormData();
            formData.append('profileImage', file);

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const xhr = new XMLHttpRequest();

            xhr.upload.addEventListener('progress', function (e) {
                if (e.lengthComputable && uploadProgressBar) {
                    const percentComplete = (e.loaded / e.total) * 100;
                    uploadProgressBar.style.width = percentComplete + '%';
                    uploadProgressBar.textContent = Math.round(percentComplete) + '%';
                }
            });

            xhr.addEventListener('load', function () {
                if (uploadProgressContainer) {
                    uploadProgressContainer.style.display = 'none';
                }

                if (xhr.status === 200) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        if (response.success) {
                            const profileImage = document.getElementById('profileImage');
                            const imagePreview = document.getElementById('imagePreview');

                            if (profileImage) {
                                profileImage.src = response.imageUrl;
                            }
                            if (imagePreview) {
                                imagePreview.src = response.imageUrl;
                            }

                            showAlert('success', response.message || 'Profilbild uppdaterad!');

                            setTimeout(() => {
                                const modal = bootstrap.Modal.getInstance(document.getElementById('uploadImageModal'));
                                if (modal) {
                                    modal.hide();
                                }
                            }, 2000);
                        } else {
                            showAlert('danger', response.message || 'Ett fel uppstod');
                        }
                    } catch (e) {
                        showAlert('danger', 'Ett fel uppstod vid bearbetning av svaret');
                    }
                } else {
                    showAlert('danger', 'Ett fel uppstod vid uppladdning: ' + xhr.statusText);
                }
            });

            xhr.addEventListener('error', function () {
                if (uploadProgressContainer) {
                    uploadProgressContainer.style.display = 'none';
                }
                showAlert('danger', 'Ett nätverksfel uppstod');
            });

            xhr.open('POST', uploadUrl);
            if (token) {
                xhr.setRequestHeader('RequestVerificationToken', token);
            }
            xhr.send(formData);
        },

        addWorkExperience: function () {
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
        },

        addEducation: function () {
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
        },

        addCompetence: function () {
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
                            onclick="cv.removeCompetence(this)">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            `;

            container.insertAdjacentHTML('beforeend', html);
            competenceCount++;
            console.log('Kompetens tillagd. Nytt count:', competenceCount);
        },

        removeCompetence: function (button) {
            console.log('Tar bort kompetens');

            const inputGroup = button.closest('.input-group');
            if (inputGroup) {
                inputGroup.remove();
                console.log('Kompetens borttagen');
            }
        },

        validateCvForm: function () {
            console.log('Validerar CV-formulär...');

            const form = document.getElementById('editCvForm');
            if (!form) return true;

            const requiredInputs = form.querySelectorAll('input[required], textarea[required]');
            let isValid = true;

            for (let input of requiredInputs) {
                if (!input.value.trim()) {
                    input.classList.add('is-invalid');
                    isValid = false;
                } else {
                    input.classList.remove('is-invalid');
                }
            }

            if (!isValid) {
                showAlert('warning', 'Vänligen fyll i alla obligatoriska fält', 'cvValidationError');
                return false;
            }

            console.log('Formulär godkänt!');
            return true;
        },

        listFunctions: function () {
            console.log('=== CV.js Functions ===');
            console.log('1. cv.init() - Initierar CV-funktionalitet');
            console.log('2. cv.addWorkExperience() - Lägger till arbetserfarenhet');
            console.log('3. cv.addEducation() - Lägger till utbildning');
            console.log('4. cv.addCompetence() - Lägger till kompetens');
            console.log('5. cv.removeCompetence(button) - Tar bort kompetens');
            console.log('6. cv.validateCvForm() - Validerar formulär');
            console.log('7. cv.uploadProfileImage() - Laddar upp profilbild');
            console.log('========================');
        },

        getCounts: function () {
            return {
                workExpCount,
                educationCount,
                competenceCount
            };
        }
    };
})();

document.addEventListener('DOMContentLoaded', function () {
    cv.init();
});

window.cv = cv;

console.log('CV.js laddad - Alla funktioner redo!');