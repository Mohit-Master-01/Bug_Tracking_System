$(document).ready(function () {


    $("#googleDetailsForm").validate({
        rules: {
            UserName: {
                required: true,
            },
            Email: {
                required: true,
            }
        },
        messages: {
            UserName: {
                required: "Please enter your Username."
            },
            Password: {
                required: "Please enter your Password.",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnRegister");
            const btnLoader = $("#btnLoader");
            // AJAX submission
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                $.ajax({
                    url: '/Account/GoogleDetails',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            window.location.href = '/Dashboard/Dashboard'
                        } else {
                            showToast(result.message, "error")
                        }
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast("Unknown error occurred", "error")
                    }
                });
            }, 2000);
        }
    });

});