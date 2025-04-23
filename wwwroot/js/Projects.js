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
                console.log(response); // Debugging: Check response in console

                if (response.success) {
                    showToast("success", response.message || "Project saved successfully!");
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
});

//$("#assignProjectForm").submit(function (e) {
//    e.preventDefault();

//    var developerIds = $("#developerIds").val(); // Get multiple selected developer IDs as array
//    if (!developerIds || developerIds.length === 0) {
//        showToast("Please select at least one developer!", "error");
//        return;
//    }

//    var formData = $(this).serialize(); // It will serialize projectId but not the multiple developer IDs correctly

//    // Manually append developerIds[] to handle multiple values
//    $.ajax({
//        type: "POST",
//        url: "/Projects/AssignProject",
//        data: {
//            ProjectId: $("input[name='ProjectId']").val(),
//            developerIds: developerIds
//        },
//        traditional: true, // Important to send array properly
//        contentType: "application/x-www-form-urlencoded",
//        dataType: "json",
//        success: function (response) {
//            console.log("Response received:", response);

//            if (response.success) {
//                showToast(response.message, "success");
//                setTimeout(function () {
//                    window.location.href = "/Projects/UnassignProjectList";
//                }, 2000);
//            } else {
//                showToast(response.message, "error");
//            }
//        },
//        error: function (xhr) {
//            console.error("AJAX Error:", xhr.responseText);
//            showToast("Something went wrong. Please try again.", "error");
//        }
//    });
//});

    //$("#assignProjectForm").submit(function (e) {
    //    e.preventDefault();

    //    var developerId = $("#developerId").val();
    //    if (!developerId) {
    //        showToast("Please select a developer!", "error");
    //        return;
    //    }

    //    var formData = $(this).serialize();

    //    $.ajax({
    //        type: "POST",
    //        url: "/Projects/AssignProject",
    //        data: formData,
    //        contentType: "application/x-www-form-urlencoded",
    //        dataType: "json",
    //        success: function (response) {
    //            console.log("Response received:", response);

    //            if (response.success) {
    //                showToast(response.message, "success");

    //                setTimeout(function () {
    //                    window.location.href = "/Projects/UnassignProjectList";
    //                }, 2000);
    //            } else {
    //                showToast(response.message, "error");
    //            }
    //        },
    //        error: function (xhr) {
    //            console.error("AJAX Error:", xhr.responseText);
    //            showToast("Something went wrong. Please try again.", "error");
    //        }
    //    });
    //});

    //function showToast(message, type) {
    //    Toastify({
    //        text: message,
    //        duration: 3000,
    //        close: true,
    //        gravity: "top",
    //        position: "right",
    //        backgroundColor: type === "success" ? "green" : "red",
    //        stopOnFocus: true
    //    }).showToast();
    //}

    





