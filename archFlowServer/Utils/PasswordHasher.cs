using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace archFlowServer.Utils;

public class PasswordHasher
{
    private readonly string _pepper;

    public PasswordHasher(IOptions<SecuritySettings> options)
    {
        _pepper = options.Value.Pepper;
    }

    public string HashPassword(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var passwordBytes = Encoding.UTF8.GetBytes(password + _pepper);

        var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            Iterations = 4,
            MemorySize = 65536
        };

        var hashBytes = argon2.GetBytes(32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashBytes)}";
    }

    public bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = parts[1];

        var passwordBytes = Encoding.UTF8.GetBytes(password + _pepper);

        var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            Iterations = 4,
            MemorySize = 65536
        };

        var computedHash = Convert.ToBase64String(argon2.GetBytes(32));

        return computedHash == hash;
    }
}

