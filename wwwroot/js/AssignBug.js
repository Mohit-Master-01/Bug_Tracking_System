﻿$(document).ready(function () {
    $("#assignBugForm").submit(function (e) {
        e.preventDefault();

        var developerId = $("#developerId").val();
        if (!developerId) {
            showToast("Please select a developer!", "error");
            return;
        }

        var formData = $(this).serialize();

        $.ajax({
            type: "POST",
            url: "/Bugs/AssignBug",
            data: formData,
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            success: function (response) {
                console.log("Response received:", response);

                if (response.success) {
                    showToast(response.message, "success");

                    setTimeout(function () {
                        window.location.href = "/Bugs/UnassignBugList";
                    }, 2000);
                } else {
                    showToast(response.message, "error");
                }
            },
            error: function (xhr) {
                console.error("AJAX Error:", xhr.responseText);
                showToast("Something went wrong. Please try again.", "error");
            }
        });
    });

    function showToast(message, type) {
        Toastify({
            text: message,
            duration: 3000,
            close: true,
            gravity: "top",
            position: "right",
            backgroundColor: type === "success" ? "green" : "red",
            stopOnFocus: true
        }).showToast();
    }

    $("#assignBugForm").submit(function (event) {
        event.preventDefault(); // Prevent default form submission

        var bugId = $("input[name='bugId']").val();
        var developerId = $("#developerId").val();

        if (!developerId) {
            showToast("Please select a developer.", "error");
            return;
        }

        $.ajax({
            url: "/Bugs/AssignBug",
            type: "POST",
            data: { bugId: bugId, developerId: developerId },
            success: function (response) {
                if (response.success) {
                    showToast(response.message, "success");
                    setTimeout(function () {
                        window.location.href = "/Bugs/UnassignBugList";
                    }, 2000);
                } else {
                    showToast(response.message, "error");
                }
            },
            error: function () {
                showToast("An error occurred while assigning the bug.", "error");
            }
        });
    });

    function showToast(message, type) {
        Toastify({
            text: message,
            duration: 3000,
            gravity: "top",
            position: "right",
            backgroundColor: type === "success" ? "green" : "red",
        }).showToast();
    }

});
