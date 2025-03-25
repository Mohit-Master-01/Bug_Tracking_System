document.addEventListener("DOMContentLoaded", function () {
    const notificationDropdown = document.getElementById("notificationDropdown");
    const notificationList = document.getElementById("notificationList");
    const notificationCount = document.getElementById("notificationCount");

    async function fetchNotifications() {
        try {
            const response = await fetch('/Notification/GetUnreadNotifications');

            if (!response.ok) {
                throw new Error(`Server error: ${response.status}`);
            }

            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                throw new Error("Invalid response format");
            }

            const notifications = await response.json();

            notificationList.innerHTML = "";
            if (notifications.length > 0) {
                notifications.forEach(n => {
                    let item = document.createElement("li");
                    item.innerHTML = `<a href="#" class="dropdown-item notification-item" data-id="${n.notificationId}">${n.message}</a>`;
                    notificationList.appendChild(item);
                });
                notificationCount.style.display = "inline";
                notificationCount.innerText = notifications.length;
            } else {
                notificationList.innerHTML = `<li><a class="dropdown-item text-center" href="#">No new notifications</a></li>`;
                notificationCount.style.display = "none";
            }
        } catch (error) {
            console.error("Error fetching notifications:", error);
        }
    }


    async function updateNotificationCount() {
        try {
            const response = await fetch('/Notification/GetUnreadCount');
            const data = await response.json();

            console.log("Notification count:", data.count); // Debugging

            if (data.count > 0) {
                notificationCount.style.display = "inline";
                notificationCount.innerText = data.count;
            } else {
                notificationCount.style.display = "none";
            }
        } catch (error) {
            console.error("Error fetching notification count:", error);
        }
    }

    notificationList.addEventListener("click", async function (event) {
        if (event.target.classList.contains("notification-item")) {
            const notificationId = event.target.getAttribute("data-id");

            try {
                await fetch(`/Notification/MarkAsRead?notificationId=${notificationId}`, { method: "POST" });
                fetchNotifications(); // Refresh notifications after marking as read
                updateNotificationCount();
            } catch (error) {
                console.error("Error marking notification as read:", error);
            }
        }
    });

    notificationDropdown.addEventListener("click", fetchNotifications);
    fetchNotifications(); // Initial Load
    updateNotificationCount(); // Update Count on Page Load
});