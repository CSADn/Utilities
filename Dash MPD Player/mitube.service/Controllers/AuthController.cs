using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mitube.service.Services;
using mitube.service.Services.Repositories;

namespace mitube.service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserRepository userRepo, IJwtService jwt, ILogger<AuthController> logger)
    {
        _userRepo = userRepo;
        _jwt = jwt;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        string? username = null, password = null;
        var ct = Request.ContentType?.ToLowerInvariant() ?? "";

        if (ct.Contains("application/json"))
        {
            var body = await System.Text.Json.JsonSerializer.DeserializeAsync<LoginRequest>(Request.Body,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body != null) { username = body.Username; password = body.Password; }
        }
        else if (ct.Contains("application/x-www-form-urlencoded"))
        {
            var form = await Request.ReadFormAsync();
            username = form["username"];
            password = form["password"];
        }
        else
        {
            return BadRequest(new { error = "Unsupported Content-Type" });
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return BadRequest(new { error = "Username and password are required" });

        var user = await _userRepo.GetByUsernameAsync(username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", username);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        var token = _jwt.GenerateToken(user.Username, user.Id);
        _logger.LogInformation("User logged in: {Username}", user.Username);

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresIn = 86400,
            Username = user.Username,
            DisplayName = user.DisplayName
        });
    }

    [AllowAnonymous]
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogInformation("PING received from {RemoteIp}", HttpContext.Connection.RemoteIpAddress);
        return Content("pong", "text/plain");
    }

    [Authorize]
    [HttpGet("validate")]
    public IActionResult Validate()
    {
        return Ok(new { valid = true, username = User.Identity?.Name });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public int ExpiresIn { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
}
