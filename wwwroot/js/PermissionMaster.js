$(document).ready(function () {

    $("#TabId, #PermissionType, #IsActive, #btnSavePermission").prop("disabled", true);

    // Attach event listener for role selection change
    $("#RoleId").change(function () {
        var roleId = $(this).val();
        if (roleId) {
            fetchRolePermissions(roleId);
            $("#TabId, #PermissionType, #IsActive, #btnSavePermission").prop("disabled", false);

        } else {
            clearPermissionsTable();
            $("#TabId, #PermissionType, #IsActive, #btnSavePermission").prop("disabled", true);
        }
    });

    // Attach event listener for save button
    $("#btnSavePermission").click(function (e) {
        e.preventDefault();
        savePermission();
    });

    // Tab selection change event (Clear fields)
    $("#RoleId").change(function () {
        $("#PermissionType").val("");
        $("#TabId").val("");
        $("#IsActive").prop("checked", false);
    });

    // Event delegation for dynamically generated edit buttons
    $("#permissionsTable").on("click", ".edit-btn", function () {
        var tabId = $(this).data("tabid");
        var permissionId = $(this).data("id");
        var isActive = $(this).data("active");
        var permissionType = $(this).data("type");
        editPermission(permissionId, tabId, isActive, permissionType);
    });
});

// Function to fetch role-based permissions
function fetchRolePermissions(roleId) {
    $.ajax({
        url: "/Masters/GetRolePermissions",
        type: "GET",
        data: { roleId: roleId },
        success: function (data) {
            console.log(data)
            updatePermissionsTable(data);
        },
        error: function () {
            alert("Failed to fetch permissions.");
        }
    });
}

// Function to update the permissions table dynamically
function updatePermissionsTable(data) {
    var tableBody = $("#permissionsTable");
    tableBody.empty();

    if (data.length > 0) {
        $.each(data, function (index, permission) {
            var row = `
                <tr>
                    <td>${index + 1}</td>
                    <td>${permission.tabName}</td>
                    <td>${permission.permissionType}</td>
                    <td>${permission.isactive ? "Yes" : "No"}</td>
                    <td>
                        <button class="btn btn-sm btn-warning edit-btn"
                                data-tabid="${permission.tabid}" 
                                data-active="${permission.isactive}"
                                data-id="${permission.permissionid}"
                                data-type="${permission.permissionType}">
                            Edit
                        </button>
                        <a href="/Masters/DeletePermission?id=${permission.permissionid}" class="btn btn-sm btn-danger">Delete</a>
                    </td>
                </tr>`;
            tableBody.append(row);
        });
    } else {
        tableBody.append("<tr><td colspan='5' class='text-center'>No permissions found for this role.</td></tr>");
    }
}

// Function to clear the permissions table
function clearPermissionsTable() {
    $("#permissionsTable").html("<tr><td colspan='5' class='text-center'>No permissions found for this role.</td></tr>");
}

// Function to populate form fields for editing a permission
//function editPermission(tabId, isActive,id, type) {

function editPermission(permissionId, tabId, isActive, permissionType) {
    $("#Permissionid").val(permissionId);
    $("#TabId").val(tabId);
    $("#PermissionType").val(permissionType);

    // Fix IsActive Checkbox Issue
    if (isActive === "True" || isActive === "true" || isActive == 1) {
        $("#IsActive").prop("checked", true);
    } else {
        $("#IsActive").prop("checked", false);
    }
}


// Function to save permission changes
function savePermission() {
    var roleId = $("#RoleId").val();
    var tabId = $("#TabId").val();
    var permissionType = $("#PermissionType").val();
    var permissionId = $("#Permissionid").val();

    if (!roleId || !tabId || !permissionType) {
        alert("All fields are required!");
        return;
    }

    var data = {
        Roleid: parseInt(roleId),
        Tabid: parseInt(tabId),
        PermissionType: permissionType,
        Permissionid: permissionId,
        Isactive: $("#IsActive").is(":checked")
    };

    $.ajax({
        url: '/Masters/SavePermission',
        type: 'POST',
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            alert(response.message);
            if (response.success) {
                location.reload();
            }
            //alert("Permission saved successfully!");
        },
        error: function (xhr) {
            console.error("Error:", xhr.responseText);
            alert("Something went wrong: " + xhr.status);
        }
    });
}