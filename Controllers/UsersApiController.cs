using Microsoft.AspNetCore.Mvc;
using FirstMvcApp.Models;
using FirstMvcApp.Services;

namespace FirstMvcApp.Controllers;

/// <summary>
/// API endpoints for user management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersApiController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersApiController> _logger;

    public UsersApiController(IUserService userService, ILogger<UsersApiController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    /// <response code="200">Returns list of users</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<User>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<User>>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<List<User>>.SuccessResponse(users, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting users: {ex.Message}");
            return StatusCode(500, ApiResponse<List<User>>.ServerErrorResponse("Error retrieving users"));
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User object</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">If user not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<User>>> GetUserById(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<User>.NotFoundResponse("User not found"));

            return Ok(ApiResponse<User>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting user: {ex.Message}");
            return StatusCode(500, ApiResponse<User>.ServerErrorResponse("Error retrieving user"));
        }
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User object</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">If user not found</response>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<User>>> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(ApiResponse<User>.NotFoundResponse("User not found"));

            return Ok(ApiResponse<User>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting user by email: {ex.Message}");
            return StatusCode(500, ApiResponse<User>.ServerErrorResponse("Error retrieving user"));
        }
    }

    /// <summary>
    /// Update user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="user">Updated user object</param>
    /// <returns>Success message</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(ApiResponse.ErrorResponse("User ID is required", 400));

            user.Id = id;
            var updated = await _userService.UpdateUserAsync(id, user);
            
            if (!updated)
                return NotFound(ApiResponse.NotFoundResponse("User not found"));

            return Ok(ApiResponse.SuccessResponse(null, "User updated successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user: {ex.Message}");
            return StatusCode(500, ApiResponse.ServerErrorResponse("Error updating user"));
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">User deleted successfully</response>
    /// <response code="404">User not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(ApiResponse.ErrorResponse("User ID is required", 400));

            var deleted = await _userService.DeleteUserAsync(id);
            
            if (!deleted)
                return NotFound(ApiResponse.NotFoundResponse("User not found"));

            return Ok(ApiResponse.SuccessResponse(null, "User deleted successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting user: {ex.Message}");
            return StatusCode(500, ApiResponse.ServerErrorResponse("Error deleting user"));
        }
    }
}
