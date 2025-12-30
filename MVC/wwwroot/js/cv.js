// cv.js
(() => {
    'use strict';

    class CVManager {
        constructor() {
            this.initDynamicForms();
            this.initImagePreview();
            this.initValidation();
        }

        initDynamicForms() {
            // Dynamiskt lägga till/ta bort rader för kompetenser, utbildningar, erfarenheter
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
            // Lägg till real-time validering
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

        addDynamicRow(button) {
            const container = button.closest('.dynamic-container');
            const template = container.querySelector('.dynamic-template');
            const newRow = template.cloneNode(true);

            newRow.classList.remove('dynamic-template');
            newRow.classList.add('dynamic-row');
            newRow.style.display = 'block';

            // Uppdatera index för alla inputs
            const index = container.querySelectorAll('.dynamic-row').length;
            const inputs = newRow.querySelectorAll('input, select, textarea');

            inputs.forEach(input => {
                const name = input.getAttribute('name');
                if (name) {
                    const newName = name.replace(/\[\d+\]/, `[${index}]`);
                    input.setAttribute('name', newName);
                }

                // Nollställ värden
                if (input.type !== 'hidden') {
                    input.value = '';
                }
            });

            // Lägg till raden före knappen
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

        // Export CV som PDF (för VG-krav)
        exportAsPDF() {
            // Implementera PDF-export här
            console.log('Exporting CV as PDF...');
            // Använd jsPDF eller liknande bibliotek
        }

        // Spara CV som XML (för VG-krav)
        exportAsXML() {
            // Implementera XML-export här
            console.log('Exporting CV as XML...');
            // Använd XMLSerializer
        }
    }

    // Initialisera när sidan laddas
    document.addEventListener('DOMContentLoaded', () => {
        window.cvManager = new CVManager();
    });
})();