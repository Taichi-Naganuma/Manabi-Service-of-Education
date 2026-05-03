using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;
using Manabi.Shared.Models;

namespace Manabi.Api.Services;

public class AccountDeletionService(AppDbContext db, UserManager<AppUser> userManager)
{
    public const string DeletedDisplayName = "退会したユーザー";

    public async Task<bool> DeleteAsync(string userId, AccountDeletionReason reason)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted) return false;

        var hadTeacher = await db.TeacherProfiles.AnyAsync(t => t.UserId == userId);
        var hadSent = await db.WitnessLetters.AnyAsync(l => l.FromUserId == userId);
        var hadReceived = await db.WitnessLetters.AnyAsync(l => l.ToUserId == userId);
        var accountAgeDays = (int)Math.Floor((DateTime.UtcNow - user.CreatedAt).TotalDays);

        await using var tx = await db.Database.BeginTransactionAsync();

        await db.TeacherProfiles.Where(t => t.UserId == userId).ExecuteDeleteAsync();
        await db.StorySnippets.Where(s => s.AuthorUserId == userId).ExecuteDeleteAsync();
        await db.WitnessLetters.Where(l => l.ToUserId == userId).ExecuteDeleteAsync();
        // DirectMessage は保持（C案: 双方保持・送信者匿名化）
        // AppUser の IsDeleted=true と DisplayName="退会したユーザー" で表示側が匿名化を担う
        await db.Reviews.Where(r => r.ReviewerId == userId).ExecuteDeleteAsync();
        await db.SelfReflections.Where(r => r.UserId == userId).ExecuteDeleteAsync();
        await db.UserBlocks.Where(b => b.BlockerUserId == userId || b.BlockedUserId == userId).ExecuteDeleteAsync();
        await db.EmailNotificationPreferences.Where(e => e.UserId == userId).ExecuteDeleteAsync();

        // AppUser は匿名化して残す（Letter 送信履歴・Session 統計を保持するため）
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DisplayName = DeletedDisplayName;
        user.Bio = string.Empty;
        user.AvatarUrl = null;
        user.Email = null;
        user.NormalizedEmail = null;
        user.UserName = $"deleted_{userId}";
        user.NormalizedUserName = $"DELETED_{userId.ToUpperInvariant()}";
        user.PasswordHash = null;
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.Role = UserRole.Student;
        user.EmailConfirmed = false;
        user.PhoneNumber = null;
        user.PhoneNumberConfirmed = false;
        user.TwoFactorEnabled = false;
        user.LockoutEnd = null;
        user.LockoutEnabled = false;
        user.AccessFailedCount = 0;

        await db.SaveChangesAsync();

        db.AccountDeletionLogs.Add(new AccountDeletionLog
        {
            Reason = reason,
            DeletedAt = DateTime.UtcNow,
            AccountAgeDays = accountAgeDays,
            HadTeacherProfile = hadTeacher,
            HadSentLetters = hadSent,
            HadReceivedLetters = hadReceived
        });
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return true;
    }
}
