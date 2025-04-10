
$(document).ready(function () {


     
    // Profile Image Preview
    $("#ProfileImage").change(function (event) {
        var reader = new FileReader();
        reader.onload = function () {
            $("#profilePreview").attr("src", reader.result);
        };
        reader.readAsDataURL(event.target.files[0]);
    });

    // Show/Hide Password Field
    $("#changePasswordCheck").change(function () {
        if ($(this).is(":checked")) {
            $("#passwordField").slideDown();
        } else {
            $("#passwordField").slideUp();
        }
    });

    // jQuery Validation
    $("#editProfileForm").validate({
        rules: {
            FirstName: "required",
            LastName: "required",
            UserName: "required",
            Email: {
                required: true,
                email: true
            },
            PhoneNumber: {
                required: true,
                digits: true
            },
            LinkedInProfile: {
                url: true
            },
            GitHubProfile: {
                url: true
            },
            PasswordHash: {
                minlength: 6
            }
        },
        messages: {
            FirstName: "Please enter your first name",
            LastName: "Please enter your last name",
            UserName: "Please enter your username",
            Email: {
                required: "Email is required",
                email: "Please enter a valid email"
            },
            PhoneNumber: {
                required: "Phone number is required",
                digits: "Enter only numbers"
            },
            LinkedInProfile: "Enter a valid LinkedIn URL",
            GitHubProfile: "Enter a valid GitHub URL",
            PasswordHash: {
                minlength: "Password must be at least 6 characters long"
            }
        },
        errorPlacement: function (error, element) {
            error.addClass("text-danger small");
            element.closest(".col-md-6, .mb-3").append(error);
        },
        highlight: function (element) {
            $(element).addClass("is-invalid").removeClass("is-valid");
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid").addClass("is-valid");
        }
    });

    // AJAX Form Submission
    $("#editProfileForm").submit(function (e) {
        e.preventDefault();

        if (!$(this).valid()) {
            toastr.error("Please correct the errors in the form.");
            return;
        }

        var formData = new FormData(this);

        $.ajax({
            url: "/Profile/EditProfile",
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            beforeSend: function () {
                $("#saveBtn").prop("disabled", true);
                $("#btnSpinner").removeClass("d-none");
            },
            success: function (response) {
                if (response.success) {
                    showToast("success", response.message);
                    setTimeout(() => {
                        window.location.href = "/Profile/Profile";
                    }, 2000);
                } else {
                    showToast("error", response.message);
                }
            },
            error: function () {
                toastr.error("An error occurred. Please try again.");
            },
            complete: function () {
                $("#saveBtn").prop("disabled", false);
                $("#btnSpinner").addClass("d-none");
            }
        });
    });


    //// AJAX Form Submission
    //$("#editProfileForm").submit(function (e) {
    //    e.preventDefault();

    //    if (!$(this).valid()) {
    //        toastr.error("Please correct the errors in the form.");
    //        return;
    //    }

    //    var formData = new FormData(this);

    //    $.ajax({
    //        url: "/Profile/EditProfile",
    //        type: "POST",
    //        data: formData,
    //        contentType: false,
    //        processData: false,
    //        beforeSend: function () {
    //            $("#saveBtn").prop("disabled", true);
    //            $("#btnSpinner").removeClass("d-none");
    //        },
    //        success: function (response) {
    //            if (response.success) {
    //                showToast("success", response.message);
    //                setTimeout(() => { window.location.href = "/Profile"; }, 2000);
    //            } else {
    //                showToast("error", response.message);
    //            }
    //        },
    //        error: function () {
    //            toastr.error("An error occurred. Please try again.");
    //        },
    //        complete: function () {
    //            $("#saveBtn").prop("disabled", false);
    //            $("#btnSpinner").addClass("d-none");
    //        }
    //    });
    //});




    $("#Verification").validate({
        submitHandler: function (form, event) {
            event.preventDefault();

            const btnUpdateEmail = $("#btnUpdateEmail");
            const btnLoader = $("#btnLoader");

            btnUpdateEmail.prop("disabled", true);
            btnLoader.removeClass("d-none");

            let email = document.getElementById("VerifyEmailHere").textContent;

            //const email = ("#").val(); // Fetch email from Razor Model
            console.log("Sending email for verification:", email);

            $.ajax({
                url: '/Profile/UpdateEmailVerification',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ Email: email }), // Send correct format
                success: function (result) {
                    if (result.success) {
                        showToast("success", "OTP sent to email!");

                        // Ensure redirection works properly
                        setTimeout(() => {
                            window.location.href = '/Profile/ProfileOTP';
                        }, 2000);
                    } else {
                        showToast("error", result.message);
                    }
                },
                error: function (xhr) {
                    showToast("error", "An error occurred: " + xhr.responseText);
                },
                complete: function () {
                    btnUpdateEmail.prop("disabled", false);
                    btnLoader.addClass("d-none");
                }
            });
        }
    });


    const phoneInputField = document.querySelector("#PhoneNumber");
    const phoneInput = window.intlTelInput(phoneInputField, {
        initialCountry: "IN",
        geoIpLookup: function (callback) {
            fetch('https://ipapi.co/json', { mode: 'no-cors' })
                .then((response) => response.json())
                .then((data) => callback(data.country_code))
                .catch(() => callback("us"));
        },
        utilsScript: "https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.8/js/utils.js"
    });

});
function showToast(type, message) {
    if (type === "success") {
        toastr.success(message);
    } else if (type === "error") {
        toastr.error(message);
    } else {
        console.warn("Unknown toast type:", type);
    }
}
