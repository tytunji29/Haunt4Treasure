namespace Haunt4Treasure.Helpers;

public class AppSettings
{
    public string? E_Key { get; set; }
    public string? E_IV { get; set; }
    public string EfcKey { get; set; } = default!;
    public string SendGridKey { get; set; } = default!;
    public string WhatsAppKey { get; set; } = default!;
    public string WhatsAppUri { get; set; } = default!;
    public string EmailFrom { get; set; } = default!;
    public string ReplyTo { get; set; } = default!;
    public string MailDisplayName { get; set; } = default!;
    public string JwtKey { get; set; } = default!;
    public int JwtExpiry { get; set; } = default!;
    public string JwtAudience { get; set; } = default!;
    public string IV { get; set; } = default!;
    public string OneAppClientId { get; set; } = default!;
    public string OneApp_X_CRE_KEY { get; set; } = default!;
    public string OneApp_SECKEY { get; set; } = default!;

    public string IdentityApiBaseUrl { get; set; } = default!;
    public string App_X_APP_ID { get; set; } = default!;
    public string AppName { get; set; } = default!;
    public string AppEncryptionToken { get; set; } = default!;
    public string CloudinaryApiKey { get; set; } = default!;
    public string CloudinarySecreteKey { get; set; } = default!;
    public string CloudinaryUsername { get; set; } = default!;
    //Wallet Service. 
    public string WalletAppId { get; set; } = default!;
    public string WalletAPIKey { get; set; } = default!;
    public string APIKEY { get; set; } = default!;
    public string AppTrialDomain { get; set; } = default!; //to add 
    public string AppSubscribedDomain { get; set; } = default!;
    public string AppCustomizeDomain { get; set; } = default!;
    public string WithJaraIcon { get; set; } = default!;
    public string DefaultImageUrl { get; set; } = default!;

    public int OtpExpiry { get; set; } = default!;

}
