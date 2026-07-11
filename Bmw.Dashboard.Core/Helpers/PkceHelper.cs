using System.Security.Cryptography;
using System.Text;

namespace Bmw.Dashboard.Core.Helpers;

public static class PkceHelper
{
    public static(string verifier, string challenge) Generate() 
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
        string verifier = Base64UrlEncoder.Encode(randomBytes);

        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(verifier));
        string challenge = Base64UrlEncoder.Encode(hash);

        return (verifier, challenge);
    }
}
