using System.Security.Cryptography;

namespace Medicore.Api.Services;

public static class PasswordHashService
{
    private const int Iteraciones = 210_000;
    private const int LongitudSalt = 16;
    private const int LongitudHash = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(LongitudSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iteraciones, HashAlgorithmName.SHA256, LongitudHash);

        return $"pbkdf2-sha256${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string password, string passwordHash)
    {
        var partes = passwordHash.Split('$');
        if (partes.Length != 4 || partes[0] != "pbkdf2-sha256" ||
            !int.TryParse(partes[1], out var iteraciones) || iteraciones < 100_000)
        {
            throw new InvalidOperationException("El formato del hash de contraseña almacenado no es válido.");
        }

        var salt = Convert.FromBase64String(partes[2]);
        var hashEsperado = Convert.FromBase64String(partes[3]);
        var hashRecibido = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashRecibido, hashEsperado);
    }
}
