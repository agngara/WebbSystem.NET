(() => {
    'use strict';

    class CVManager {
        constructor() {
            this.initDynamicForms();
            this.initImagePreview();
            this.initValidation();
            this.initEditToggle();
        }

        initDynamicForms() {
            document.addEventListener('click', (e) => {
                if (e.target.classList.contains('add-dynamic-row')) {
                    this.addDynamicRow(e.target);
                }

                if (e.target.classList.contains('remove-dynamic-row')) {
                    this.removeDynamicRow(e.target);
                }
            });
        }

        initImagePreview() {
            const imageInput = document.querySelector('input[type="file"][accept="image/*"]');
            if (imageInput) {
                imageInput.addEventListener('change', (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        const reader = new FileReader();
                        reader.onload = (e) => {
                            const preview = document.getElementById('image-preview');
                            if (preview) {
                                preview.src = e.target.result;
                                preview.style.display = 'block';
                            }
                        };
                        reader.readAsDataURL(file);
                    }
                });
            }
        }

        initValidation() {
            const forms = document.querySelectorAll('.needs-validation');
            forms.forEach(form => {
                form.addEventListener('submit', (e) => {
                    if (!form.checkValidity()) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                    form.classList.add('was-validated');
                }, false);
            });
        }

        initEditToggle() {
            const cvContainer = document.getElementById('cvContainer');
            const editBtn = document.getElementById('editCvBtn');
            const saveBtn = document.getElementById('saveCvBtn');
            const cancelBtn = document.getElementById('cancelCvBtn');

            if (!cvContainer) return;

            if (editBtn) {
                editBtn.addEventListener('click', () => {
                    cvContainer.classList.remove('cv-view-mode');
                    cvContainer.classList.add('cv-edit-mode');
                });
            }

            if (saveBtn) {
                saveBtn.addEventListener('click', () => {
                    this.saveCV();
                    cvContainer.classList.remove('cv-edit-mode');
                    cvContainer.classList.add('cv-view-mode');
                });
            }

            if (cancelBtn) {
                cancelBtn.addEventListener('click', () => {
                    cvContainer.classList.remove('cv-edit-mode');
                    cvContainer.classList.add('cv-view-mode');
                });
            }
        }

        saveCV() {
            console.log('CV sparad');
        }

        addDynamicRow(button) {
            const container = button.closest('.dynamic-container');
            const template = container.querySelector('.dynamic-template');
            const newRow = template.cloneNode(true);

            newRow.classList.remove('dynamic-template');
            newRow.classList.add('dynamic-row');
            newRow.style.display = 'block';

            const index = container.querySelectorAll('.dynamic-row').length;
            const inputs = newRow.querySelectorAll('input, select, textarea');

            inputs.forEach(input => {
                const name = input.getAttribute('name');
                if (name) {
                    const newName = name.replace(/\[\d+\]/, `[${index}]`);
                    input.setAttribute('name', newName);
                }

                if (input.type !== 'hidden') {
                    input.value = '';
                }
            });

            container.insertBefore(newRow, button);
        }

        removeDynamicRow(button) {
            const row = button.closest('.dynamic-row');
            if (row) {
                row.remove();
                this.renumberRows(button.closest('.dynamic-container'));
            }
        }

        renumberRows(container) {
            const rows = container.querySelectorAll('.dynamic-row');
            rows.forEach((row, index) => {
                const inputs = row.querySelectorAll('input, select, textarea');
                inputs.forEach(input => {
                    const name = input.getAttribute('name');
                    if (name) {
                        const newName = name.replace(/\[\d+\]/, `[${index}]`);
                        input.setAttribute('name', newName);
                    }
                });
            });
        }

        exportAsPDF() {
            console.log('Exporting CV as PDF...');
        }

        exportAsXML() {
            console.log('Exporting CV as XML...');
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        window.cvManager = new CVManager();
    });


    window.handleFormSubmit = function(formId) {
        const form = document.getElementById(formId);
        if (!form) {
            console.error(`Form with id "${formId}" not found`);
            return;
        }

        form.addEventListener("submit", async function(e) {
            e.preventDefault();

            const formData = new FormData(form);

            try {
                await fetch(form.action, {
                    method: 'POST',
                    body: formData
                });
                // NotifyUI interceptor handles the response
            } catch (error) {
                console.error('Form submission error:', error);
                ui.showError('An error occurred submitting the form');
            }
        });
    };

})();
