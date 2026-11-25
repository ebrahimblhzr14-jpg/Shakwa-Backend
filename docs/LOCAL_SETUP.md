# Shakwa Backend — Local Developer Setup Guide

## 1. Prerequisites and Required Software

* **Operating System:** Windows 10/11 or Windows Server
* **.NET SDK:** .NET 8.0 SDK (version 8.0.x or higher)
* **IDE / Editor:**
  * Microsoft Visual Studio 2022 (Community / Professional / Enterprise) with the **ASP.NET and web development** workload installed, OR
  * Visual Studio Code with the **C# Dev Kit** extension
* **Database Engine:** Microsoft SQL Server 2019, 2022, or SQL Server Express / LocalDB
* **Database Client:** SQL Server Management Studio (SSMS) or Azure Data Studio

---

## 2. Project Structure Overview

```
Backend/
├── docs/
│   └── LOCAL_SETUP.md
└── src/
    └── complaint_project_ADO/
        ├── ComplainsSystemAPI/         # ASP.NET Core 8 Web API host & controllers
        ├── ComplaintManagement.Access/   # ADO.NET data access layer & repositories
        ├── ComplaintManagement.Business/ # Business services, DTOs & security
        ├── ComplaintManagement.Tests/    # Unit & integration test suite
        └── ComplainsSystemAPI.sln        # Main Visual Studio Solution
```

---

## 3. Opening the Solution and Restoring Dependencies

### In Visual Studio 2022:
1. Launch Visual Studio 2022.
2. Select **Open a project or solution**.
3. Navigate to and open:
   `src\complaint_project_ADO\ComplainsSystemAPI.sln`
4. Visual Studio will automatically restore NuGet packages. If needed, right-click the solution in Solution Explorer and select **Restore NuGet Packages**.

### In CLI:
From the backend directory:
```powershell
cd src/complaint_project_ADO
dotnet restore
```

---

## 4. Database Setup & Configuration

### Database Creation Note:
* **Current Limitation:** A standalone safe SQL schema generation script is not currently included in this source repository. The project uses ADO.NET with parameterized queries and database procedures.
* Database initialization requires obtaining a development database schema from the database administrator or restoring the authorized development backup into your local SQL Server instance as `ComplaintManagementDB`.

### Configuring the Connection String:
To configure your local database connection without committing passwords or secrets into version control, use .NET User Secrets or an untracked `appsettings.Development.json` override.

#### Option A: Windows Authentication (Recommended for Local Dev)
In `appsettings.json` or `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ComplaintManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```
*(Replace `.` with your SQL Server instance name such as `localhost` or `.\SQLEXPRESS` if applicable).*

#### Option B: .NET User Secrets (Completely isolated from source tree)
Run from the `ComplainsSystemAPI` project folder:
```powershell
cd src/complaint_project_ADO/ComplainsSystemAPI
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ComplaintManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "YOUR_LOCAL_TEST_SECRET_KEY_MIN_32_CHARS"
```

---

## 5. Running the Backend Service

### Via CLI:
From the repository root:
```powershell
dotnet run --project src/complaint_project_ADO/ComplainsSystemAPI/ComplainsSystemAPI.csproj --urls "http://0.0.0.0:5000;http://0.0.0.0:5199;https://localhost:7043"
```

### Via Visual Studio:
1. Set `ComplainsSystemAPI` as the Startup Project.
2. Choose the `ComplainsSystemAPI` run profile.
3. Press **F5** (Debug) or **Ctrl+F5** (Run without debugging).

---

## 6. Running Backend Tests

To execute the test suite:
```powershell
dotnet test src/complaint_project_ADO/ComplaintManagement.Tests/ComplaintManagement.Tests.csproj
```
