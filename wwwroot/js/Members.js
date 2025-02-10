$(document).ready(function () {

    // ✅ Initialize Form Validation
    $("#memberForm").validate({
        rules: {
            UserName: { required: true, minlength: 3, maxlength: 50 },
            Email: { required: true, email: true },
            RoleId: { required: true },
            ProjectId: {
                required: function () {
                    return $("#roleDropdown").val() === "1"; // Required only for Project Manager
                }
            }
        },
        messages: {
            UserName: { required: "Please enter the user's name.", minlength: "At least 3 characters.", maxlength: "Max 50 characters." },
            Email: { required: "Please enter an email address.", email: "Invalid email format." },
            RoleId: { required: "Please select a role." },
            ProjectId: { required: "Please select a project." }
        },
        highlight: function (element) {
            $(element).addClass("is-invalid");
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid").addClass("is-valid");
        },
        errorPlacement: function (error, element) {
            error.addClass("text-danger");
            error.insertAfter(element);
        },
        submitHandler: function (form, event) {
            event.preventDefault(); // Prevent default form submission

            if (!$("#memberForm").valid()) {
                showToast("error", "Please correct the highlighted errors.");
                return;
            }

            const btnSubmit = $("#btnMember");
            const btnLoader = $("#btnLoader");

            btnSubmit.prop("disabled", true);
            btnLoader.removeClass("d-none");

            var formData = new FormData(form);

            $.ajax({
                url: '/Members/SaveMember',
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,               
                success: function (result) {
                    if (result.success) {
                        showToast("success", "Member has been saved successfully!");

                        //// ✅ Reset form on successful add (Not edit)
                        //if ($("#UserId").val() === "0") {
                        //    $("#memberForm")[0].reset();
                        //    $("#projectContainer").hide();
                        //}

                        // ✅ Redirect after delay
                        setTimeout(() => {
                            window.location.href = "/Members/MembersList";
                        }, 2000);
                    } else {
                        showToast("error", result.message);
                    }
                },
                complete: function () {
                    btnSubmit.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    showToast("error", "An error occurred. Please try again.");
                }
            });


        }
    });

    // ✅ Show/Hide Project Dropdown based on Role selection
    $("#roleDropdown").change(function () {
        var selectedRole = $(this).val();

        if (selectedRole === "1") { // If "Project Manager" is selected
            $("#projectContainer").show();

            // Fetch and populate projects dynamically
            $.get("/Members/GetProjects", function (data) {
                $("#projectDropdown").empty().append('<option value="">Select Project</option>');
                $.each(data, function (index, project) {
                    $("#projectDropdown").append(`<option value="${project.projectId}">${project.projectName}</option>`);
                });
            });

        } else {
            $("#projectContainer").hide();
            $("#projectDropdown").empty().append('<option value="">Select Project</option>');
        }
    });


});

/**
 * ✅ Show Toast Notification
 */
function showToast(type, message) {
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: 3000
    };

    if (type === "success") {
        toastr.success(message);
    } else if (type === "error") {
        toastr.error(message);
    } else if (type === "info") {
        toastr.info(message);
    } else {
        toastr.warning(message);
    }
}
