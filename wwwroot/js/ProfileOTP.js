$(document).ready(function () {

    // Handle OTP input focus and validation
    $('.otp-box').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, ''); // Allow only numbers

        if (this.value.length === 1) {
            $(this).next('.otp-box').focus(); // Auto-focus next box
        }
    });

    $("#loginForm").submit(function (event) {
        event.preventDefault();

        // Ensure email is fetched correctly
        const email = $('#Email').val();
        if (!email) {
            showToast("error", "Error: Email is missing. Please try again.");
            return;
        }

        // Combine values from all OTP input fields
        let otp = "";
        $('.otp-box').each(function () {
            otp += $(this).val();
        });

        // Validate OTP length
        if (otp.length !== 6) {
            showToast("warning", "Please enter the complete 6-digit OTP.");
            return;
        }

        const formData = {
            Email: email,
            Otp: otp
        };

        console.log("Form Data Sent:", formData); // Debugging output

        const btnNext = $("#btnNext");
        const btnLoader = $("#btnLoader");

        // Disable button and show loader
        btnNext.prop("disabled", true);
        btnLoader.removeClass("d-none");

        setTimeout(function () {
            $.ajax({
                url: '/Profile/ProfileOTP',
                type: 'POST',
                data: formData,
                success: function (result) {
                    console.log("OTP Verification Result:", result); // Debugging output

                    if (result.success) {
                        showToast("success", "OTP Verified Successfully! Redirecting...");
                        setTimeout(() => { window.location.href = "/Profile/Profile"; }, 2000);
                    } else {
                        showToast("error", "Invalid OTP. Please try again.");
                    }
                },
                complete: function () {
                    btnNext.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function (xhr) {
                    console.error("AJAX Error:", xhr.responseText);
                    showToast("error", "Unable to verify OTP. Please try again.");
                }
            });
        }, 1000);
    });

    // OTP Resend functionality
    $("#resend-btn").click(function (e) {
        e.preventDefault();

        $.ajax({
            url: "/Account/ResendOtp",
            method: "GET",
            success: function (data) {
                console.log("Resend OTP Response:", data); // Debugging output

                if (data.success) {
                    showToast("info", "OTP has been resent to your email.");
                    startOtpTimer();
                } else {
                    showToast("error", "Failed to resend OTP. Please try again.");
                }
            },
            error: function (xhr) {
                console.error("Resend OTP Error:", xhr.responseText);
                showToast("error", "An error occurred. Please try again.");
            }
        });
    });

    // Timer for OTP expiration
    function startOtpTimer() {
        let timeRemaining = 60;
        const timerElement = $("#timer");
        const resendSection = $("#resend-section");

        resendSection.hide();

        const interval = setInterval(() => {
            if (timeRemaining <= 0) {
                clearInterval(interval);
                timerElement.text("You can now resend OTP.");
                resendSection.show();
            } else {
                timeRemaining--;
                const minutes = Math.floor(timeRemaining / 60).toString().padStart(2, "0");
                const seconds = (timeRemaining % 60).toString().padStart(2, "0");
                timerElement.text(`Time remaining: ${minutes}:${seconds}`);
            }
        }, 1000);
    }

    // Start the timer when the page loads
    startOtpTimer();
});
