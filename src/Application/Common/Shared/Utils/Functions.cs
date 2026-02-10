using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Common.Shared.Utils;

public partial class Functions(IUserRepository userRepository, ILogger<Functions>logger)
{
    private static readonly Random _rand = new();
    public static string? PhoneLocalFormat(string? phoneNumber, bool? withoutPrefix = false)
    {
        if (phoneNumber == null) return null;
        phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("/", "").Replace(".", "").Replace("\\", "");
        if (phoneNumber.Length < 8) return null;
        phoneNumber = phoneNumber.Length > 8 ? phoneNumber.Substring(phoneNumber.Length - 8, 8) : phoneNumber;
        return (withoutPrefix == true ? "" : "232") + phoneNumber;
    }

        public static string GenerateBarcode(string? brandName, string? categoryName, int sequence)
        {
            string brandCode = !string.IsNullOrWhiteSpace(brandName)
                ? new string(brandName.Where(char.IsLetterOrDigit).ToArray()).ToUpper().Substring(0, Math.Min(3, brandName.Length))
                : "PRD";

            string catCode = !string.IsNullOrWhiteSpace(categoryName)
                ? new string(categoryName.Where(char.IsLetterOrDigit).ToArray()).ToUpper().Substring(0, Math.Min(3, categoryName.Length))
                : "GEN";

            return $"{brandCode}-{catCode}-{sequence.ToString().PadLeft(6, '0')}";
        }
    

    public static string TitleCaseConvert(string title)
    {
        try
        {
            var wordStart = 0;
            var result = new char[title.Length];
            var ri = 0;
            for (var i = 0; i < title.Length; ++i)
            {
                if (title[i] == '_')
                {
                    wordStart = i + 1;
                }
                else if (i == wordStart) result[ri++] = char.ToUpper(title[i]);
                else result[ri++] = char.ToLower(title[i]);
            }
            return new string(result, 0, ri);
        }
        catch (Exception)
        {
            return title;
        }
    }

    public static string TitleCaseRevert(string title)
    {
        try
        {
            var result = new char[10000];
            var ri = 0;
            foreach (var t in title)
            {
                if (char.IsUpper(t) && ri != 0)
                {
                    result[ri] = '_';
                    ri++;
                    result[ri] = char.ToLower(t);
                }
                else result[ri] = char.ToLower(t);
                ri++;
            }
            return new string(result, 0, ri);
        }
        catch (Exception)
        {
            return title;
        }
    }

    public static string GenerateUid(bool? braces = true)
    {
        return Guid.NewGuid().ToString(braces == true ? "D" : "N");
    }
    public static string Generate(int? length = 10)
    {
        if (length != null && length > 20) length = 20;
        const string charset = "09182736455463728190";
        var outputChars = new char[length ?? 10];
        using var rng = RandomNumberGenerator.Create();
        const int minIndex = 0;
        var maxIndexExclusive = charset.Length;
        var diff = maxIndexExclusive - minIndex;
        var upperBound = uint.MaxValue / diff * diff;
        var randomBuffer = new byte[sizeof(int)];

        for (var i = 0; i < outputChars.Length; i++)
        {
            uint randomUInt;
            do
            {
                rng.GetBytes(randomBuffer);
                randomUInt = BitConverter.ToUInt32(randomBuffer, 0);
            }
            while (randomUInt >= upperBound);
            var charIndex = (int)(randomUInt % diff);

            outputChars[i] = charset[charIndex];
        }

        return new string(outputChars);
    }

    public static string GenerateKey(int? length = 10)
    {
        if (length is > 20) length = 20;
        const string charset = "09182736455463728190AZBYCWDVEUFTGSHRIQJPKOLNMMNLOKPJQIRHSGTFUEVDWCXBYAZ";
        var outputChars = new char[length ?? 10];
        using var rng = RandomNumberGenerator.Create();
        const int minIndex = 0;
        var maxIndexExclusive = charset.Length;
        var diff = maxIndexExclusive - minIndex;
        var upperBound = uint.MaxValue / diff * diff;
        var randomBuffer = new byte[sizeof(int)];

        for (var i = 0; i < outputChars.Length; i++)
        {
            uint randomUInt;
            do
            {
                rng.GetBytes(randomBuffer);
                randomUInt = BitConverter.ToUInt32(randomBuffer, 0);
            }
            while (randomUInt >= upperBound);
            var charIndex = (int)(randomUInt % diff);

            outputChars[i] = charset[charIndex];
        }

        return new string(outputChars);
    }

    public Users? GetUser(string? username)
    {
        logger.LogInformation($"GetUser: {username}");
        var users = userRepository.FindByEmailOrPhoneOrUsername(username);
        if (users == null) return null;
        return users.Count is 0 or > 1 ? null : users[0];
    }

    public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
    {
        for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            yield return day;
    }

    public static bool IsBase64(string base64)
    {
        var buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
    }

    public static string ToBase64(string value)
    {
        return IsBase64(value) ? value : Convert.ToBase64String(Encoding.UTF8.GetBytes(value.Trim()));
    }

    public static string? FromBase64(string base64)
    {
        return IsBase64(base64) ? Encoding.UTF8.GetString(Convert.FromBase64String(base64.Trim())) : null;
    }



    public static bool IsEmail(string? email)
    {
        return MyRegex().IsMatch(email ?? "");
    }



    [GeneratedRegex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.IgnoreCase, "en-CI")]
    private static partial Regex MyRegex();

    public static HttpClientHandler ClientHandlerSertificateValidation()
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
    }

    public static string JsonSerialize(dynamic? data)
    {
        return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static dynamic? JsonDeserialize<TM>(dynamic data)
    {
        try
        {
            return JsonSerializer.Deserialize<TM>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
        public static string GenerateSku(string? brandName, string? categoryName)
        {
            string brand = GetPrefix(brandName, "BRD");
            string cat = GetPrefix(categoryName, "CAT");
            string unique = _rand.Next(1000, 9999).ToString();
            return $"{brand}-{cat}-{unique}".ToUpper();
        }

        private static string GetPrefix(string? text, string fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
                return fallback;
            text = Regex.Replace(text.ToUpper(), "[^A-Z]", "");
            return text.Length >= 3 ? text[..3] : text.PadRight(3, 'X');
        }
    
}

