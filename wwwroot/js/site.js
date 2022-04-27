var chatInputForm = document.getElementById("chat-input-form");
var chatInputTextarea = document.getElementById("chat-input-textarea");

// Textarea - expand vertically on text input, max 5 lines
if (chatInputTextarea) {
    chatInputTextarea.addEventListener("input", function () {
        this.style.height = "auto";
        this.style.height = Math.min(this.scrollHeight, 100) + "px";
    });
}

// On page load - scoll down automatically
var chatMessages = document.getElementById("chat-messages");
chatMessages.scrollTop = chatMessages.scrollHeight;