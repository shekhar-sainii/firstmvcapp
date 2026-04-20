# MongoDB Setup Guide for FirstMvcApp

This guide will help you set up MongoDB for your ASP.NET Core MVC application.

## Installation Options

### Option 1: MongoDB Community Edition (Local)
1. Download MongoDB Community Edition from: https://www.mongodb.com/try/download/community
2. Install MongoDB on your machine
3. By default, MongoDB runs on `localhost:27017`
4. The connection string in `appsettings.json` is already configured: `mongodb://localhost:27017`

### Option 2: MongoDB Atlas (Cloud)
1. Go to: https://www.mongodb.com/cloud/atlas
2. Create a free account
3. Create a new cluster
4. Get your connection string
5. Update the `ConnectionStrings.MongoDb` in `appsettings.json` with your Atlas connection string

### Option 3: Docker (Recommended for Development)
If you have Docker installed, run MongoDB in a container:

```bash
docker run -d -p 27017:27017 --name mongodb mongo:latest
```

## Configuration

The MongoDB connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017"
  }
}
```

For MongoDB Atlas, replace with:
```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority"
  }
}
```

## Features Implemented

✅ **User Model** - MongoDB BSON Document mapping
✅ **MongoDbService** - Full CRUD operations
✅ **Password Hashing** - SHA256 password encryption
✅ **Authentication** - Login/Register/Logout
✅ **Session Management** - User session tracking
✅ **Error Handling** - Try-catch with logging

## Database Collections

The application will automatically create a database `FirstMvcAppDb` with a `users` collection when you first run the app.

### User Document Structure
```json
{
  "_id": ObjectId(),
  "fullName": "string",
  "email": "string",
  "password": "hashed_password",
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "isActive": true
}
```

## Testing

1. **Register**: Go to `/Account/Register` and create a new account
2. **Login**: Go to `/Account/Login` with your credentials
3. **Verify**: Check MongoDB to see the new user document

## NuGet Packages

The following package is used:
- `MongoDB.Driver` (v2.23.0)

## Troubleshooting

### Connection Error
- Ensure MongoDB is running
- Check connection string in `appsettings.json`
- Verify MongoDB port (default: 27017)

### User Already Exists
- The system prevents duplicate email registrations
- Try registering with a different email address

### Password Issues
- Passwords are hashed using SHA256
- Minimum 6 characters required
- Passwords must match in registration form

## Next Steps

1. Install and run MongoDB
2. Build and run the application
3. Navigate to `/Account/Register`
4. Create a test account
5. Login and logout to test functionality
