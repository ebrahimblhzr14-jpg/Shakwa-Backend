# Shakwa Backend — Local Developer Setup Guide

## 1. Primary Development Tools & Prerequisites

* **Primary IDE:** Microsoft Visual Studio 2022 (Community, Professional, or Enterprise) with the **ASP.NET and web development** workload.
* **.NET SDK:** .NET 8.0 SDK (version 8.0.x or higher).
* **Database Engine:** Microsoft SQL Server 2019, 2022, or SQL Server Express / LocalDB.
  *(Note: Visual Studio provides tooling to connect to databases, but a separate SQL Server database engine instance or SQL Server Express installation is required on the local machine).*
* **Database Management Tool:** SQL Server Management Studio (SSMS) or Azure Data Studio.

---

## 2. Downloading the Repository

Anyone can obtain the backend source code directly from GitHub without an account:
1. Navigate to: `https://github.com/ebrahimblhzr14-jpg/Shakwa-Backend`
2. Click the green **Code** button and select **Download ZIP**.
3. Extract the downloaded ZIP archive to a local folder (e.g., `C:\Projects\Shakwa-Backend`).

Alternatively, clone via Git:
```powershell
git clone https://github.com/ebrahimblhzr14-jpg/Shakwa-Backend.git
```

---

## 3. Project Structure

```
Shakwa-Backend/
├── docs/
│   └── LOCAL_SETUP.md             # Developer setup guide
└── src/
    └── complaint_project_ADO/
        ├── ComplainsSystemAPI/    # ASP.NET Core 8 Web API & controllers
        ├── ComplaintManagement.Access/   # ADO.NET data access layer & repositories
        ├── ComplaintManagement.Business/ # Domain logic, DTOs & security services
        ├── ComplaintManagement.Tests/    # Unit & integration test suites
        └── ComplainsSystemAPI.sln        # Visual Studio 2022 solution
```

---

## 4. Opening and Building in Visual Studio 2022

1. Launch **Visual Studio 2022**.
2. Select **Open a project or solution**.
3. Browse to and open:
   `src\complaint_project_ADO\ComplainsSystemAPI.sln`
4. Visual Studio will automatically restore required NuGet packages. If needed, right-click the solution in Solution Explorer and select **Restore NuGet Packages**.
5. Build the solution (**Build > Build Solution** or press **Ctrl+Shift+B**).

---

## 5. Local Database Configuration

### Database Schema Notice:
* **Current Limitation:** A standalone safe SQL database creation script is not included in this source repository. The backend uses direct ADO.NET parameterized queries against existing tables and stored procedures.
* Database initialization requires obtaining the development database schema or restoring the authorized development backup into your local SQL Server instance as `ComplaintManagementDB`.

### Configuring the Connection String:
To configure your local database connection without committing credentials or passwords:

#### Option A: Windows Authentication (Recommended)
Edit `src/complaint_project_ADO/ComplainsSystemAPI/appsettings.json` or `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ComplaintManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```
*(If using a named instance like SQL Server Express, use `Server=.\\SQLEXPRESS;Database=ComplaintManagementDB;...`)*.

#### Option B: .NET User Secrets
From the `ComplainsSystemAPI` project folder:
```powershell
cd src/complaint_project_ADO/ComplainsSystemAPI
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ComplaintManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
```

---

## 6. Running the Backend Service

### Via Visual Studio 2022:
1. In Solution Explorer, right-click `ComplainsSystemAPI` and choose **Set as Startup Project**.
2. Select the `ComplainsSystemAPI` profile in the top toolbar.
3. Press **Ctrl+F5** (Run without debugging) or **F5** (Debug).
4. The API will listen on `http://localhost:5000` (or `http://0.0.0.0:5000`).

### Via Command Line:
```powershell
cd src/complaint_project_ADO
dotnet run --project ComplainsSystemAPI/ComplainsSystemAPI.csproj --urls "http://0.0.0.0:5000"
```

---

## 7. Running Backend Tests

In Visual Studio 2022, open **Test > Test Explorer** and click **Run All Tests**, or execute via CLI:
```powershell
dotnet test src/complaint_project_ADO/ComplaintManagement.Tests/ComplaintManagement.Tests.csproj
```
*(36/36 tests passing)*.
