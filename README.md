# IP Forensics Report

A full-stack application where registered users can generate and view encrypted IP forensics reports.

The solution contains:

* **ASP.NET Core Web API**
* **Angular client**
* **MySQL database**
* Integration with **ip-api.com** and **AbuseIPDB**

## Prerequisites

Install the following before running the solution:

* Git
* .NET 10 SDK
* MySQL Server 8 or later
* Node.js compatible with Angular 22
  Recommended: Node.js 22.22.3 or later
* npm
* An AbuseIPDB account and API key

MySQL Workbench is optional, but it can make importing the database script easier.

## 1. Clone the Repository

```bash
git clone https://github.com/janevsk1/IpForensicsReport.git
cd IpForensicsReport
```

## 2. Create the Database

Run the following script in MySQL:

```text
Database/schema.sql
```

The script creates:

* The `IpForensicsReport` database
* The `Users` table
* The `IpForensicsReports` table

## 3. Configure Backend Secrets

Open a terminal in the API project:

```bash
cd IpForensicsReport.Api
```

Set the required configuration values with .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=IpForensicsReport;User ID=root;Password=YOUR_MYSQL_PASSWORD;"
dotnet user-secrets set "Jwt:SigningKey" "YOUR_LONG_RANDOM_SIGNING_KEY"
dotnet user-secrets set "ExternalApis:AbuseIpDb:ApiKey" "YOUR_ABUSEIPDB_API_KEY"
dotnet user-secrets set "ReportEncryption:Key" "YOUR_BASE64_ENCODED_32_BYTE_KEY"
```

Replace the placeholder values with your local MySQL credentials and secret keys.

The committed `appsettings.json` intentionally contains empty values for secrets. Do not commit real API keys, database passwords, JWT keys, or encryption keys.

### Generate the Report Encryption Key

The report encryption key must contain exactly 32 random bytes encoded as Base64.

PowerShell:

```powershell
$keyBytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($keyBytes)
$encryptionKey = [Convert]::ToBase64String($keyBytes)
$rng.Dispose()

dotnet user-secrets set "ReportEncryption:Key" $encryptionKey
```

You can verify the configured secrets with:

```bash
dotnet user-secrets list
```

This command is optional and only displays the currently configured values.

## 4. Run the API

From the `IpForensicsReport.Api` directory:

```bash
dotnet restore
dotnet run --launch-profile https
```

The API runs at:

```text
https://localhost:7023
```

To verify the database connection, open:

```text
https://localhost:7023/health/database
```

For a local HTTPS certificate issue, run:

```bash
dotnet dev-certs https --trust
```

Then restart the API.

## 5. Run the Angular Client

Open another terminal:

```bash
cd IpForensicsReport.Client
npm ci
npm start
```

The client runs at:

```text
http://localhost:4200
```

The Angular development proxy forwards `/api` requests to the backend at `https://localhost:7023`.

## Run Order

1. Start MySQL.
2. Start the ASP.NET Core API.
3. Start the Angular client.
4. Open `http://localhost:4200`.
5. Register a user, log in, and generate an IP report.

## Notes

* An internet connection is required when generating reports because the backend calls external IP information services.
* The AbuseIPDB API key is used only by the backend and is not exposed to Angular.
* Generated report data is encrypted before it is stored in MySQL.
* Changing the report encryption key will prevent previously stored reports from being decrypted.
