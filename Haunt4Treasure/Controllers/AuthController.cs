using System.Text.Json;
using Google.Apis.Auth;
using Haunt4Treasure.Models;
using Haunt4Treasure.RegistrationFlow;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Haunt4Treasure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service) : ControllerBase
{
    private readonly IAuthService _googleService = service;
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLoginSignUp([FromBody] ExternalLoginRequest idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken.AccessToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "335471388791-gr465stv6lcha6ku8sisre5dqlolirui.apps.googleusercontent.com" }
            });
            var rec = new ExternalInternalRequest
            {
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Email = payload.Email,
                PhoneNumber = string.Empty, // Google does not provide phone number by default
                Password = string.Empty, // Password is not used for external login
                ProfileImagePath = payload.Picture,
                AgeConfirmed = true, // Assuming age confirmation is true for external users
            };
            var res = await _googleService.ProcessInternalUser(rec, 2);

            return Ok(res);
        }
        catch (InvalidJwtException)
        {
            return Unauthorized("Invalid Google token");
        }
    }

    [HttpPost("Internal")]
    public async Task<IActionResult> InternalSignUp([FromBody] ExternalInternalRequest request)
    {
        try
        {
            var res = await _googleService.ProcessInternalUser(request, 1);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("SignIn")]
    public async Task<IActionResult> SignIn([FromBody] LoginModel request)
    {
        try
        {
            var res = await _googleService.ProcessUserLogin(request);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
