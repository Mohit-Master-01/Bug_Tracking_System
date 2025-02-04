
$(document).ready(function () {

    document.querySelectorAll('.toggle-password').forEach(toggle => {
        toggle.addEventListener('click', () => {
            // Locate the password input within the same parent (.pass-group)
            const passwordInput = toggle.closest('.pass-group').querySelector('.pass-input');

            // Toggle input type between "password" and "text"
            if (passwordInput.type === "password") {
                passwordInput.type = "text";
                toggle.classList.remove("fa-eye-slash");
                toggle.classList.add("fa-eye");
            } else {
                passwordInput.type = "password";
                toggle.classList.remove("fa-eye");
                toggle.classList.add("fa-eye-slash");
            }
        });

    });

    $("#loginForm").validate({
        rules: {
            EmailOrUsername: { required: true },
            Password: { required: true }
        },
        messages: {
            EmailOrUsername: { required: "Please enter your credentials." },
            Password: { required: "Please enter your password." }
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const formData = new FormData(form);
            const btnLogin = $("#btnLogin");
            const btnLoader = $("#btnLoader");

            btnLogin.prop("disabled", true);
            btnLoader.removeClass("d-none");

            setTimeout(() => {
                $.ajax({
                    url: '/Account/Login',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            showToast("success", "Login successful! Redirecting...");
                            setTimeout(() => { window.location.href = '/Dashboard/Dashboard'; }, 2000);
                        } else {
                            showToast("error", result.message);
                        }
                    },
                    complete: function () {
                        btnLogin.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast("error", "An error occurred while logging in.");
                    }
                });
            }, 1000);
        }
    });


    $("#forgotPassword").validate({
        rules: {
            Email: { required: true, email: true }
        },
        messages: {
            Email: { required: "Please enter your email.", email: "Not a valid email" }
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const formData = new FormData(form);
            const btnSubmit = $("#btnSubmit");
            const btnLoader = $("#btnLoader");

            btnSubmit.prop("disabled", true);
            btnLoader.removeClass("d-none");

            setTimeout(() => {
                $.ajax({
                    url: '/Account/ForgotPassword',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        showToast("info", result.message);
                    },
                    complete: function () {
                        btnSubmit.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast("error", "An error occurred while sending the email.");
                    }
                });
            }, 1000);
        }
    });



    $("#ResetPassword").validate({
        rules: {
            PasswordHash: { required: true, minlength: 8 },
            ConfirmPassword: { required: true, minlength: 8, equalTo: "#PasswordHash" }
        },
        messages: {
            PasswordHash: { required: "Please enter a password.", minlength: "Password must be at least 8 characters." },
            ConfirmPassword: { required: "Please confirm password", equalTo: "Passwords don't match." }
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const formData = new FormData(form);
            const btnSubmit = $("#btnSubmit");
            const btnLoader = $("#btnLoader");

            btnSubmit.prop("disabled", true);
            btnLoader.removeClass("d-none");

            setTimeout(() => {
                $.ajax({
                    url: '/Account/ResetPassword',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            showToast("success", "Password changed successfully! Redirecting...");
                            setTimeout(() => { window.location.href = "/Account/Login"; }, 2000);
                        } else {
                            showToast("error", result.message);
                        }
                    },
                    complete: function () {
                        btnSubmit.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast("error", "An error occurred while resetting password.");
                    }
                });
            }, 1000);
        }
    });


    document.querySelectorAll('.toggle-password').forEach(toggle => {
        toggle.addEventListener('click', () => {
            // Locate the password input within the same parent (.input-wrap)
            const passwordInput = document.getElementById('Password');

            // Toggle input type between "password" and "text"
            if (passwordInput.type === "password") {
                passwordInput.type = "text";
                toggle.classList.remove("fa-lock");
                toggle.classList.add("fa-lock-open");
            } else {
                passwordInput.type = "password";
                toggle.classList.remove("fa-lock-open");
                toggle.classList.add("fa-lock");
            }
        });
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


});
