$(document).ready(function () {

    $("#Registration").validate({
        rules: {
            UserName: { required: true, minlength: 4, maxlength: 50 },
            Email: { required: true, email: true },
            PasswordHash: { required: true, minlength: 6 },
            ConfirmPassword: { required: true, minlength: 8, equalTo: "#PasswordHash" }
        },
        messages: {
            UserName: { required: "Please enter a username.", minlength: "At least 4 characters.", maxlength: "Max 50 characters." },
            Email: { required: "Please enter your email.", email: "Invalid email format." },
            PasswordHash: { required: "Please enter a password.", minlength: "At least 6 characters." },
            ConfirmPassword: { required: "Confirm password required.", equalTo: "Passwords do not match." }
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const btnRegister = $("#btnRegister");
            const btnLoader = $("#btnLoader");

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
                    if (result.success) {
                        showToast("success", "Registration successful! OTP sent to email.");
                        setTimeout(() => { window.location.href = '/Account/OtpCheck'; }, 2000);
                    } else {
                        showToast("error", result.message);
                    }
                },
                complete: function () {
                    btnRegister.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    showToast("error", "An error occurred while registering.");
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