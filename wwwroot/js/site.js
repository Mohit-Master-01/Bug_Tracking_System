// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    const preloader = document.getElementById("preloader");

    // Simulate loading time
    setTimeout(() => {
        preloader.style.opacity = "0"; // Fade-out effect
        setTimeout(() => {
            preloader.style.display = "none"; // Completely hide preloader
        }, 500); // Wait for fade-out effect to finish
    }, 2000); // Adjust delay as per loading time
});

$(document).ready(function () {
    $("#globalSearch").on("keyup", function () {
        let query = $(this).val().trim();

        if (query.length > 1) { // Start searching after 2 characters
            $.ajax({
                url: "/Search/GlobalSearch",
                type: "GET",
                data: { query: query },
                success: function (data) {
                    if (data.length > 0) {
                        let resultsHtml = "<ul class='list-group'>";
                        data.forEach(item => {
                            resultsHtml += `<li class='list-group-item'><a href='${item.url}'>${item.name}</a></li>`;
                        });
                        resultsHtml += "</ul>";
                        $("#searchResultsContent").html(resultsHtml);
                        $("#searchResults").show();
                    } else {
                        $("#searchResultsContent").html("<p class='text-muted'>No results found</p>");
                        $("#searchResults").show();
                    }
                }
            });
        } else {
            $("#searchResults").hide();
        }
    });

    // Hide search results when clicking outside
    $(document).click(function (event) {
        if (!$(event.target).closest("#globalSearch, #searchResults").length) {
            $("#searchResults").hide();
        }
    });
});



