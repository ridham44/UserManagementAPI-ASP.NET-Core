# User Management API

A RESTful API built with **ASP.NET Core 8** for managing users. Supports full CRUD operations, data validation, request logging middleware, and API-key authentication middleware.

---

## Features

| Feature | Details |
|---|---|
| **CRUD Endpoints** | GET, POST, PUT, DELETE for `/api/users` |
| **Validation** | Data annotations + business-rule (unique email) validation |
| **Logging Middleware** | Logs every request/response with method, path, status code, and elapsed time |
| **Auth Middleware** | API-key authentication via `X-Api-Key` header |
| **Swagger UI** | Interactive docs served at `/` (root) in development |
| **Seeded Data** | Three sample users pre-loaded on startup |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Any REST client (Swagger UI, Postman, curl, etc.)

---

## Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/<your-username>/UserManagementAPI.git
cd UserManagementAPI

# 2. Restore dependencies
dotnet restore

# 3. Run the API
dotnet run
```

The API will start on `https://localhost:5001` (or `http://localhost:5000`).  
Open `https://localhost:5001` in your browser to access **Swagger UI**.

---

## Authentication

All `/api/*` endpoints require an API key in the request header:

```
X-Api-Key: dev-secret-key-12345
```

> ⚠️ Change the key in `appsettings.json` before deploying to production.

---

## API Endpoints

### GET /api/users
Returns all users. Supports optional query filters.

| Query Param | Example | Description |
|---|---|---|
| `role` | `?role=Admin` | Filter by role (Admin, User, Moderator) |
| `isActive` | `?isActive=true` | Filter by active status |

**Response `200 OK`**
```json
{
  "success": true,
  "message": "Retrieved 3 user(s).",
  "data": [...]
}
```

---

### GET /api/users/{id}
Returns a single user by ID.

**Response `200 OK`** — user found  
**Response `404 Not Found`** — no user with that ID

---

### POST /api/users
Creates a new user.

**Request body:**
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "role": "User",
  "age": 27
}
```

**Validation rules:**
- `firstName` / `lastName` — required, 1–50 characters
- `email` — required, valid format, unique across all users
- `role` — must be `Admin`, `User`, or `Moderator`
- `age` — must be between 18 and 120

**Response `201 Created`** — user created  
**Response `400 Bad Request`** — validation errors  
**Response `409 Conflict`** — email already exists

---

### PUT /api/users/{id}
Updates an existing user. All fields are **optional** — only the fields you include will be changed.

**Request body (partial update example):**
```json
{
  "role": "Admin",
  "isActive": false
}
```

**Response `200 OK`** — updated user  
**Response `400 Bad Request`** — validation errors  
**Response `404 Not Found`** — no user with that ID  
**Response `409 Conflict`** — email conflict

---

### DELETE /api/users/{id}
Deletes a user by ID.

**Response `200 OK`** — deleted successfully  
**Response `404 Not Found`** — no user with that ID

---

## Project Structure

```
UserManagementAPI/
├── Controllers/
│   └── UsersController.cs      # CRUD endpoints
├── Data/
│   └── UserRepository.cs       # IUserRepository + in-memory implementation
├── Middleware/
│   ├── ApiKeyAuthenticationMiddleware.cs   # API-key auth
│   └── RequestLoggingMiddleware.cs         # Request/response logging
├── Models/
│   └── User.cs                 # User, CreateUserRequest, UpdateUserRequest, ApiResponse<T>
├── Program.cs                  # App startup & middleware pipeline
├── appsettings.json
└── UserManagementAPI.csproj
```

---

## Example curl Commands

```bash
# Set your base URL and API key
BASE="https://localhost:5001"
KEY="dev-secret-key-12345"

# List all users
curl -k -H "X-Api-Key: $KEY" "$BASE/api/users"

# Get user by ID
curl -k -H "X-Api-Key: $KEY" "$BASE/api/users/1"

# Create a user
curl -k -X POST "$BASE/api/users" \
  -H "X-Api-Key: $KEY" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Doe","email":"jane@example.com","role":"User","age":27}'

# Update a user (partial)
curl -k -X PUT "$BASE/api/users/1" \
  -H "X-Api-Key: $KEY" \
  -H "Content-Type: application/json" \
  -d '{"role":"Admin"}'

# Delete a user
curl -k -X DELETE "$BASE/api/users/1" \
  -H "X-Api-Key: $KEY"
```

---

## Middleware Pipeline

```
Request → RequestLoggingMiddleware → ApiKeyAuthenticationMiddleware → Controller → Response
```

1. **RequestLoggingMiddleware** — logs request arrival and response (status + ms) for every call.
2. **ApiKeyAuthenticationMiddleware** — validates `X-Api-Key` header; rejects with 401/403 if missing or wrong. Swagger paths are excluded.

---

## License

MIT
