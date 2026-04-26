namespace Manabi.Api.Models;

public class TeacherProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public List<string> Skills { get; set; } = [];
    public int Rate30Min { get; set; }
    public int Rate60Min { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }

    public List<Session> Sessions { get; set; } = [];
}
