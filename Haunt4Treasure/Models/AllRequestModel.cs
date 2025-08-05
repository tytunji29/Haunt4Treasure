using Google.Apis.Gmail.v1.Data;
using Newtonsoft.Json;

namespace Haunt4Treasure.Models;

public class AllRequestModel
{
}

#region AddUser
public class AddUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public bool AgeConfirmed { get; set; }
    public bool IsEmailUser { get; set; }
}
public class ExternalInternalRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public bool AgeConfirmed { get; set; }
}
#endregion
public class ProfileEdit { public string BankName { get; set; } = string.Empty; public string AccountNumber { get; set; } = string.Empty; public IFormFile? profilePic { get; set; } }
public record GameCashOut(Guid SessionId, bool Fiftyfifty, bool Skipped, int NumberOfAnsweredQuestions, decimal CashoutAmount);
public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
}
public class ExternalLoginRequest
{
    public string AccessToken { get; set; } = string.Empty;
}
public class ApplicationLoginLog
{
    public string ModeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public string LastLogin { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
}