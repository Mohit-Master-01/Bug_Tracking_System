$(document).ready(function () {
    $("#loginForm").submit(function (event) {
        event.preventDefault();

        // Fetch email
        const email = $('#Email').val();

        // Combine values from all OTP input fields
        let otp = '';
        $('.otp-box').each(function () {
            otp += $(this).val();
        });

        // Validate OTP length
        if (otp.length !== 6) {
            alert("Please enter the complete 6-digit OTP.");
            return;
        }

        const formData = {
            Email: email,
            Otp: otp
        };

        const btnNext = $("#btnNext");
        const btnLoader = $("#btnLoader");

        // Disable button and show loader
        btnNext.prop("disabled", true);
        btnLoader.removeClass("d-none");

        console.log(formData.Email);
        console.log(formData.Otp);

        // Simulate a delay before AJAX
        setTimeout(function () {
            // AJAX submission
            $.ajax({
                url: '/Account/OtpCheck',
                type: 'POST',
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        alert('Otp Verified Successfully');
                        window.location.href = "/Account/Login";
                    }
                },
                error: function () {
                    alert('Unable to verify OTP');
                }
            });
        }, 1000);
    });

    // Timer and Resend OTP logic remain the same
    const timerElement = $("#timer");
    const resendSection = $("#resend-section");
    const resendButton = $("#resend-btn");
    const duration = 60;
    let timeRemaining = duration;

    const interval = setInterval(() => {
        if (timeRemaining <= 0) {
            clearInterval(interval);
            timerElement.text("You can resend the OTP.");
            resendSection.show();
        } else {
            timeRemaining--;
            const minutes = Math.floor(timeRemaining / 60).toString().padStart(2, "0");
            const seconds = (timeRemaining % 60).toString().padStart(2, "0");
            timerElement.text(`Time remaining: ${minutes}:${seconds}`);
        }
    }, 1000);

    resendButton.click(function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Account/ResendOTP",
            method: "GET",
            success: function (data) {
                if (data.success) {
                    alert("OTP has been resent to your email.");
                    timeRemaining = duration;
                    resendSection.hide();
                    timerElement.text("Time remaining: 01:00");
                    setInterval(interval);
                } else {
                    alert("Failed to resend OTP. Please try again.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error resending OTP:", error);
                alert("An error occurred. Please try again.");
            }
        });
    });

    $('.otp-box').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, ''); // Allow only numbers
    });


});
