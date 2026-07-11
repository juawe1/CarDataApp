using Bmw.Dashboard.Core.Interfaces;
using Windows.Security.Credentials;

namespace Bmw.Dashboard.Core.Services;

public class PasswordVaultService : IPasswordVaultService
{
    private const string resource = "BMW_CarData_App";
    private const string account = "ActiveUser";
    public string GetRefreshToken()
    {
        var vault = new PasswordVault();
        try
        {
            var cred = vault.Retrieve(resource, account);
            cred.RetrievePassword();
            return cred.Password;

        }
        catch
        {
            // No token found, return empty string
            return string.Empty;
        }
    }

    public void SaveTokens(string refreshToken)
    {
        var vault = new PasswordVault();

        try
        {
            var existing = vault.Retrieve(resource, account);
            vault.Remove(existing);
        }
        catch
        {
            // No existing token, ignore
        }

        var cred = new PasswordCredential(resource, account, refreshToken);
        vault.Add(cred);
    }
}
