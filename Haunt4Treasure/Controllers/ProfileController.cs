using Haunt4Treasure.Models;
using Haunt4Treasure.RegistrationFlow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Haunt4Treasure.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProfileController(IAllService service) : ControllerBase
{
    private readonly IAllService _allService = service;

    [HttpPost("GetDetial")]
    public async Task<ReturnObject> GetDetial()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new ReturnObject
            {
                Status = false,
                Message = "User not authenticated"
            };
        }
        var result = await _allService.GetDetial(userId);
        return result;
    }

    [HttpPost("UpdateUser")]
    public async Task<ReturnObject> UpdateUser(ProfileEdit GC)
    {
        //no userid is coming from token extract it from token
        var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new ReturnObject
            {
                Status = false,
                Message = "User not authenticated"
            };
        }
        var result = await _allService.UpdateUser(GC);
        return result;
    }
}