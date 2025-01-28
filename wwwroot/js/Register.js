$(document).ready(function () {

    $("#Registration").validate({
        rules: {
            UserName: {
                required: true,
                minlength: 4,
                maxlength: 50
            },
            Email: {
                required: true,
                email: true
            },
            PasswordHash: {
                required: true,
                minlength: 6
            },
            ConfirmPassword: {
                required: true,
                minlength: 8,
                equalTo: "#PasswordHash"
            },
        },
        messages: {
            Username: {
                required: "Please enter a username.",
                minlength: "Username must be at least 3 characters.",
                maxlength: "Username cannot exceed 15 characters."
            },
            //FirstName: {
            //    required: "Please enter your first name."
            //},
            Email: {
                required: "Please enter your email.",
                email: "Please enter a valid email address."
            },
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required: "Please enter confirm password",
                minlength: "Confirm password must be least 8 characters",
                equalTo: "Password doesn't match"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault();
            const btnRegister = $("#btnRegister");
            const btnLoader = $("#btnLoader");

            //Disable button and show loader
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");

            const formData = new FormData(form);

            $.ajax({
                url: '/Account/Registration',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        alert('Successfull');
                        window.location.href = '/Account/OtpCheck';
                    }
                },
                complete: function () {
                    //Re-enable button and hide loader
                    btnRegister.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    alert('An error occured while registering the user.');
                }
            });
        }

    });



// Toggle visibility of the Password field
document.getElementById('togglePasswordHash').addEventListener('click', function () {
    const passwordField = document.getElementById('PasswordHash');
    const type = passwordField.type === 'password' ? 'text' : 'password';
    passwordField.type = type;

    // Toggle icon
    this.innerHTML = type === 'password' 
        ? '<i class="bi bi-eye"></i>' 
        : '<i class="bi bi-eye-slash"></i>';
});

// Toggle visibility of the Confirm Password field
document.getElementById('toggleConfirmPassword').addEventListener('click', function () {
    const confirmPasswordField = document.getElementById('ConfirmPassword');
    const type = confirmPasswordField.type === 'password' ? 'text' : 'password';
    confirmPasswordField.type = type;

    // Toggle icon
    this.innerHTML = type === 'password' 
        ? '<i class="bi bi-eye"></i>' 
        : '<i class="bi bi-eye-slash"></i>';
});


    $.validator.addMethod("lettersOnly", function (value, element) {
        return this.optional(element) || /^[a-zA-Z]+$/.test(value);
    }
    )
});