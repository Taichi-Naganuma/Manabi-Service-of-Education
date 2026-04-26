namespace Manabi.Shared.Models.Responses;

public class TeacherProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public List<string> Skills { get; set; } = [];
    public int Rate30Min { get; set; }
    public int Rate60Min { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
