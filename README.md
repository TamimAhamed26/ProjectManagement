# 🧭 Project Management System (ASP.NET Core MVC)

A **role-based Project Management System** built with **ASP.NET Core 9 MVC**, **Entity Framework Core**, and **SQL Server**.  
It provides structured task management, departmental organization, and access control for Admins, Managers, and Employees.

---

## 📚 Table of Contents
- [Features](#-features)
- [Role & User Seeding](#-role--user-seeding)
- [Core Modules](#️-core-modules)
- [Database](#-database)
- [Installation & Setup](#-installation--setup)
- [Technologies Used](#-technologies-used)
- [UI Preview (AdminLTE Template)](#-ui-preview-adminlte-template)
- [License](#-license)
- [Screenshots](#-screenshots)
  - [SuperAdmin / Admin](#-superadmin--admin)
  - [Employee Dashboard](#-employee-dashboard)

---

## ✨ Features

### 🔐 Authentication & Authorization
- ASP.NET Core Identity integrated.
- Role-based access with:
  - **SuperAdmin** – full control over roles, departments, categories, and all data.
  - **Admin** – manage roles, departments, and tasks (except SuperAdmin).
  - **Manager** – similar to Admin, focused on department-level operations.
  - **Employee** – view, submit, and track assigned tasks.
  
#### 🧭 Task Assignment Workflow
- Admins assign tasks to employees.  
- Employees can accept, work on, and submit tasks.  
- Admins/Managers review and approve submissions.

#### 🗂️ Other Key Features
- **File & Link Support:** Attach reference materials and submission files.  
- **Real-time Filtering:** Filter tasks by status, department, and employee.  
- **Calendar View:** Visual representation of task deadlines.  
- **Profile Management:** Users can update their profiles and photos.  

---

## 🧱 Role & User Seeding

Automatic role and user creation via `ApplicationDbInitializer`:

| Role | Default Email | Password |
|------|----------------|----------|
| SuperAdmin | superadmin@example.com | SuperAdmin123! |
| Admin | admin@example.com | Admin123! |
| Manager | manager@example.com | Manager123! |
| Employee | employee@example.com | Employee123! |

> Additional demo users (`employee1@example.com`, `admin2@example.com`, etc.) are also created automatically.

---

## 🗂️ Core Modules

### 1️⃣ **Departments**
- Admin/SuperAdmin can create, edit, delete, and view departments.  
- Departments can have multiple **Users** and **Categories**.  
- Supports **AJAX-based** creation, editing, and deletion.

### 2️⃣ **Categories**
- Linked to Departments.  
- Manageable via AJAX requests.  
- Tasks can be assigned/unassigned to categories dynamically.

### 3️⃣ **Task Management (TasklistController)**
- Full CRUD operations for tasks.  
- Tasks may belong to categories or remain “Uncategorized.”  
- Admins and Managers can manage tasks with validation.

### 4️⃣ **Task Assignment (AssignedTaskController)**
- Managers/Admins assign tasks to employees.  
- **Reference files or links** can be attached.  
- Employees submit work and request confirmation.  
- Admins/Managers **review, approve, or reject submissions**.  
- **AJAX filters (status, department, employee)** for **interactive dashboards**.  
- Employees can **add remarks** and track progress.

### 5️⃣ **Roles & User Management (RolesController)**
- Create/edit/delete roles (SuperAdmin/Admin).  
- Assign users to roles and departments.  
- AJAX endpoints for fetching unassigned users.

### 6️⃣ **Profile Management**
- Users can edit their profile, picture, phone number, and email.  
- Profile image validation (only `.jpg`, `.jpeg`, `.png`, max 5 MB).  
- Automatically replaces previous profile picture on update.

### 7️⃣ **Dashboard (HomeController)**
- Role-based redirects:
  - Admins/Managers → **Admin Dashboard (AIndex)**
  - Employees → **Employee Dashboard (EIndex)**
- Displays assigned tasks, departments, and quick links.

### 🔍 Filtering & Search
- Filter by status, department, employee, assigned-by.  
- Real-time client-side filtering.  
- Persistent filters within a session.

---

## 💾 Database

**Using:**  
- Entity Framework Core  
- SQL Server  

### File Upload Directories
Ensure these exist and have write permissions:
```

wwwroot/Pictures
wwwroot/TaskReferences
wwwroot/TaskSubmissions

````

---

## ⚙️ Installation & Setup

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/TamimAhamed26/ProjectManagement.git
cd ProjectManagement
````

### 2️⃣ Configure Database

In `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ProjectManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### (Optional) Configure Google OAuth

Add this section to `appsettings.json`:

```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

* Then register credentials in **Google Cloud Console → Credentials**.
* Redirect URI:
  `https://localhost:7057/signin-google`
* If skipped, normal Identity login will still work.

### 3️⃣ Apply Migrations

```bash
dotnet ef database update
```

Or from Package Manager Console:

```bash
Update-Database
```

### 4️⃣ Build and Run the Project

```bash
dotnet run
```

---

## 🧰 Technologies Used

| Category       | Technology                                    |
| -------------- | --------------------------------------------- |
| Framework      | ASP.NET Core 9 MVC                            |
| ORM            | Entity Framework Core                         |
| Database       | SQL Server                                    |
| Authentication | ASP.NET Core Identity + Google OAuth          |
| Frontend       | Razor Views + Bootstrap 5 (AdminLTE Template) |
| JS Plugins     | ApexCharts, OverlayScrollbars                 |

---

## 📸 UI Preview (AdminLTE Template)

* Sidebar navigation
* Task cards and table views
* Role-based navbar
* Filterable task table
* Responsive layout (Bootstrap 5)

---

## 📜 License

This project is for **educational purposes**.

---

## 📸 Screenshots

### 🧑‍💼 SuperAdmin / Admin

#### 🗂️ Category and User Management (AJAX)

<img width="1199" height="687" alt="Category and User Management" src="https://github.com/user-attachments/assets/d59cc916-69f7-4fca-afd4-60b1f9aede51" />

#### ➕ Add Task to Categories (AJAX)

<img width="1416" height="750" alt="Add Task to Category" src="https://github.com/user-attachments/assets/9ead33c4-2647-4a15-8473-ac9e77e16592" />

#### 🧾 Admin Assigns Tasks (Department-wise & Category-wise)

<img width="1196" height="706" alt="Admin Assigns Task" src="https://github.com/user-attachments/assets/23aaf738-4533-4b47-b2b5-b8deffe286df" />

#### 🔍 Admin Task View (Filter/Search by Employee or Department)

<img width="1435" height="775" alt="Admin Task View with Filter" src="https://github.com/user-attachments/assets/7a65713e-85b0-417d-acd2-1dda902c92ad" />

#### 🌐 SuperAdmin Task Overview (Filter/Search by Employee / Department / Assigner)

<img width="1431" height="766" alt="SuperAdmin Task Overview" src="https://github.com/user-attachments/assets/7fe0f6fc-810d-4aef-be89-20ae97d875a2" />

---

### 👨‍💻 Employee Dashboard

#### 📊 Employee Task Overview

<img width="1175" height="778" alt="Employee Dashboard Overview" src="https://github.com/user-attachments/assets/e0e0cfc6-e913-49a0-ba40-36b024faa627" />

#### 📝 Task Detail, Remarks & Overdue Status

Tasks automatically move to **Overdue** if past the due date.
Employees can **add remarks** or perform **single task updates**. <img width="1440" height="753" alt="Employee Task Detail and Remarks" src="https://github.com/user-attachments/assets/2d896daf-9dc7-44f8-9e05-632672bd7487" />

#### ⚙️ Single Task Update Example

<img width="1397" height="774" alt="Single Task Update" src="https://github.com/user-attachments/assets/9f2be082-8c29-4766-86fd-152125531f2a" />
```
