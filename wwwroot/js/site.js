var createRoomButton = document.getElementById("create-room-btn");
var createRoomModal = document.getElementById("create-room-modal");

createRoomButton.addEventListener("click", openModal);

function openModal() {
    createRoomModal.classList.add("active");
}

function closeModal() {
    createRoomModal.classList.remove("active");
}
