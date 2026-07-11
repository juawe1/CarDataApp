namespace Bmw.Dashboard.Core.Interfaces;

public interface IPasswordVaultService
{
    void SaveTokens(string refreshToken);
    string GetRefreshToken();
}
