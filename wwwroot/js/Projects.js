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
});


