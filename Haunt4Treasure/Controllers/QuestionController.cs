using System.Text.Json;
using Haunt4Treasure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haunt4Treasure.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController : ControllerBase
{
    private readonly HauntDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public QuestionController(HauntDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("AddQuestion")]
    public async Task<IActionResult> AddQuestion()
    {
        string apiUrl = $"https://opentdb.com/api.php?amount=49";

        using var httpClient = new HttpClient();
        string jsonResponse = await httpClient.GetStringAsync(apiUrl);

        var root = JsonSerializer.Deserialize<TriviaApiResponse>(jsonResponse);

        if (root?.Results != null)
        {
            // using var db = new HauntDbContext();

            foreach (var item in root.Results)
            {
                var recordCount = item.IncorrectAnswers
    .Select(ans => System.Net.WebUtility.HtmlDecode(ans))
    .Append(System.Net.WebUtility.HtmlDecode(item.CorrectAnswer))
    .Distinct()
    .ToList();
                if (recordCount.Count() == 4)
                {
                    var question = new Question
                    {
                        Id = Guid.NewGuid(),
                        Text = System.Net.WebUtility.HtmlDecode(item.Question),
                        CorrectAnswer = System.Net.WebUtility.HtmlDecode(item.CorrectAnswer),
                        Category = System.Net.WebUtility.HtmlDecode(item.Category),
                        Difficulty = item.Difficulty,
                        Options = item.IncorrectAnswers
        .Select(ans => System.Net.WebUtility.HtmlDecode(ans))
        .Append(System.Net.WebUtility.HtmlDecode(item.CorrectAnswer))
        .Distinct()
        .ToList()
                    };

                    _dbContext.Questions.Add(question);
                }
            }

            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Questions inserted successfully.");
        }
        else
        {
            Console.WriteLine("No results found.");
        }

        return Ok(new { message = "Questions added successfully." });
    }

    [Authorize]
    [HttpPost("GetQuestions")]
    public async Task<ReturnObject> GetQuestions(decimal amountStaked)
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
        var gameSession = new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userId),
            AmountStaked = amountStaked,
            Status = "InProgress",
            UsedSkip = false,
            UsedFiftyFifty = false,
            StartedAt = DateTime.UtcNow,
            NumberOfAnsweredGame = 0
        };
        _dbContext.GameSessions.Add(gameSession);
        await _dbContext.SaveChangesAsync();
        var questions =await  _dbContext.Questions.GroupBy(q => q.Text)           // group by question text
    .Select(g => g.First())         // pick first question from each group
    .OrderBy(q => Guid.NewGuid())   // shuffle randomly
    .Take(25)
    .ToListAsync();
        var res = new ReturnObject
        {
            Data = questions,
            Status = true,
            Message = "Record Found Successfully"
        };
        return res;
    }

    [HttpGet("GetSampleQuestions")]
    public async Task<ReturnObject> GetSampleQuestions()
    {
        var questions = await _dbContext.Questions
         .Where(q => q.Difficulty.ToLower() == "easy")
            .GroupBy(q => q.Text)           // group by question text
    .Select(g => g.First())         // pick first question from each group
    .OrderBy(q => Guid.NewGuid())   // shuffle randomly
    .Take(5)
    .ToListAsync();

        var res = new ReturnObject
        {
            Data = questions,
            Status = true,
            Message = "Record Found Successfully"
        };
        return res;
    }

}