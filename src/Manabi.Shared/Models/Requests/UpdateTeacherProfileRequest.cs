namespace Manabi.Shared.Models.Requests;

public class UpdateTeacherProfileRequest
{
    public List<string>? Skills { get; set; }
    public int? Rate30Min { get; set; }
    public int? Rate60Min { get; set; }
}
