using Azure.Core;
using Google.Apis.PeopleService.v1.Data;
using Haunt4Treasure.Helpers;
using Haunt4Treasure.Models;
using Haunt4Treasure.Repository;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Haunt4Treasure.RegistrationFlow
{
    public interface IAuthService
    {
        Task<ReturnObject> ProcessInternalUser(ExternalInternalRequest request, int source);
        Task<ReturnObject> ProcessUserLogin(LoginModel encryptedData);
    }
    public class AuthService(IConfiguration config, IAuthRepository authRepo, IAuthenticationHelpers authHelper) : IAuthService
    {
        private readonly IConfiguration _config = config;
        private readonly IAuthenticationHelpers _authHelper = authHelper;
        private readonly IAuthRepository _authRepo = authRepo;

        public async Task<ReturnObject> ProcessInternalUser(ExternalInternalRequest request, int source)
        {
            var user = new User();
            var userInfo = new ReturnObject();
            var userId = "";
            try
            {
                var tokD = new UserTokenDetails();
                (user, decimal balance) = await _authRepo.GetUserAsync(request.Email);
                if (user == null)
                {
                    var newUser = new AddUserRequest();
                    newUser.Email = request.Email;
                    newUser.FirstName = request.FirstName;
                    newUser.LastName = request.LastName;
                    newUser.PhoneNumber = request.PhoneNumber;
                    newUser.ProfileImagePath = request.ProfileImagePath;
                    tokD = new UserTokenDetails
                    {
                        Email = request.Email,
                        FullName = $"{request.FirstName} {request.LastName}",
                        PhoneNumber = request.PhoneNumber ?? "",
                        PictureUrl = request.ProfileImagePath ?? "",
                        Token = _config["XapiKey"] ?? ""
                    };
                    switch (source)
                    {
                        case 1:
                            var (hash, salt) = PasswordHelper.CreatePasswordHash(request.Password);
                            newUser.IsEmailUser = false;
                            newUser.Password = hash;
                            newUser.PasswordSalt = salt;
                            tokD.LoginChannel = "Internal";
                            tokD.ModeType = "Internal";
                            break;
                        case 2:
                            newUser.IsEmailUser = true;
                            newUser.Password = "";
                            tokD.LoginChannel = "External";
                            tokD.ModeType = "External";
                            break;
                        default:
                            break;
                    }
                    userId = await _authRepo.AddUserAsync(newUser);
                    tokD.ModeId = userId;
                }
                else
                {
                    tokD = new UserTokenDetails
                    {
                        Email = request.Email,
                        FullName = $"{request.FirstName} {request.LastName}",
                        PhoneNumber = request.PhoneNumber ?? "",
                        PictureUrl = request.ProfileImagePath ?? "",
                        Token = _config["XapiKey"] ?? ""
                    };

                    tokD.ModeId = user.Id.ToString();
                    switch (source)
                    {
                        case 1:
                            tokD.LoginChannel = "Internal";
                            tokD.ModeType = "Internal";
                            break;
                        case 2:
                            tokD.LoginChannel = "External";
                            tokD.ModeType = "External";
                            break;
                        default:
                            break;
                    }
                }
                // 4. Generate JWT token
                (string token, string refreshToken, string time) = CreateAccessToken(tokD);
                return new ReturnObject
                {
                    Status = true,
                    Message = $"Login Is Successful",
                    Data = new LoginResponse
                    {
                        Balance = balance,
                        FullName = $"{request.FirstName} {request.LastName}",
                        Token = token,
                        RefreshToken = refreshToken,
                        Expiration = time
                    }
                };
            }
            catch (Exception ex)
            {
                return new ReturnObject
                {
                    Status = false,
                    Message = $"An Error Occured During Login"
                };
            }
        }
        public async Task<ReturnObject> ProcessUserLogin(LoginModel request)
        {
            var user = new User();
            var userInfo = new ReturnObject();
            var userId = "";
            try
            {
                var tokD = new UserTokenDetails();
                (user, decimal balance) = await _authRepo.GetUserAsync(request.Email);
                if (user == null)
                {
                    return new ReturnObject
                    {
                        Status = false,
                        Message = $"User Not Found"
                    };
                }
                if(!user.IsEmailUser)
                {
                    // Verify password
                    if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                    {
                        return new ReturnObject
                        {
                            Status = false,
                            Message = $"Invalid Login Credential"
                        };
                    }
                }
               
                tokD = new UserTokenDetails
                {
                    Email = request.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    PhoneNumber = user.PhoneNumber ?? "",
                    PictureUrl = user.ProfileImagePath ?? "",
                    Token = _config["XapiKey"] ?? ""
                };

                tokD.LoginChannel = "Internal";
                tokD.ModeType = "Internal";
                tokD.ModeId = user.Id.ToString();

                // 4. Generate JWT token
                (string token, string refreshToken, string time) = CreateAccessToken(tokD);
                return new ReturnObject
                {
                    Status = true,
                    Message = $"Login Is Successful",
                    Data = new LoginResponse
                    {
                        Balance = balance,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Token = token,
                        RefreshToken = refreshToken,
                        Expiration = time
                    }
                };
            }
            catch (Exception ex)
            {
                return new ReturnObject
                {
                    Status = false,
                    Message = $"An Error Occured During Login"
                };
            }
        }
        //Generating access token
        private (string token, string refreshToken, string expiration) CreateAccessToken(UserTokenDetails user)
        {
            // Define claims
            var claims = new List<Claim>
            {
                new Claim("PictureUrl", user.PictureUrl ?? ""),
                new Claim("UserId", user.ModeId),
                new Claim("ModeType", user.ModeType),
                new Claim("Email", user.Email),
                new Claim("Channel", user.LoginChannel),
                new Claim("ExpiryDate", DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:AccessTime"]!)).ToString()),
                new Claim("FullName", user.FullName),
                //new Claim("PhoneNumber", user.PhoneNumber),
                new Claim(JwtRegisteredClaimNames.Sub, user.ModeId)
            };

            // Get JWT key and ensure it's at least 64 bytes for HmacSha512
            var jwtKey = _config["Jwt:Key"]!;
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            if (keyBytes.Length < 64)
            {
                throw new InvalidOperationException("JWT key must be at least 64 bytes for HmacSha512.");
            }
            var symmetricKey = new SymmetricSecurityKey(keyBytes);

            // Create token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:AccessTime"]!)),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha512Signature)
            };

            // Generate token and refresh token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();
            var expiration = $"{_config["Jwt:AccessTime"]} Mins";

            return (stringToken, refreshToken, expiration);
        }
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        private static class PasswordHelper
        {
            public static (string hash, string salt) CreatePasswordHash(string password)
            {
                byte[] saltBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }

                byte[] hashBytes;
                using (var sha256 = SHA256.Create())
                {
                    var combined = Combine(saltBytes, Encoding.UTF8.GetBytes(password));
                    hashBytes = sha256.ComputeHash(combined);
                }

                string hash = Convert.ToBase64String(hashBytes);
                string salt = Convert.ToBase64String(saltBytes);

                return (hash, salt);
            }

            public static bool VerifyPassword(string password, string base64Hash, string base64Salt)
            {
                var saltBytes = Convert.FromBase64String(base64Salt);
                var expectedHash = Convert.FromBase64String(base64Hash);

                using (var sha256 = SHA256.Create())
                {
                    var combined = Combine(saltBytes, Encoding.UTF8.GetBytes(password));
                    var computedHash = sha256.ComputeHash(combined);
                    return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
                }
            }

            private static byte[] Combine(byte[] first, byte[] second)
            {
                var result = new byte[first.Length + second.Length];
                Buffer.BlockCopy(first, 0, result, 0, first.Length);
                Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
                return result;
            }
        }
    }
}
