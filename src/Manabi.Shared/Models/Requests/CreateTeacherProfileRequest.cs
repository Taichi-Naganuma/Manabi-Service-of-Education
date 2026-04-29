using System.ComponentModel.DataAnnotations;

namespace Manabi.Shared.Models.Requests;

public class CreateTeacherProfileRequest
{
    [Required, MinLength(1)]
    public List<string> Skills { get; set; } = [];

    public List<string> Categories { get; set; } = [];

    public string Bio { get; set; } = string.Empty;

    [Range(0, 100000)]
    public int? Rate30Min { get; set; }

    [Range(0, 100000)]
    public int? Rate60Min { get; set; }
}
