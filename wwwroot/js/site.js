// Create new room modal
var createRoomButton = document.getElementById("create-room-btn");
var createRoomModal = document.getElementById("create-room-modal");
createRoomButton.addEventListener("click", openModal);

function openModal() {
    createRoomModal.classList.add("active");
}

function closeModal() {
    createRoomModal.classList.remove("active");
}

// Textarea - expand vertically on text input, max 5 lines
var chatInputTextarea = document.getElementById("chat-input-textarea");
chatInputTextarea.addEventListener("input", updateChatInputTextareaHeight, false);

function updateChatInputTextareaHeight() {
    this.style.height = "auto";
    this.style.height = Math.min(this.scrollHeight, 100) + "px";
}

// On page load - scoll down automatically
var chatMessages = document.getElementsByClassName("chat-messages")[0];
chatMessages.scrollTop = chatMessages.scrollHeight;