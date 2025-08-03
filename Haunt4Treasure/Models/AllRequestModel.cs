using Google.Apis.Gmail.v1.Data;
using Newtonsoft.Json;

namespace Haunt4Treasure.Models;

public class AllRequestModel
{
}

#region AddUser
public class AddUserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool AgeConfirmed { get; set; }
    public bool IsEmailUser { get; set; }
}
public class ExternalInternalRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool AgeConfirmed { get; set; }
}
#endregion
public record ProfileEdit(string bankName, string accountNumber,IFormFile? profilePic);
public record GameCashOut(Guid sessionId, bool fiftyfifty, bool skipped, int numberOfAnsweredQuestions, decimal cashoutAmount);
public class LoginModel
{
    public string Email { get; set; }
    public string? Password { get; set; }
}
public class ExternalLoginRequest
{
    public string AccessToken { get; set; }
}
public class ApplicationLoginLog
{
    public string ModeId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public string PictureUrl { get; set; }
    public string LastLogin { get; set; }
    public string RawResponse { get; set; }
}