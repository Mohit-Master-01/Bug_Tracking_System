$(document).ready(function () {
    $("form").submit(function (event) {
        event.preventDefault(); // Prevent default form submission

        let formData = $(this).serialize(); // Serialize form data
        let actionUrl = $(this).attr("action");

        $.ajax({
            url: actionUrl,
            type: "POST",
            data: formData,
            success: function (response) {
                if (response.success) {
                    showToast("success", response.message);
                    setTimeout(function () {
                        window.location.href = "/Projects/ProjectList";
                    }, 2000); // Redirect after 2 seconds
                } else {
                    showToast("error", response.message);
                }
            },
            error: function (xhr, status, error) {
                showToast("error", "An error occurred while processing the request.");
            }
        });
    });

    function showToast(type, message) {
        let toastHTML = `<div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                            <div class="d-flex">
                                <div class="toast-body">${message}</div>
                                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                            </div>
                        </div>`;

        let toastContainer = $("#toast-container");
        if (toastContainer.length === 0) {
            toastContainer = $('<div id="toast-container" class="position-fixed top-0 end-0 p-3" style="z-index: 1050;"></div>');
            $("body").append(toastContainer);
        }

        toastContainer.append(toastHTML);
        let toastElement = toastContainer.children(".toast:last");
        let toast = new bootstrap.Toast(toastElement[0]);
        toast.show();

        setTimeout(function () {
            toastElement.remove();
        }, 3000);
    }

    $("#assignProjectForm").submit(function (e) {
        e.preventDefault();

        var developerId = $("#developerId").val();
        if (!developerId) {
            showToast("Please select a developer!", "error");
            return;
        }

        var formData = $(this).serialize();

        $.ajax({
            type: "POST",
            url: "/Projects/AssignProject",
            data: formData,
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            success: function (response) {
                console.log("Response received:", response);

                if (response.success) {
                    showToast(response.message, "success");

                    setTimeout(function () {
                        window.location.href = "/Projects/UnassignProjectList";
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

    


});


