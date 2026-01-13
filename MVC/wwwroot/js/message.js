function toggleDelete(id) {
    const form = document.getElementById('delete-form-' + id);
    form.classList.toggle('hidden');
}


function toggleDeleteGuest(id) {
    const form = document.getElementById('delete-guest-form-' + id);
   
    form.classList.toggle('hidden');
}

