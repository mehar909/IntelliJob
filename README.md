# IntelliJob

An AI-powered job portal built with **ASP.NET Web Forms (.NET Framework 4.7.2)**. IntelliJob connects job seekers with companies and features AI-driven mock interviews powered by **Google Gemini** and **Vapi Voice AI**.

---

## Features

- **Job Seekers** – Register, build profiles, upload resumes, search & apply for jobs, save favorites
- **Companies** – Register, post jobs, view applicants, shortlist candidates
- **Admin Panel** – Manage users, jobs, contacts, and view dashboard analytics
- **AI Mock Interviews** – Take voice-based mock interviews with AI-generated questions (Gemini + Vapi)
- **Interview Feedback** – AI-generated scoring on communication, technical skills, problem-solving, and more
- **Resume Builder** – Build and download resumes from within the platform
- **Contact Form** – Users can submit queries; admin can view and reply

---

## Prerequisites

Make sure you have the following installed:

| Tool | Download Link |
|------|--------------|
| **Visual Studio 2019/2022** (with ASP.NET and web development workload) | [https://visualstudio.microsoft.com/downloads/](https://visualstudio.microsoft.com/downloads/) |
| **SQL Server** (LocalDB is included with Visual Studio, or use SQL Server Express) | [https://www.microsoft.com/en-us/sql-server/sql-server-downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |
| **.NET Framework 4.7.2 Developer Pack** | [https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) |
| **Git** (optional, for cloning) | [https://git-scm.com/download/win](https://git-scm.com/download/win) |

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/mehar909/IntelliJob.git
cd IntelliJob
```

### 2. Open in Visual Studio

1. Open **Visual Studio**
2. Click **File → Open → Project/Solution**
3. Navigate to the cloned folder and open `IntelliJob.sln`

### 3. Restore NuGet Packages

Visual Studio should restore packages automatically on build. If not:

1. Right-click the solution in **Solution Explorer**
2. Click **Restore NuGet Packages**

The project uses these packages:

| Package | Version |
|---------|---------|
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 4.1.0 |
| Newtonsoft.Json | 13.0.3 |
| System.IO | 4.3.0 |
| System.Net.Http | 4.3.4 |
| System.Runtime | 4.3.0 |
| System.Security.Cryptography.Algorithms | 4.3.0 |
| System.Security.Cryptography.Encoding | 4.3.0 |
| System.Security.Cryptography.Primitives | 4.3.0 |
| System.Security.Cryptography.X509Certificates | 4.3.0 |

**Optional: Install packages manually via Package Manager Console**

Go to **Tools → NuGet Package Manager → Package Manager Console** and run:

```powershell
Install-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -Version 4.1.0
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package System.IO -Version 4.3.0
Install-Package System.Net.Http -Version 4.3.4
Install-Package System.Runtime -Version 4.3.0
Install-Package System.Security.Cryptography.Algorithms -Version 4.3.0
Install-Package System.Security.Cryptography.Encoding -Version 4.3.0
Install-Package System.Security.Cryptography.Primitives -Version 4.3.0
Install-Package System.Security.Cryptography.X509Certificates -Version 4.3.0
```

### 4. Set Up the Database

1. Open **SQL Server Management Studio (SSMS)** or use **SQL Server Object Explorer** in Visual Studio
2. Connect to `(localdb)\MSSQLLocalDB` (or your SQL Server instance)
3. Run the following SQL scripts **in order**:

   - **`IntelliJob/App_Data/IntelliJob_Tables.sql`** – Creates the database and all core tables (Users, JobSeekers, Companies, Jobs, AppliedJobs, Contact, FeaturedMarks, UserFavorites, etc.)
   - **`IntelliJob/App_Data/Interview_Tables.sql`** – Creates the AI interview tables (Interviews, InterviewQuestions, InterviewTranscripts, InterviewFeedback)

> **Note:** The first script includes `CREATE DATABASE IntelliJob` and `USE IntelliJob` at the top. If you already have a database named `IntelliJob`, skip those lines.

### 5. Configure the Connection String

Open `IntelliJob/Web.config` and verify the connection string matches your SQL Server instance:

```xml
<connectionStrings>
    <add name="cs"
         connectionString="Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=IntelliJob; Integrated Security=True; trustServerCertificate=true"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

- If you're using **SQL Server Express** instead of LocalDB, change `Data Source` to `.\SQLEXPRESS`
- If you're using a **named instance**, update accordingly

### 6. Configure API Keys

Open `IntelliJob/AppSettings.config` and add your API keys:

```xml
<appSettings>
    <add key="username" value="Admin" />
    <add key="password" value="123" />
    <add key="ValidationSettings:UnobtrusiveValidationMode" value="None" />

    <!-- Gemini API Key: Get yours from https://aistudio.google.com/apikey -->
    <add key="GeminiApiKey" value="YOUR_GEMINI_API_KEY" />

    <!-- Gemini Model options: gemini-3.1-flash-lite, gemini-3-flash, gemini-2.5-flash -->
    <add key="GeminiModel" value="gemini-3.1-flash-lite" />

    <!-- Vapi Voice AI: Get your public web token from https://dashboard.vapi.ai -->
    <add key="VapiWebToken" value="YOUR_VAPI_PUBLIC_KEY" />
</appSettings>
```

| Key | Where to Get It | Required For |
|-----|----------------|--------------|
| `GeminiApiKey` | [Google AI Studio](https://aistudio.google.com/apikey) | AI interview question generation & feedback |
| `VapiWebToken` | [Vapi Dashboard](https://dashboard.vapi.ai) | Voice-based AI mock interviews |

> **Note:** The AI interview features will not work without valid API keys. The rest of the application (job posting, applying, admin panel) works without them.

### 7. Build and Run

1. Press **Ctrl + Shift + B** to build the solution
2. Press **F5** (or click the green play button) to run with debugging
3. The website will open in your default browser via IIS Express

---

## Default Admin Login

| Field | Value |
|-------|-------|
| Username | `Admin` |
| Password | `123` |

> These credentials are configured in `AppSettings.config`. Change them before deploying to production.

---

## Project Structure

```
IntelliJob/
├── Admin/              # Admin panel pages (Dashboard, UserList, JobList, etc.)
├── Company/            # Company portal (PostJob, Applicants, CompanyProfile, etc.)
├── User/               # Job seeker pages (Home, JobListing, Profile, Interview, etc.)
├── App_Data/           # SQL scripts for database setup
├── assets/             # CSS, JS, fonts, images
├── photos/             # Uploaded user profile photos
├── Resumes/            # Uploaded resumes
├── Images/             # Uploaded company logos/images
├── GeminiService.cs    # Google Gemini API integration
├── Utils.cs            # Utility/helper functions
├── Web.config          # Main configuration (connection string, etc.)
├── AppSettings.config  # API keys and app settings
└── Global.asax         # Application lifecycle events
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| **Build fails with missing packages** | Right-click solution → Restore NuGet Packages |
| **Database connection error** | Verify your connection string in `Web.config` matches your SQL Server instance |
| **"Gemini API key is not configured"** | Add a valid API key in `AppSettings.config` |
| **Interview voice not working** | Add a valid Vapi web token in `AppSettings.config` |
| **LocalDB not found** | Install SQL Server LocalDB via Visual Studio Installer (Individual Components → SQL Server Express LocalDB) |
| **Port conflict on IIS Express** | Right-click project → Properties → Web → change the port number |

---

## Tech Stack

- **Backend:** ASP.NET Web Forms, C#, .NET Framework 4.7.2
- **Database:** SQL Server (LocalDB / Express)
- **AI:** Google Gemini API, Vapi Voice AI
- **Frontend:** HTML, CSS, JavaScript, Bootstrap
- **Serialization:** Newtonsoft.Json
