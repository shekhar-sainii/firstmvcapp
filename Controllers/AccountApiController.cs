using Microsoft.AspNetCore.Mvc;
using FirstMvcApp.Models;
using FirstMvcApp.Services;
using FirstMvcApp.Utilities;

namespace FirstMvcApp.Controllers;

/// <summary>
/// Authentication API endpoints for login and registration
/// </summary>
[ApiController]
[Route("api/account")]
[Produces("application/json")]
public class AccountApiController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;
    private readonly JwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountApiController> _logger;

    public AccountApiController(MongoDbService mongoDbService, JwtService jwtService, IEmailService emailService, ILogger<AccountApiController> logger)
    {
        _mongoDbService = mongoDbService;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// User login - returns JWT token on success
    /// </summary>
    /// <param name="model">Email and password credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Login successful, returns token</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .ToDictionary(e => "validation", e => new[] { e.ErrorMessage });
            return BadRequest(ApiResponse<AuthDataResponse>.ValidationErrorResponse(errors, "Invalid model state"));
        }

        try
        {
            // Find user by email
            var user = await _mongoDbService.GetUserByEmailAsync(model.Email);

            if (user != null && PasswordHelper.VerifyPassword(model.Password, user.Password))
            {
                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

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

                return Ok(ApiResponse<AuthDataResponse>.SuccessResponse(authData, "Login successful!", 200));
            }
            else
            {
                _logger.LogWarning($"Failed login attempt for {model.Email}");
                return Unauthorized(ApiResponse<AuthDataResponse>.UnauthorizedResponse("Invalid email or password"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            return StatusCode(500, ApiResponse<AuthDataResponse>.ServerErrorResponse("An error occurred during login"));
        }
    }

    /// <summary>
    /// User registration - creates new account and returns JWT token
    /// </summary>
    /// <param name="model">User registration information</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="201">Account created successfully</response>
    /// <response code="409">Email already registered</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthDataResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .ToDictionary(e => "validation", e => new[] { e.ErrorMessage });
            return BadRequest(ApiResponse<AuthDataResponse>.ValidationErrorResponse(errors, "Invalid model state"));
        }

        try
        {
            // Check if user already exists
            var existingUser = await _mongoDbService.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return Conflict(ApiResponse<AuthDataResponse>.ErrorResponse("This email is already registered", 409));
            }

            // Validate password match
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(ApiResponse<AuthDataResponse>.ErrorResponse("Passwords do not match", 400));
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

                    _logger.LogInformation($"New user {newUser.Email} registered successfully");

                    // Send welcome email asynchronously (fire and forget but with better logging)
                    #pragma warning disable CS4014
                    _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FullName).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            _logger.LogError($"Failed to send welcome email to {newUser.Email}: {task.Exception?.Message}");
                        }
                        else if (task.Result)
                        {
                            _logger.LogInformation($"Welcome email sent successfully to {newUser.Email}");
                        }
                        else
                        {
                            _logger.LogWarning($"Welcome email sending returned false for {newUser.Email}");
                        }
                    });
                    #pragma warning restore CS4014

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

                    return StatusCode(201, ApiResponse<AuthDataResponse>.SuccessResponse(authData, "Registration successful! Welcome!", 201));
                }
            }

            return BadRequest(ApiResponse<AuthDataResponse>.ErrorResponse("Error creating account. Please try again", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during registration: {ex.Message}");
            return StatusCode(500, ApiResponse<AuthDataResponse>.ServerErrorResponse("An error occurred during registration"));
        }
    }

    /// <summary>
    /// Get current user profile by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("profile/{email}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Email is required", 400));
            }

            var user = await _mongoDbService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.NotFoundResponse("User not found"));
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto, "Profile retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving profile: {ex.Message}");
            return StatusCode(500, ApiResponse<UserDto>.ServerErrorResponse("Error retrieving profile"));
        }
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="request">Profile update request</param>
    /// <returns>Updated profile information</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Server error</response>
    [HttpPut("profile/update")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request?.Email))
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Email is required", 400));
            }

            // Get existing user
            var user = await _mongoDbService.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.NotFoundResponse("User not found"));
            }

            // Update only allowed fields
            if (!string.IsNullOrEmpty(request.FullName))
            {
                user.FullName = request.FullName;
            }

            user.UpdatedAt = DateTime.UtcNow;

            // Save updated user
            bool updated = await _mongoDbService.UpdateUserAsync(user.Id, user);
            if (!updated)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Failed to update profile", 400));
            }

            _logger.LogInformation($"User {user.Email} profile updated successfully");

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto, "Profile updated successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating profile: {ex.Message}");
            return StatusCode(500, ApiResponse<UserDto>.ServerErrorResponse("Error updating profile"));
        }
    }

    /// <summary>
    /// Test email sending - sends a test welcome email
    /// </summary>
    /// <param name="request">Email test request</param>
    /// <returns>Email send status</returns>
    /// <response code="200">Test email sent successfully or with details</response>
    [HttpPost("test-email")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestEmail([FromBody] EmailTestRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.Email) || string.IsNullOrEmpty(request?.Name))
            {
                return BadRequest(ApiResponse.ErrorResponse("Email and Name are required", 400));
            }

            _logger.LogInformation($"Attempting to send test email to {request.Email}");
            
            var result = await _emailService.SendWelcomeEmailAsync(request.Email, request.Name);
            
            if (result)
            {
                return Ok(ApiResponse.SuccessResponse(null, "Test email sent successfully!", 200));
            }
            else
            {
                return Ok(ApiResponse.ErrorResponse("Failed to send test email - check logs for details", 500));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in test-email endpoint: {ex.Message}\n{ex.StackTrace}");
            return Ok(ApiResponse.ErrorResponse($"Email test failed: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Validate JWT token - checks if token is still valid
    /// </summary>
    /// <param name="request">Token validation request</param>
    /// <returns>Token validity status</returns>
    /// <response code="200">Token is valid</response>
    /// <response code="401">Token is invalid or expired</response>
    [HttpPost("validate-token")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
    {
        if (string.IsNullOrEmpty(request?.Token))
        {
            return BadRequest(ApiResponse.ErrorResponse("Token is required", 400));
        }

        try
        {
            var principal = _jwtService.ValidateToken(request.Token);
            if (principal != null)
            {
                return Ok(ApiResponse.SuccessResponse(null, "Token is valid", 200));
            }

            return Unauthorized(ApiResponse.UnauthorizedResponse("Token is invalid or expired"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating token: {ex.Message}");
            return Unauthorized(ApiResponse.UnauthorizedResponse("Token validation failed"));
        }
    }
}

/// <summary>
/// Request model for token validation
/// </summary>
public class TokenValidationRequest
{
    /// <summary>
    /// JWT token to validate
    /// </summary>
    public string? Token { get; set; }
}

/// <summary>
/// Request model for email testing
/// </summary>
public class EmailTestRequest
{
    /// <summary>
    /// Email address to send test email to
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Name of the recipient
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Request model for profile update
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// User email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Updated full name
    /// </summary>
    public string? FullName { get; set; }
}
