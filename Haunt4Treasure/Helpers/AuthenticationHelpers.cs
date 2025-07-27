using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using Haunt4Treasure.Models;
using Newtonsoft.Json;
using System;

namespace Haunt4Treasure.Helpers;
public interface IAuthenticationHelpers
{
    Task<ReturnObject> GetGoogleUserProfileAsync(string accessToken);
}
public class AuthenticationHelpers : IAuthenticationHelpers
{
    public async Task<ReturnObject> GetGoogleUserProfileAsync(string accessToken)
    {
        try
        {
            // Step 1: Create the credential from the access token
            var credential = GoogleCredential.FromAccessToken(accessToken);

            // Step 2: Initialize the PeopleService client
            var peopleService = new PeopleServiceService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Haunt4Treasure"
            });

            // Step 3: Call the People API to get user profile
            var request = peopleService.People.Get("people/me");
            request.PersonFields = "addresses,ageRanges,biographies,birthdays,calendarUrls,clientData,coverPhotos," +
                "emailAddresses,events,externalIds,genders,imClients,interests,locales,locations,memberships,metadata," +
                "names,nicknames,occupations,organizations,phoneNumbers,photos,relations,relationshipInterests,relationshipStatuses," +
                "residences,skills,taglines,urls,userDefined";

            Person profile = await request.ExecuteAsync();

            // Step 4: Extract user info
            var user = new ApplicationLoginLog
            {
                ModeId = profile.ResourceName.Replace("people/", ""),
                Email = profile.EmailAddresses?.FirstOrDefault()?.Value ?? string.Empty,
                FullName = profile.Names?.FirstOrDefault()?.DisplayName,
                GivenName = profile.Names?.FirstOrDefault()?.GivenName,
                FamilyName = profile.Names?.FirstOrDefault()?.FamilyName,
                PictureUrl = profile.Photos?.FirstOrDefault()?.Url,
                LastLogin = DateTime.UtcNow.ToString(),
                RawResponse = JsonConvert.SerializeObject(profile)
            };
            return new ReturnObject
            {
                Status = true,
                Message = "User profile retrieved successfully",
                Data = user
            };
        }
        catch (Exception ex)
        {
            return new ReturnObject
            {
                Status = false,
                Message = ex.Message
            };
        }
    }
}