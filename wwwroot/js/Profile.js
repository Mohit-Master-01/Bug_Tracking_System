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
                element.closest(".form-group").append(error);
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

            if (!$("#editProfileForm").valid()) {
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
                    $("#saveBtn").prop("disabled", true).text("Saving...");
                },
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        setTimeout(() => {
                            window.location.href = "/Profile";
                        }, 2000);
                    } else {
                        toastr.error(response.message);
                    }
                },
                error: function () {
                    toastr.error("An error occurred. Please try again.");
                },
                complete: function () {
                    $("#saveBtn").prop("disabled", false).text("Save");
                }
            });
        });
    



    const phoneInputField = document.querySelector("#PhoneNumber");
    phoneInput = window.intlTelInput(phoneInputField, {
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