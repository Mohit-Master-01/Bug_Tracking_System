$(document).ready(function () {

    

    // Initialize Quill Editor
    var quill = new Quill('#editor', {
        theme: 'snow',
        placeholder: 'Describe the bug with details...',
        modules: {
            toolbar: [
                [{ 'header': [1, 2, false] }],
                ['bold', 'italic', 'underline'],
                [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                [{ 'align': [] }],
                ['link', 'code-block']
            ]
        }
    });

    // Load existing description into Quill (For Edit Case)
    var descriptionValue = $("#Description").val();
    if (descriptionValue) {
        quill.root.innerHTML = descriptionValue;
    }

    //// Sync Quill editor content to hidden input before form submit
    //$("form").on("submit", function () {
    //    $("#Description").val(quill.root.innerHTML);

    //    // Perform front-end validation
    //    if (!validateForm()) {
    //        return false;  // Prevent form submission
    //    }
    //});

    // Sync Quill editor content to hidden input before form submit
    $("#BugForm").on("submit", function () {
        $("#Description").val(quill.root.innerHTML);

        // Perform front-end validation
        if (!validateForm()) {
            return false;  // Prevent form submission
        }

        // Show loader and disable button
        $("#btnBug").prop("disabled", true);
        $("#btnLoader").removeClass("d-none");
    });


    // Attachments Removal
    $(".remove-attachment").click(function () {
        var attachmentId = $(this).data("attachment-id");
        var button = $(this);

        if (confirm("Are you sure you want to delete this attachment?")) {
            $.ajax({
                url: '/Bugs/DeleteAttachment/' + attachmentId,
                type: 'DELETE',
                success: function (response) {
                    if (response.success) {
                        button.closest("div").remove(); // Remove from UI
                        showToast("Attachment deleted successfully!", "success");
                    } else {
                        showToast("Failed to delete attachment.", "error");
                    }
                },
                error: function () {
                    showToast("An error occurred while deleting the attachment.", "error");
                }
            });
        }
    });



    // Full Form Validation
    function validateForm() {
        let isValid = true;
        $(".error-message").remove();

        let title = $("#Title").val().trim();
        let description = quill.root.innerHTML.trim();
        let severity = $("#Severity").val();
        let priority = $("#Priority").val();
        let status = $("#StatusId").val();
        let project = $("#ProjectId").val();

        if (title === "") {
            showError($("#Title"), "Module name is required");
            isValid = false;
        }

        if (description === "" || description === "<p><br></p>") {
            showError($("#editor"), "Description is required");
            isValid = false;
        }

        if (!severity) {
            showError($("#Severity"), "Please select a severity");
            isValid = false;
        }

        if (!priority) {
            showError($("#Priority"), "Please select a priority");
            isValid = false;
        }

        if (!status) {
            showError($("#StatusId"), "Please select a status");
            isValid = false;
        }

        if (!project) {
            showError($("#ProjectId"), "Please select a project");
            isValid = false;
        }

        // Attachments validation
        if (!validateAttachments()) {
            isValid = false;
        }

        if (isValid) {
            showToast("Bug report saved successfully!", "success");
        }

        return isValid;
    }

    // Attachment Validation Function
    function validateAttachments() {
        let isValid = true;
        let allowedExtensions = [".jpg", ".jpeg", ".png", ".pdf", ".docx"];
        let maxFileSize = 5 * 1024 * 1024; // 5MB

        $(".attachment-input").each(function () {
            let files = $(this).prop("files");
            if (files.length > 0) {
                for (let i = 0; i < files.length; i++) {
                    let file = files[i];
                    let fileExtension = "." + file.name.split('.').pop().toLowerCase();

                    if (!allowedExtensions.includes(fileExtension)) {
                        showToast(`Invalid file type: ${file.name}`, "error");
                        isValid = false;
                    }

                    if (file.size > maxFileSize) {
                        showToast(`File too large: ${file.name}`, "error");
                        isValid = false;
                    }
                }
            }
        });

        return isValid;
    }

    // Show Error Messages
    function showError(element, message) {
        let errorHtml = `<div class="error-message text-danger mt-1" style="font-size: 14px;">${message}</div>`;
        $(element).after(errorHtml);
    }

    // Show Toast Messages
    function showToast(message, type) {
        let toastColor = type === "success" ? "green" : "red";

        Toastify({
            text: message,
            duration: 3000,
            gravity: "top",
            position: "right",
            backgroundColor: toastColor,
        }).showToast();
    }

    

});
