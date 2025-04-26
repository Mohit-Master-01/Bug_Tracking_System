document.addEventListener("DOMContentLoaded", function () {
    const notificationDropdown = document.getElementById("notificationDropdown");
    const notificationList = document.getElementById("notificationList");
    const notificationCount = document.getElementById("notificationCount");

    async function fetchNotifications() {
        try {
            const response = await fetch('/Notification/Notifications', {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) {
                console.warn(`Server error: ${response.status}`);
                return; // Just exit silently, don't crash
            }

            const contentType = response.headers.get("content-type") || "";
            if (!contentType.includes("application/json")) {
                console.warn("Invalid response format (not JSON)");
                return;
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
            const response = await fetch('/Notification/GetUnreadCount', {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) {
                console.warn(`Server error: ${response.status}`);
                return;
            }

            const contentType = response.headers.get("content-type") || "";
            if (!contentType.includes("application/json")) {
                console.warn("Invalid response format (not JSON)");
                return;
            }

            const data = await response.json();

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
            event.preventDefault();
            const notificationId = event.target.getAttribute("data-id");

            try {
                await fetch(`/Notification/MarkAsRead?notificationId=${notificationId}`, {
                    method: "POST",
                    credentials: 'include'
                });
                await fetchNotifications();
                await updateNotificationCount();
            } catch (error) {
                console.error("Error marking notification as read:", error);
            }
        }
    });

    notificationDropdown.addEventListener("click", function (event) {
        event.preventDefault();
        fetchNotifications();
    });

    fetchNotifications();
    updateNotificationCount();

    // === SignalR Real-Time Setup ===
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/NotificationHub")
        .build();

    connection.start()
        .then(() => {
            console.log("SignalR Connected to NotificationHub!");
        })
        .catch(err => console.error("SignalR Connection Error: ", err));

    connection.on("ReceiveNotification", function (message) {
        console.log("New notification received:", message);
        fetchNotifications();
        updateNotificationCount();
    });
});
