namespace Medicore.Api.Models;

public class RevokedAccessToken
{
    public int Id { get; set; }
    public string TokenId { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
}
