
namespace Bmw.Dashboard.Core.Models.Config;

public class BmwApiOptions
{
    public const string BmwApi = "BmwApi";
    public string ClientId { get; set; } = string.Empty;
    // Optional client secret for confidential clients. If set, BmwApiService will
    // use HTTP Basic authentication and omit client_id from the request body to
    // avoid sending multiple credentials.
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
}
