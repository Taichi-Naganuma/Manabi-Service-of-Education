namespace Manabi.Shared.Models.Requests;

public class UpdateTeacherProfileRequest
{
    public List<string>? Skills { get; set; }
    public List<string>? Categories { get; set; }
    public string? Bio { get; set; }
    public int? Rate30Min { get; set; }
    public int? Rate60Min { get; set; }
}
