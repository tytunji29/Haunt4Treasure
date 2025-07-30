using System.Text.Json;
using Haunt4Treasure.Models;
using Haunt4Treasure.RegistrationFlow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haunt4Treasure.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController(IAllService service, IHttpClientFactory httpClientFactory) : ControllerBase
{
    private readonly IAllService _allService = service;
    private readonly IHttpClientFactory _httpClientFactory;

    //[HttpPost("AddQuestion")]
    //public async Task<IActionResult> AddQuestion()
    //{
    //    string apiUrl = $"https://opentdb.com/api.php?amount=49";

    //    using var httpClient = new HttpClient();
    //    string jsonResponse = await httpClient.GetStringAsync(apiUrl);

    //    var root = JsonSerializer.Deserialize<TriviaApiResponse>(jsonResponse);

    //    if (root?.Results != null)
    //    {
    //        // using var db = new HauntDbContext();

    //        foreach (var item in root.Results)
    //        {
    //            var recordCount = item.IncorrectAnswers
    //.Select(ans => System.Net.WebUtility.HtmlDecode(ans))
    //.Append(System.Net.WebUtility.HtmlDecode(item.CorrectAnswer))
    //.Distinct()
    //.ToList();
    //            if (recordCount.Count() == 4)
    //            {
    //                var question = new Question
    //                {
    //                    Id = Guid.NewGuid(),
    //                    Text = System.Net.WebUtility.HtmlDecode(item.Question),
    //                    CorrectAnswer = System.Net.WebUtility.HtmlDecode(item.CorrectAnswer),
    //                    Category = System.Net.WebUtility.HtmlDecode(item.Category),
    //                    Difficulty = item.Difficulty,
    //                    Options = item.IncorrectAnswers
    //    .Select(ans => System.Net.WebUtility.HtmlDecode(ans))
    //    .Append(System.Net.WebUtility.HtmlDecode(item.CorrectAnswer))
    //    .Distinct()
    //    .ToList()
    //                };

    //                _dbContext.Questions.Add(question);
    //            }
    //        }

    //        await _dbContext.SaveChangesAsync();
    //        Console.WriteLine("Questions inserted successfully.");
    //    }
    //    else
    //    {
    //        Console.WriteLine("No results found.");
    //    }

    //    return Ok(new { message = "Questions added successfully." });
    //}

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