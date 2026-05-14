using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Authentication;

public sealed class CdpJwtGenerator
{
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Kept as an instance member because the generator is resolved through DI by the auth handler.")]
    public string Generate(string keyName, string privateKeyPem, string method, string host, string path, int lifetimeSeconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyPem);

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKeyPem);

        var securityKey = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = keyName,
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var uri = $"{method.ToUpperInvariant()} {host}{path}";

        var header = new JwtHeader(credentials, null, JwtConstants.HeaderType);
        header["kid"] = keyName;
        header["nonce"] = GenerateNonce();

        var payload = new JwtPayload
        {
            { "sub", keyName },
            { "iss", "cdp" },
            { "nbf", now },
            { "exp", now + lifetimeSeconds },
            { "uri", uri }
        };

        var token = new JwtSecurityToken(header, payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateNonce()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
