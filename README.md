# 📚 Library Management System (ASP.NET Core Web API)

## 📖 Project Overview
This project is a Library Management System built with ASP.NET Core Web API (.NET 8). It allows authenticated members to borrow and return books, and view their active loans. Book management endpoints are also available for adding and listing books.

The solution demonstrates:
- Entity Framework Core with SQL Server
- SSO authentication via OpenID Connect
- Model validation and error handling
- Structured logging
- Unit tests for non-trivial business logic
---

## ⚙️ Technical Requirement
- Language: C# only
- Framework: ASP.NET Core Web Application (.NET 8)
- IDE: Visual Studio 2022 (Community Edition)
- Database: SQL Server 2022 Express, SQL Server Management Studio (SSMS) 2022
- Data Access: Entity Framework Core (chosen for productivity and LINQ support; see justification below)
- Authentication: OpenID Connect (OIDC) with JWT Bearer middleware
- Validation: DataAnnotations on models (e.g., [Required], [StringLength])
- Error Handling: Appropriate HTTP status codes (400, 401, 403, 404) with meaningful messages
- Testing: xUnit with at least 3 unit tests covering loan limit, book availability, and ownership rules
- Logging: Structured logging via ILogger<T>

## 📖 Data Access Choice
I chose Entity Framework Core because:
- It integrates seamlessly with ASP.NET Core and SQL Server.
- LINQ queries make business rules (loan limits, availability checks) concise and readable.
- EF Core migrations simplify schema evolution.

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022
- SQL Server Express 2022
- SQL Server Management Studio (SSMS) 2022
- Docker Desktop
- Postman for testing endpoint with valid JWT tokens
- GitHub account for repository access

## 🔐 Authentication
- Keycloak (Please refer at the end of the README for configuration)
- All member-facing endpoints (/api/loan/borrow/{bookId}, /api/loan/return/{loanId}, /api/loan/me) are protected with JWT Bearer authentication.
Tokens must be issued by a standards-compliant OIDC provider (For this assessment I use Keycloak).
- Middleware validates issuer, audience.
- Endpoints return:
  - 401 Unauthorized → missing/invalid token
  - 403 Forbidden → member attempts action on another member’s data
- Book management endpoints (/api/book) can be public.

### 🛠 Setup Instructions
1. Clone the repository:
   ```bash
   git clone https://github.com/HafizuddinRahdzi/library-assessment-hafizuddin-rifdi.git
   cd library-assessment-hafizuddin-rifdi
2. Restore dependencies
   ```bash
   dotnet restore
3. Database Setup (There are two ways to set up the database):
   ### Option 1: Run SQL Scripts
   - Run the script to create tables (Books, Members, Loans) from schema.sql file.
   - Run the script from seed.sql to insert sample data.
   ### Option 2: Run EF Core Migrations
   - Ensure the connection string in `appsettings.json` points to your SQL Server instance.
   - Run the following commands from the project root:
   ```bash
   dotnet ef database update
6. Connection String
   - In appsettings.json:
   ```json
   "ConnectionStrings": {
   "DefaultConnection": "Server=DESKTOP-LE2LBQ2\\SQLEXPRESS;Database=LMSDB;Trusted_Connection=True;TrustServerCertificate=True"
   },
7. Run the application:
   ``` bash
   dotnet run

---

## ▶️ Running the Application
Endpoints include:
- POST /api/loans/borrow
- POST /api/loans/return
- GET /api/loans/myloans
- GET /api/members/me

---

## 🧪 Running Test
- Unit tests are included in the LMSTest project:
  ```bash
  dotnet test
- Covered Scenarios: 
  - Loan limit rule → Reject borrow when member has 3 active loans.
  - Book availability → Reject borrow when no copies are available.
  - Ownership rule (SSO provisioning) → Reject return if loan belongs to another member.

---

## 📌 Reviewer Notes
- Install SQL Server Express/Developer or run Docker container.
- Update appsettings.json with your connection string.
- Configure OIDC provider (Keycloak).
  - Run Keycloak in Docker
    ```bash
    docker run -d --name keycloak \
    -p 8080:8080 \
    -e KEYCLOAK_ADMIN=admin \
    -e KEYCLOAK_ADMIN_PASSWORD=admin \
    quay.io/keycloak/keycloak:24.0.2 start-dev
  - Create a Realm & Client
    - Go to http://localhost:8080.
    - Login with admin/admin.
    - Create a new Realm (LMSRealm)
    - Create a new Client (lms-api) with type OpenID Connect and set Valid Redirect URls to http://localhost:5168/*.
  - Add a Client Audience Mapper
    - Go to Clients → lms-api → Client Scopes.
    - Select lms-api-dedicated scope (or create one).
    - Configure a new Mapper:
    - Mapper Type: Audience.
    - Name: audience-lms-api.
    - Included Client Audience: lms-api.
    - Add to ID token: ON.
    - Add to Access token: ON.
    - Save.
  - Create a test user with a password.
  - Get a Test Token
    - Use Keycloak's token endpoint:
    ```bash
    curl -X POST \
      http://localhost:8080/realms/LibraryRealm/protocol/openid-connect/token \
      -d "client_id=lms-api" \
      -d "username=testuser" \
      -d "password=secret" \
      -d "grant_type=password"
    ```
    - This returns a JWT that you can paste into Postman as Authorization: Bearer <token>
  - Use Postman to test endpoints with valid JWT tokens.
