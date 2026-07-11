namespace Bmw.Dashboard.Core.Helpers;

public static class Base64UrlEncoder
{
    public static string Encode(byte[] input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
