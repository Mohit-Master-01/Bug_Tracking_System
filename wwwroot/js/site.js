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
