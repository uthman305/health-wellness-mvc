# FPI Health & Wellness Monitoring System (ASP.NET Core MVC)

This is the C# MVC version of the system described in your project report,
matching the same scope: authentication with roles, wellness logging, BMI
computation, gamification (points/badges/leaderboard), mentorship, and an
admin aggregate dashboard. Data is stored in PostgreSQL.

## Prerequisites

- .NET 8 SDK — https://dotnet.microsoft.com/download
- PostgreSQL 14+ running locally (or a remote instance)
- (Optional) pgAdmin or DBeaver to inspect the database

## Setup steps

1. **Create the database**

   ```
   createdb fpi_wellness
   ```

2. **Set your connection string**

   Edit `appsettings.json` and replace the placeholder password:

   ```json
   "DefaultConnection": "Host=localhost;Port=5432;Database=fpi_wellness;Username=postgres;Password=YOUR_PASSWORD"
   ```

3. **Restore packages**

   ```
   dotnet restore
   ```

4. **Install the EF Core CLI tool (once, globally)**

   ```
   dotnet tool install --global dotnet-ef
   ```

5. **Create and apply the initial migration**

   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

   This creates the Identity tables (AspNetUsers, AspNetRoles, etc.) plus
   the eight app tables (WellnessLogs, BmiRecords, Points, Badges,
   UserBadges, MentorshipAssignments, WellnessCardSessions) from the
   models in `Models/`.

6. **Run the app**

   ```
   dotnet run
   ```

   Visit the URL shown in the console (usually `https://localhost:5001`).

## First-time use

- Register a couple of accounts as **Student** and **Lecturer** through
  the Register page — these two roles are self-service.
- The **Administrator** role isn't offered at sign-up (by design, matching
  the report's RBAC model). To make yourself an admin, after registering,
  run this once against the database:

  ```sql
  INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
  SELECT u."Id", r."Id"
  FROM "AspNetUsers" u, "AspNetRoles" r
  WHERE u."Email" = 'your-email@example.com' AND r."Name" = 'Administrator';
  ```

- Log in, submit a wellness log, and you'll see points, badge, and BMI
  feedback immediately, plus a trend chart once you've logged twice.
- As Administrator, use the "Assign Mentors" nav link to pair a Lecturer
  with a Student, then log in as either to record a wellness card session.

## Project structure

```
Controllers/   — Account, Home, Wellness, Gamification, Mentorship, Admin
Models/        — entities + ViewModels
Services/      — BmiService, GamificationService (points/badges logic)
Data/          — ApplicationDbContext (EF Core + Identity)
Views/         — Razor views per controller
```

## Note on this build

This code was written and reviewed by hand but has not been compiled here
(no .NET SDK / no internet access in the environment that produced it).
Run `dotnet build` first thing after restoring packages — if anything
doesn't compile, it'll most likely be a small namespace or package-version
mismatch, easy to fix locally.


// "DefaultConnection": "Host=localhost;Port=5432;Database=fpi_wellness;Username=postgres;Password=1234"