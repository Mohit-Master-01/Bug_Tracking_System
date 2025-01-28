
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
            EmailOrUsername: {
                required: true,
            },
            Password: {
                required: true,
            }
        },
        messages: {
            EmailOrUsername: {
                required: "Please enter your Credentials."
            },
            Password: {
                required: "Please enter your Password.",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnLogin = $("#btnLogin");
            const btnLoader = $("#btnLoader");

            // Disable button and show loader
            btnLogin.prop("disabled", true);
            btnLoader.removeClass("d-none");

            setTimeout(function () {
                // AJAX submission
                $.ajax({
                    url: '/Account/Login',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            //window.location.href = '/Auth/Index';
                            alert('Login successful');
                        }
                    },
                    complete: function () {
                        //Re-enable button and hide loader
                        btnLogin.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        alert('An error occurred while registering the user.');
                    }
                });
            }, 1000);
           
        }
    });

    $("#forgotPassword").validate({
        rules: {
            Email: {
                required: true,
                email: true
            }
        },
        messages: {
            Email: {
                required: "Please enter your Email.",
                email: "Not a valid email"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnSubmit = $("#btnSubmit");
            const btnLoader = $("#btnLoader");

            // Disable button and show loader
            btnSubmit.prop("disabled", true);
            btnLoader.removeClass("d-none");

            setTimeout(function () {
                // AJAX submission
                $.ajax({
                    url: '/Account/ForgotPassword',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);

                    },
                    complete: function () {
                        btnSubmit.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        alert('An error occurred while sending the email.');
                    }
                });
            }, 1000);
            
        }
    });


    $("#ResetPassword").validate({
        rules: {
            PasswordHash: {
                required: true,
                minlength: 8
            },
            ConfirmPassword: {
                required: true,
                minlength: 8,
                equalTo: "#PasswordHash"
            },
        },
        messages: {
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required: "Please enter confirm password",
                minlength: "Confirm password must be least 8 characters",
                equalTo: "Password doesn't match"
            },
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            //var formData = {
            //    PasswordHash: $('#PasswordHash').val(),
            //}
            
            const btnSubmit = $("#btnSubmit");
            const btnLoader = $("#btnLoader");

            // Disable button and show loader
            btnSubmit.prop("disabled", true);
            btnLoader.removeClass("d-none");

            setTimeout(function () {
                // AJAX submission
                $.ajax({
                    url: '/Account/ResetPassword',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            window.location.href = '/Account/Login';
                        }
                    },
                    complete: function () {
                        btnSubmit.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                error: function () {
                        alert('An error occurred while registering the user.');
                    }
                });
            },1000)
            
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
