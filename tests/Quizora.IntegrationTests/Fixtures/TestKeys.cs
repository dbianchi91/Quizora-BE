using System.Security.Cryptography;

namespace Quizora.IntegrationTests.Fixtures;

public static class TestKeys
{
    private static readonly Lazy<(string Private, string Public)> _keys = new(() =>
    {
        using var rsa = RSA.Create(2048);
        var priv = rsa.ExportRSAPrivateKeyPem();
        var pub = rsa.ExportSubjectPublicKeyInfoPem();
        return (priv, pub);
    });

    public static string PrivateKeyPem => _keys.Value.Private;
    public static string PublicKeyPem => _keys.Value.Public;
}
