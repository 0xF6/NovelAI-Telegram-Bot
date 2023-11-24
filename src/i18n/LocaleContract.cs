namespace nai.i18n;

public class LocaleContract : ILocaleContract
{
    public string StartMessage { get; set; }
    public string SentAuthRequest { get; set; }
    public string AuthSuccess { get; set; }
    public string AuthFailed { get; set; }
    public string YourBalance { get; set; }
    public string CoinsGranted { get; set; }
    public string InsufficientBalance { get; set; }
}