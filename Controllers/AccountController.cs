using Microsoft.AspNetCore.Mvc;
using FirstMvcApp.Models;
using FirstMvcApp.Services;
using FirstMvcApp.Utilities;

namespace FirstMvcApp.Controllers;

public class AccountController : Controller
{
    private readonly MongoDbService _mongoDbService;
    private readonly JwtService _jwtService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(MongoDbService mongoDbService, JwtService jwtService, ILogger<AccountController> logger)
    {
        _mongoDbService = mongoDbService;
        _jwtService = jwtService;
        _logger = logger;
    }

    // GET: Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Login (returns JSON with JWT token)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Find user by email
                var user = await _mongoDbService.GetUserByEmailAsync(model.Email);

                if (user != null && PasswordHelper.VerifyPassword(model.Password, user.Password))
                {
                    // Generate JWT token
                    var token = _jwtService.GenerateToken(user);

                    // Store in session as well for server-side use
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("IsAuthenticated", "true");
                    HttpContext.Session.SetString("JwtToken", token);

                    _logger.LogInformation($"User {user.Email} logged in successfully");

                    var authData = new AuthDataResponse
                    {
                        Token = token,
                        User = new UserDto
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Email = user.Email
                        }
                    };

                    // Return JSON response with token
                    return Json(ApiResponse<AuthDataResponse>.SuccessResponse(authData, "Login successful!", 200));
                }
                else
                {
                    _logger.LogWarning($"Failed login attempt for {model.Email}");
                    return Json(ApiResponse<AuthDataResponse>.ErrorResponse("Invalid email or password", 401));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return Json(ApiResponse<AuthDataResponse>.ServerErrorResponse("An error occurred during login"));
            }
        }

        return Json(ApiResponse<AuthDataResponse>.ErrorResponse("Invalid model state", 400));
    }

    // GET: Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Register (returns JSON with JWT token)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _mongoDbService.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return Json(ApiResponse<AuthDataResponse>.ErrorResponse("This email is already registered", 409));
                }

                // Create new user
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = PasswordHelper.HashPassword(model.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Save user to MongoDB
                bool created = await _mongoDbService.CreateUserAsync(user);

                if (created)
                {
                    // Get the created user
                    var newUser = await _mongoDbService.GetUserByEmailAsync(model.Email);
                    if (newUser != null)
                    {
                        // Generate JWT token
                        var token = _jwtService.GenerateToken(newUser);

                        // Store in session as well
                        HttpContext.Session.SetString("UserId", newUser.Id);
                        HttpContext.Session.SetString("UserEmail", newUser.Email);
                        HttpContext.Session.SetString("UserName", newUser.FullName);
                        HttpContext.Session.SetString("IsAuthenticated", "true");
                        HttpContext.Session.SetString("JwtToken", token);

                        _logger.LogInformation($"New user {newUser.Email} registered successfully");

                        var authData = new AuthDataResponse
                        {
                            Token = token,
                            User = new UserDto
                            {
                                Id = newUser.Id,
                                FullName = newUser.FullName,
                                Email = newUser.Email
                            }
                        };

                        return Json(ApiResponse<AuthDataResponse>.SuccessResponse(authData, "Registration successful! Welcome!", 201));
                    }
                }

                return Json(ApiResponse<AuthDataResponse>.ErrorResponse("Error creating account. Please try again", 400));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                return Json(ApiResponse<AuthDataResponse>.ServerErrorResponse("An error occurred during registration"));
            }
        }

        return Json(ApiResponse<AuthDataResponse>.ErrorResponse("Invalid model state", 400));
    }

    // GET: Profile
    public IActionResult Profile()
    {
        var userName = HttpContext.Session.GetString("UserName");
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var token = HttpContext.Session.GetString("JwtToken");

        if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userName))
        {
            return RedirectToAction("Login");
        }

        var model = new ProfileViewModel
        {
            FullName = userName,
            Email = userEmail,
            Token = token ?? string.Empty,
            IsAuthenticated = true
        };

        return View(model);
    }

    // GET: Logout
    public IActionResult Logout()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        HttpContext.Session.Clear();
        _logger.LogInformation($"User {userEmail} logged out");
        
        // Return a logout view that clears localStorage before redirecting
        return View();
    }
}
