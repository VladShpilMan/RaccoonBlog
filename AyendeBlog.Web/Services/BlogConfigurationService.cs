namespace AyendeBlog.Web.Services;

public class BlogConfigurationService
{
    private readonly IConfiguration _configuration;

    private (string Id, string Secret)? _microsoftOAuthKeys;
    private (string Id, string Secret)? _googleOAuthKeys;
    private (string Id, string Secret)? _twitterOAuthKeys;
    private (string Id, string Secret)? _facebookOAuthKeys;
    
    public (string Id, string Secret)? MicrosoftOAuthKeys => 
        _microsoftOAuthKeys ??= GetKeys("Microsoft", "ClientId", "ClientSecret");

    public (string Id, string Secret)? GoogleOAuthKeys => 
        _googleOAuthKeys ??= GetKeys("Google", "ClientId", "ClientSecret");

    public static (string Id, string Secret)? TwitterOAuthKeys
    {
        get
        {
            return null;
        }
    }

    public (string Id, string Secret)? FacebookOAuthKeys => 
        _facebookOAuthKeys ??= GetKeys("Facebook", "AppId", "AppSecret");

    public string MainBlogUrl => 
        _configuration?["MainUrl"] ?? string.Empty;

    public BlogConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    private (string Id, string Secret)? GetKeys(string provider, string idKey, string secretKey)
    {
        var baseKey = "Raccoon/OAuth/" + provider;
        var fullIdKey = (baseKey + "/" + idKey).Replace("/", ":");
        var fullSecretKey = (baseKey + "/" + secretKey).Replace("/", ":");

        var id = _configuration[fullIdKey];
        var secret = _configuration[fullSecretKey];

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret))
            return null;

        return (id, secret);
    }
}