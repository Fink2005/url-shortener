namespace UrlService.Domain.ValueObjects;

public record ShortCode
{
    public string Value { get; }

    public ShortCode(string value = "")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = Generate();
        }
        else
        {
            Value = value;
        }
    }

    private static string Generate()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "")
            .Substring(0, 8);
    }

    public override string ToString() => Value;
}
