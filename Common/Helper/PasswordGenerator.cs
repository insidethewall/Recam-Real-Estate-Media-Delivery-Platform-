using System.Security.Cryptography;

namespace RecamSystemApi.Helper;

public static class PasswordGenerator
{
    /// <summary>
    /// Generates a strong password with at least one uppercase letter, one lowercase letter, one digit, and one special character.
    /// </summary>
    /// <param name="length">The desired length of the password. Minimum is 6 characters.</param>
    /// <returns>A randomly generated strong password.</returns>
    /// <exception cref="ArgumentException">Thrown if the length is less than 6.</exception>
    public static string GenerateStrongPassword(int length = 12)
    {
        if (length < 6) throw new ArgumentException("Password length must be at least 6");

        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specials = "!@#$%^&*()-_=+[]{}<>?";

        // Ensure at least one of each category
        var randomChars = new[]
        {
        upper[RandomNumberGenerator.GetInt32(upper.Length)],
        lower[RandomNumberGenerator.GetInt32(lower.Length)],
        digits[RandomNumberGenerator.GetInt32(digits.Length)],
        specials[RandomNumberGenerator.GetInt32(specials.Length)],
    };

        // Fill the rest with random mix
        var allChars = upper + lower + digits + specials;
        var remaining = length - randomChars.Length;
        var password = new char[length];
        randomChars.CopyTo(password, 0);

        for (int i = randomChars.Length; i < length; i++)
        {
            password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
        }

        // Shuffle to avoid predictable placement
        return Shuffle(password);
    }

    private static string Shuffle(char[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return new string(array);
    }
}