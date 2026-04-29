using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;

namespace Manabi.Api.Services;

public class TeacherService(AppDbContext db)
{
    public async Task<List<TeacherProfileResponse>> SearchAsync(
        string? skill, int? maxRate, string sortBy = "rating")
    {
        var query = db.TeacherProfiles
            .Include(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(skill))
            query = query.Where(t => t.Skills.Any(s =>
                s.ToLower().Contains(skill.ToLower())));

        if (maxRate.HasValue)
            query = query.Where(t => t.Rate30Min <= maxRate.Value);

        query = sortBy switch
        {
            "price" => query.OrderBy(t => t.Rate30Min),
            _ => query.OrderByDescending(t => t.AverageRating)
        };

        return await query.Select(t => ToResponse(t)).ToListAsync();
    }

    public async Task<TeacherProfileResponse?> GetByUserIdAsync(string userId)
    {
        var profile = await db.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId);

        return profile is null ? null : ToResponse(profile);
    }

    public async Task<(TeacherProfileResponse? result, string? error)> CreateAsync(
        string userId, CreateTeacherProfileRequest req)
    {
        if (await db.TeacherProfiles.AnyAsync(t => t.UserId == userId))
            return (null, "先生プロフィールはすでに存在します。");

        var profile = new TeacherProfile
        {
            UserId = userId,
            Skills = req.Skills,
            Categories = req.Categories,
            Rate30Min = req.Rate30Min,
            Rate60Min = req.Rate60Min
        };

        db.TeacherProfiles.Add(profile);

        if (!string.IsNullOrWhiteSpace(req.Bio))
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is not null) user.Bio = req.Bio;
        }

        await db.SaveChangesAsync();

        await db.Entry(profile).Reference(t => t.User).LoadAsync();
        return (ToResponse(profile), null);
    }

    public async Task<(TeacherProfileResponse? result, string? error)> UpdateAsync(
        string userId, UpdateTeacherProfileRequest req)
    {
        var profile = await db.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (profile is null)
            return (null, "先生プロフィールが見つかりません。");

        if (req.Skills is not null) profile.Skills = req.Skills;
        if (req.Categories is not null) profile.Categories = req.Categories;
        if (req.Rate30Min.HasValue) profile.Rate30Min = req.Rate30Min.Value;
        if (req.Rate60Min.HasValue) profile.Rate60Min = req.Rate60Min.Value;
        if (req.Bio is not null) profile.User.Bio = req.Bio;

        await db.SaveChangesAsync();
        return (ToResponse(profile), null);
    }

    private static TeacherProfileResponse ToResponse(TeacherProfile t) => new()
    {
        UserId = t.UserId,
        DisplayName = t.User.DisplayName,
        Bio = t.User.Bio,
        AvatarUrl = t.User.AvatarUrl,
        Skills = t.Skills,
        Categories = t.Categories,
        Rate30Min = t.Rate30Min,
        Rate60Min = t.Rate60Min,
        AverageRating = t.AverageRating,
        TotalReviews = t.TotalReviews
    };
}
