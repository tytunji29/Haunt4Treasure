using System.Text.Json.Serialization;

namespace Haunt4Treasure.Models;

public class AllResponseModel
{
}
public class QuestionRawDto
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public string Options { get; set; }
    public string CorrectAnswer { get; set; }
    public string Category { get; set; }
    public string Difficulty { get; set; }
}

public class ReturnObject
{
    public bool Status { get; set; }
    public dynamic Data { get; set; }
    public string Message { get; set; }
}
public class TriviaApiResponse
{
    [JsonPropertyName("response_code")]
    public int ResponseCode { get; set; }

    [JsonPropertyName("results")]
    public List<TriviaQuestionDto> Results { get; set; }
}
public class UserTokenDetails
{
    public string ModeType { get; set; }
    public string ModeId { get; set; }
    public string Token { get; set; }
    public string Email { get; set; }
    public string LoginChannel { get; set; }
    public string PictureUrl { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
}

public class LoginResponse
{
    public decimal Balance { get; set; }
    public string? FullName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
    public List<QuestionCategory> Category { get; set; }
}
public class TriviaQuestionDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("question")]
    public string Question { get; set; }

    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; }

    [JsonPropertyName("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; }
}