namespace Discussly.Core.DTOs
{
    public record BanRequest(
        string Reason,
        int? DurationMinutes = null);
}
