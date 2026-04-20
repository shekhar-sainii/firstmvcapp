# Swagger (OpenAPI) Documentation

Swagger is integrated into FirstMvcApp for interactive API documentation and testing.

## Accessing Swagger

After running the application, visit:
```
https://localhost:5001/swagger
```

## Features

✅ **Interactive API Documentation** - View all endpoints with parameters and responses
✅ **Try It Out** - Test API endpoints directly from the browser
✅ **JWT Authentication** - Built-in Bearer token support
✅ **Request/Response Examples** - See data structures
✅ **Error Response Documentation** - HTTP status codes and error messages

## API Endpoints

### User Management API

Base URL: `/api/users`

#### Get All Users
- **Endpoint**: `GET /api/users`
- **Description**: Retrieve all users from the database
- **Response**: List of User objects
- **Status Code**: 200 OK

#### Get User by ID
- **Endpoint**: `GET /api/users/{id}`
- **Description**: Get a specific user by MongoDB ObjectId
- **Parameters**: 
  - `id` (string, required) - MongoDB ObjectId
- **Response**: User object
- **Status Codes**: 200 OK, 404 Not Found

#### Get User by Email
- **Endpoint**: `GET /api/users/email/{email}`
- **Description**: Get a specific user by email address
- **Parameters**: 
  - `email` (string, required) - User email
- **Response**: User object
- **Status Codes**: 200 OK, 404 Not Found

#### Update User
- **Endpoint**: `PUT /api/users/{id}`
- **Description**: Update user information
- **Parameters**: 
  - `id` (string, required) - MongoDB ObjectId
- **Request Body**: User object with updated fields
- **Response**: Success message
- **Status Codes**: 200 OK, 400 Bad Request, 404 Not Found

#### Delete User
- **Endpoint**: `DELETE /api/users/{id}`
- **Description**: Delete a user
- **Parameters**: 
  - `id` (string, required) - MongoDB ObjectId
- **Response**: Success message
- **Status Codes**: 200 OK, 404 Not Found

## User Object Structure

```json
{
  "id": "507f1f77bcf86cd799439011",
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "hashed_password_string",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "isActive": true
}
```

## Using Bearer Token Authentication

Many endpoints require JWT authentication:

1. **Obtain Token**: 
   - Login at `/Account/Login`
   - Register at `/Account/Register`
   - Token is returned in response

2. **In Swagger**:
   - Click the "Authorize" button at top-right
   - Paste token (without "Bearer" prefix)
   - Click "Authorize"

3. **In Code**:
   ```javascript
   const token = localStorage.getItem('jwtToken');
   const response = await fetch('/api/users', {
       headers: {
           'Authorization': `Bearer ${token}`
       }
   });
   ```

## Example API Calls

### Get All Users
```bash
curl -X GET "https://localhost:5001/api/users" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Get User by ID
```bash
curl -X GET "https://localhost:5001/api/users/507f1f77bcf86cd799439011" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Get User by Email
```bash
curl -X GET "https://localhost:5001/api/users/email/john@example.com" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Update User
```bash
curl -X PUT "https://localhost:5001/api/users/507f1f77bcf86cd799439011" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "507f1f77bcf86cd799439011",
    "fullName": "Jane Doe",
    "email": "jane@example.com",
    "password": "hashed_password",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "isActive": true
  }'
```

### Delete User
```bash
curl -X DELETE "https://localhost:5001/api/users/507f1f77bcf86cd799439011" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Testing in Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Find the endpoint you want to test
3. Click "Try it out" button
4. Enter required parameters
5. Click "Execute"
6. View response and status code

## Response Examples

### Success Response (200)
```json
{
  "id": "507f1f77bcf86cd799439011",
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "$2a$11$...",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "isActive": true
}
```

### Error Response (404)
```json
{
  "message": "User not found"
}
```

### Error Response (500)
```json
{
  "message": "Error retrieving user"
}
```

## Swagger Configuration

The Swagger configuration is in `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FirstMvcApp API",
        Version = "v1",
        Description = "API documentation"
    });
    
    // JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", ...);
    c.AddSecurityRequirement(...);
});
```

## XML Comments

To add detailed descriptions in Swagger, use XML comments in your controller:

```csharp
/// <summary>
/// Get all users from the database
/// </summary>
/// <returns>List of User objects</returns>
[HttpGet]
public async Task<ActionResult<List<User>>> GetAllUsers()
{
    // Implementation
}
```

## Deployment Settings

For production, update Swagger route:

In `Program.cs`:
```csharp
// Only enable in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Troubleshooting

### Swagger UI Not Loading
- Check that URL is correct: `https://localhost:5001/swagger`
- Verify `app.UseSwagger()` is called before routing
- Check browser console for JavaScript errors

### Authorization Not Working
- Verify JWT secret in `appsettings.json`
- Check token expiration
- Ensure Bearer token format: `Bearer {token}`

### API Returns 401 Unauthorized
- Token may have expired
- Re-login and get new token
- Click "Authorize" button and paste new token

## Next Steps

1. Run `dotnet restore` to install Swagger package
2. Run `dotnet run` to start the application
3. Navigate to `https://localhost:5001/swagger`
4. Explore and test the API endpoints
5. Use for API documentation and integration testing
