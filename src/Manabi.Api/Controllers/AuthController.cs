using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Manabi.Api.Models;
using Manabi.Api.Services;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        if (await userManager.FindByEmailAsync(req.Email) != null)
            return Conflict("このメールアドレスはすでに使用されています。");

        var user = new AppUser
        {
            UserName = req.Email,
            Email = req.Email,
            DisplayName = req.DisplayName,
            Role = req.Role
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var (token, expiresAt) = tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user == null || user.IsDeleted)
            return Unauthorized("メールアドレスまたはパスワードが正しくありません。");

        var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized("メールアドレスまたはパスワードが正しくありません。");

        var (token, expiresAt) = tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponse>> Me()
    {
        var userId = User.FindFirst("sub")?.Value
                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var user = await userManager.FindByIdAsync(userId!);
        if (user == null || user.IsDeleted) return Unauthorized();

        var (token, expiresAt) = tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        });
    }
}
