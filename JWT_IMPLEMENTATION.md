# JWT Token Implementation Guide

## Overview
This application now implements **JWT (JSON Web Token)** authentication with **localStorage** support for modern web applications. Tokens are securely stored on the client-side and sent with API requests.

## 🎯 Features

✅ **JWT Token Generation** - Secure token creation on login/register  
✅ **localStorage Storage** - Tokens stored on client-side  
✅ **Automatic Token Expiration** - Tokens expire after configured time  
✅ **Token Validation** - Verify token authenticity and expiration  
✅ **Hybrid Authentication** - Both session and JWT support  
✅ **AJAX Forms** - Non-blocking authentication requests  

## 📋 JWT Configuration

JWT settings are configured in `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-characters-long-please",
    "Issuer": "FirstMvcApp",
    "Audience": "FirstMvcAppUsers",
    "ExpireMinutes": 60
  }
}
```

### Configuration Parameters:
- **Secret**: Private key for signing tokens (should be 32+ characters)
- **Issuer**: Who issued the token (e.g., your app name)
- **Audience**: Who can use the token
- **ExpireMinutes**: How long token is valid (default: 60 minutes)

⚠️ **IMPORTANT**: Change the `Secret` value to a strong, unique key in production!

## 🏗️ Architecture

### JWT Service (`Services/JwtService.cs`)
Handles token generation and validation:
- `GenerateToken(user)` - Creates JWT token for user
- `ValidateToken(token)` - Validates token signature and expiration
- `GetUserIdFromToken(token)` - Extracts user ID from token

### Account Controller (`Controllers/AccountController.cs`)
Updated to:
- Return JSON responses with tokens
- Generate JWT on login and registration
- Store token in session for server-side access
- Return user data along with token

### Auth JavaScript (`wwwroot/js/auth.js`)
Client-side token management:
- `AuthService.setToken(token)` - Store token in localStorage
- `AuthService.getToken()` - Retrieve token from localStorage
- `AuthService.setUser(user)` - Store user data
- `AuthService.getUser()` - Get user data
- `AuthService.isAuthenticated()` - Check if user logged in
- `AuthService.logout()` - Clear tokens and user data
- Automatic token expiration checking

## 📱 How It Works

### Login Flow
1. User submits login form via AJAX
2. Server validates credentials
3. Server generates JWT token
4. Server returns token + user data as JSON
5. Client stores token in `localStorage` via `AuthService`
6. Client redirects to home page

### Token Storage
```javascript
// localStorage keys:
localStorage.jwtToken  // JWT token string
localStorage.user      // User object (JSON)
```

### Using Tokens in API Calls
```javascript
// Get token for Authorization header
const token = AuthService.getToken();
const authHeader = `Bearer ${token}`;

// Use in fetch requests
fetch('/api/endpoint', {
    headers: {
        'Authorization': authHeader
    }
});
```

### Token Expiration
The `auth.js` file automatically:
- Checks if token is expired on page load
- Checks every 60 seconds
- Logs user out if token expired
- Redirects to login page

## 🔐 Security Best Practices

1. **HTTPS Only** - Always use HTTPS in production
2. **Secure Secret** - Change default JWT secret to strong random value
3. **Short Expiration** - Use shorter expiration times for sensitive apps
4. **Token Rotation** - Implement refresh tokens for longer sessions
5. **HttpOnly Cookies** - Consider storing tokens in HttpOnly cookies instead of localStorage for enhanced security

## 📝 JWT Token Structure

Tokens are in format: `header.payload.signature`

### Decoded Token Example:
```javascript
{
  // Header
  {
    "alg": "HS256",
    "typ": "JWT"
  },
  
  // Payload (Claims)
  {
    "nameid": "user_id",
    "email": "user@example.com",
    "name": "User Name",
    "userId": "user_id",
    "exp": 1672531200,
    "iss": "FirstMvcApp",
    "aud": "FirstMvcAppUsers"
  },
  
  // Signature (verified on server)
  "HMACSHA256(header.payload, secret)"
}
```

## 🧪 Testing

1. **Register** - Go to `/Account/Register`
   - Fill form and submit
   - Token stored in localStorage
   - Logged in user dropdown appears

2. **Login** - Go to `/Account/Login`
   - Login with credentials
   - Token stored in localStorage
   - Check DevTools → Application → localStorage

3. **Verify Token** - Open browser console:
   ```javascript
   localStorage.getItem('jwtToken')      // See token
   localStorage.getItem('user')          // See user data
   AuthService.isAuthenticated()         // Check if logged in
   ```

4. **Token Expiration** - Wait for token to expire:
   - Change `ExpireMinutes` to 0 in appsettings.json
   - Restart app
   - Token will auto-logout user

## 🚀 API Integration

For API endpoints that require authentication:

### Protect API Endpoints
```csharp
[Authorize]  // Requires valid JWT token
public IActionResult ProtectedEndpoint()
{
    var userId = User.FindFirst("userId")?.Value;
    return Ok(new { message = "Protected data", userId });
}
```

### Send Token in Requests
```javascript
const response = await fetch('/api/protected', {
    method: 'GET',
    headers: {
        'Authorization': `Bearer ${AuthService.getToken()}`
    }
});
```

## 📊 Database Considerations

User credentials stored in MongoDB:
- Email (unique index recommended)
- Password (SHA256 hashed)
- FullName
- Timestamps (createdAt, updatedAt)

### Create Index for Performance:
```javascript
db.users.createIndex({ "email": 1 }, { unique: true })
```

## 🔄 Refresh Tokens (Optional Enhancement)

For production apps, implement refresh tokens:
1. Generate short-lived access token (15-60 minutes)
2. Generate long-lived refresh token (7-30 days)
3. Store refresh token in httpOnly cookie
4. When access token expires, use refresh token to get new one
5. If refresh token expired, require re-login

## ⚠️ Using with Traditional Forms

Current implementation uses AJAX for auth endpoints. Traditional form submission will still work but won't store JWT automatically. To use traditional forms:

1. Add hidden field in form to store token
2. Use JavaScript to intercept form submission
3. Handle JSON response before redirect

## 🆘 Troubleshooting

### Token Not Stored
- Check browser console for errors
- Verify localStorage is not disabled
- Check that auth.js is loaded

### Token Expired Too Quickly
- Check `Jwt:ExpireMinutes` in appsettings.json
- Verify server time is correct
- Check deployed environment clock settings

### "Invalid Token" Errors
- Verify JWT secret matches between environments
- Check token format in Authorization header (`Bearer {token}`)
- Ensure token hasn't been modified

## 📚 References

- [JWT.io](https://jwt.io) - Debug and understand JWTs
- [Microsoft JWT Documentation](https://docs.microsoft.com/aspnet/core/security)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
