# 🐞 Bug Tracking System

A comprehensive web-based bug tracking and project management system with role-based access, real-time capabilities, and integration with third-party services like Google and Zoom. Built on ASP.NET Core MVC with modular design and a secure architecture.

---

## 📌 Features Overview

### 🧑‍💼 User Roles & Access
- **Admin**: Full control of projects, members, assignments, system logs, and reports.
- **Project Manager**: Project oversight, developer assignments, bug lifecycle management.
- **Developer**: Bug fixing, tracking via calendar, and project-specific access.
- **Tester**: Bug reporting (manual + Excel), verification, and status updating.

---

## 🧩 Key Functionalities

### ✅ Authentication & Security
- Email/Password & Google OAuth Login
- Email OTP Verification
- Encrypted Password Storage
- Restrict/Unrestrict Users
- Role-Based Access Control

### 📁 Project Management
- Create, edit, assign, and track projects
- Assign multiple users across roles
- **Project-Based Workspace**:
  - Members can access only assigned projects
  - **Navbar dropdown** to switch between projects (for multi-project users)
  - Context-aware filters and bug visibility

### 🐛 Bug Tracking
- Add/Edit/Delete Bugs
- Import via Excel + ZIP (images linked by row/image number)
- Bug Calendar View (FullCalendar)
- Status updates, filters, and priority settings

### 📅 Google Calendar Integration
- Add bug events with reminder notifications (Google OAuth-only)

### 📞 Zoom Meeting Integration
- **Schedule meetings** related to bugs or discussions from inside the app
- **Role-based access** to Zoom (PM, Developer, Tester)
- Store Zoom links and notify participants via email

### 💬 Chat System (Planned)
- In-app real-time chat workspace between project members
- Based on SignalR (under development)

### 📊 Reports & Export
- Download project/member/bug reports in PDF and Excel
- Filter by project, user, status, and role

### 📜 Audit Logs
- Track logins, data changes, and permission updates
- Password-protected "Clear Logs" feature
- Filter by module/action/date

---

## 🛠️ Tech Stack

| Layer        | Technology            |
|--------------|------------------------|
| Backend      | ASP.NET Core MVC       |
| Frontend     | HTML5, CSS3, JS, Razor |
| DB           | SQL Server (EF Core)   |
| Auth         | Google OAuth2 + OTP    |
| Design       | Argon Dashboard        |
| Calendar     | FullCalendar.js        |
| Excel        | EPPlus                 |
| Notifications| SignalR (ready)        |
| Meetings     | Zoom API Integration   |

---

## 🔐 System Architecture & Design

- MVC Architecture + Repository Pattern
- DTOs for data transfer between layers
- Hubs setup for future real-time features (notifications)
- Folder Structure:
├── Controllers/
├── Views/
├── Models/
├── Repositories/
├── DTOs/
├── Hubs/
├── wwwroot/
├── BugAttachments/


---

## 🧪 Testing Strategy

| Testing Type      | Description |
|--------------------|-------------|
| Unit Testing       | (Planned) NUnit/XUnit test methods for services & logic |
| Integration Testing| Validated flows like user registration, project assignment, and bug lifecycle manually |
| System Testing     | Role-wise validation for all modules (Admin, PM, Developer, Tester) |
| Usability Testing  | Interface flow, validation feedback, and mobile responsiveness |
| User Satisfaction  | Feedback from test users to refine workflow and experience |

---

## 🚀 Future Enhancements

- ✅ **SignalR Notifications** (In-app bug alerts, role-specific updates)
- ✅ **Real-Time Chat** among members within the same project
- ✅ **Performance Dashboard** with charts and graphs
- ✅ **Dark Mode UI Toggle**
- ✅ **Bug Severity Prediction (ML-based)**
- ✅ **Mobile App (Flutter / PWA)**
- ✅ **Push Notifications via Firebase**

---

## 📬 Contact
Developed by: Mohit Mitesh Master 
For queries or suggestions: mohitmaster440@gmail.com
