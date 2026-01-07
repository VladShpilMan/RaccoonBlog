using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace AyendeBlog.Web.Services;

public class Recaptcha2Service
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    
    public const string ModelStateErrorKey = "CaptchaNotValid";
    private const string RecaptchaResponseFieldName = "g-recaptcha-response";
    
    private string? RecaptchaSecret => _config["Recaptcha:Secret"];
    private string? SiteKey => _config["Recaptcha:SiteKey"];
    
    public Recaptcha2Service(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<bool> Validate(ModelStateDictionary modelState, string? recaptchaResponse)
    {
        if (string.IsNullOrEmpty(recaptchaResponse))
        {
            modelState.AddModelError(ModelStateErrorKey, "Captcha response not supplied.");
            return false;
        }

        var result = await VerifyResponse(recaptchaResponse);
        
        if (result.IsValid)
            return true;

        modelState.AddModelError(ModelStateErrorKey, result.ErrorMessage ?? "Captcha is invalid.");
        return false;
    }
    
    private async Task<CaptchaVerificationResult> VerifyResponse(string response)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", RecaptchaSecret!),
                new KeyValuePair<string, string>("response", response)
            });

            var apiResponse = await _httpClient.PostAsync("/recaptcha/api/siteverify", content);
            apiResponse.EnsureSuccessStatusCode();
            
            var responseObj = await apiResponse.Content.ReadFromJsonAsync<JObject>();

            if (responseObj?["success"]?.Value<bool>() == true)
            {
                return CaptchaVerificationResult.Valid();
            }
        }
        catch (Exception err)
        {
            //TODO log
        }

        return CaptchaVerificationResult.Error("Captcha response is invalid. Please try again.");
    }
}

public class CaptchaVerificationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static CaptchaVerificationResult Valid() => new() { IsValid = true };
    public static CaptchaVerificationResult Error(string msg) => new() { IsValid = false, ErrorMessage = msg };
}