using System.Text.Json;
using Haunt4Treasure.Models;
using Haunt4Treasure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haunt4Treasure.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController(IAllService service) : ControllerBase
{
    private readonly IAllService _allService = service;
   // private readonly IHttpClientFactory _httpClientFactory;

    [HttpPost("AddQuestion")]
    public async Task<IActionResult> AddQuestion()
    {
      
        return Ok(await _allService.PostQuestion());
    }

    [Authorize]
    [HttpPost("GetQuestions")]
    public async Task<ReturnObject> GetQuestions(decimal amountStaked, Guid? category)
    {
        //take this to repo where you save the game session and exact the userId from the token
        var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new ReturnObject
            {
                Status = false,
                Message = "User not authenticated"
            };
        }
        var res = await _allService.ProcessQuestions(userId, amountStaked,category);
        return res;
    }

    [HttpGet("GetQuestionCategory")]
    public async Task<ReturnObject> GetQuestionCategory()
    {
        return await _allService.ProcessSampleQuestionsCategories();
    }
    [HttpGet("GetSampleQuestions")]
    public async Task<ReturnObject> GetSampleQuestions()
    {
        return await _allService.ProcessSampleQuestions();
    }

}