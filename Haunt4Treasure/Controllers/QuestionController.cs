using System.Text.Json;
using Haunt4Treasure.Models;
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


    [HttpGet("GetQuestions")]
    public async Task<ReturnObject> GetQuestions()
    {
        //sql
        //        var rawQuery = @"SELECT TOP 25 *
        //FROM Questions
        //WHERE Text NOT LIKE '%\u0022%'
        //  AND Text NOT LIKE '%\u00AE%'
        //  AND Text NOT LIKE '%\u2122%'
        //  AND Text NOT LIKE '%\u0027%'
        //  AND Options NOT LIKE '%\u0027%'
        //  AND Options NOT LIKE '%\u0022%'
        //  AND Options NOT LIKE '%\u00AE%'
        //  AND Options NOT LIKE '%\u2122%'
        //ORDER BY NEWID();";


        var questions =await  _dbContext.Questions.GroupBy(q => q.Text)           // group by question text
    .Select(g => g.First())         // pick first question from each group
    .OrderBy(q => Guid.NewGuid())   // shuffle randomly
    .Take(25)
    .ToListAsync();
        //        var rawQuery = @"SELECT *
        //FROM public.Questions
        //WHERE text NOT LIKE '%""%'      -- \u0022 = double quote
        //  AND text NOT LIKE '%®%'      -- \u00AE = registered trademark
        //  AND text NOT LIKE '%™%'      -- \u2122 = trademark
        //  AND text NOT LIKE '%''%'     -- \u0027 = single quote
        //  AND options NOT LIKE '%''%'
        //  AND options NOT LIKE '%""%' 
        //  AND options NOT LIKE '%®%' 
        //  AND options NOT LIKE '%™%' 
        //ORDER BY RANDOM()
        //LIMIT 25;
        //";
        //        var rawQuestions = await _dbContext.Set<QuestionRawDto>()
        //        .FromSqlRaw(rawQuery)
        //        .ToListAsync();

        //        var questions = rawQuestions.Select(q => new Question
        //        {
        //            Id = q.Id,
        //            Text = q.Text,
        //            Options = JsonSerializer.Deserialize<List<string>>(q.Options ?? "[]"),
        //            CorrectAnswer = q.CorrectAnswer,
        //            Category = q.Category,
        //            Difficulty = q.Difficulty
        //        }).ToList();

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