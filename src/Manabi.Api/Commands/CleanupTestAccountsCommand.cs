using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;

namespace Manabi.Api.Commands;

public static class CleanupTestAccountsCommand
{
    public static async Task<int> RunAsync(string[] args, IServiceProvider services)
    {
        var pattern = ParseEmailPattern(args);
        if (pattern is null)
        {
            Console.WriteLine("Usage: dotnet run --project src/Manabi.Api -- cleanup-test-accounts --email-pattern <pattern>");
            Console.WriteLine("  <pattern> はワイルドカード * を含む文字列。例: \"test*\", \"*@example.com\"");
            return 1;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var likePattern = pattern.Replace('*', '%');
        var targets = await db.Users
            .Where(u => u.Email != null && EF.Functions.ILike(u.Email, likePattern) && !u.IsDeleted)
            .Select(u => new { u.Id, u.Email, u.DisplayName, u.CreatedAt })
            .ToListAsync();

        if (targets.Count == 0)
        {
            Console.WriteLine($"パターン \"{pattern}\" に一致するアカウントは見つかりませんでした。");
            return 0;
        }

        Console.WriteLine($"パターン \"{pattern}\" に一致するアカウント: {targets.Count} 件");
        Console.WriteLine(new string('-', 80));
        foreach (var t in targets)
            Console.WriteLine($"  {t.Id}  {t.Email,-40}  {t.DisplayName}  ({t.CreatedAt:yyyy-MM-dd})");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine();
        Console.WriteLine("⚠️  これらのアカウントと関連データ (Letter / Story / Chat / Session 等) を完全削除します。");
        Console.WriteLine("    この操作は取り消せません。");
        Console.Write("実行しますか？ [yes/no]: ");

        var input = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (input != "yes")
        {
            Console.WriteLine("キャンセルしました。");
            return 0;
        }

        var deleted = 0;
        foreach (var t in targets)
        {
            await HardDeleteAsync(db, t.Id);
            var user = await userManager.FindByIdAsync(t.Id);
            if (user is not null)
            {
                await userManager.DeleteAsync(user);
                deleted++;
                Console.WriteLine($"  ✅ 削除: {t.Email}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"完了: {deleted}/{targets.Count} 件のアカウントを削除しました。");
        return 0;
    }

    private static string? ParseEmailPattern(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--email-pattern")
                return args[i + 1];
        }
        return null;
    }

    private static async Task HardDeleteAsync(AppDbContext db, string userId)
    {
        await db.TeacherProfiles.Where(t => t.UserId == userId).ExecuteDeleteAsync();
        await db.StorySnippets.Where(s => s.AuthorUserId == userId).ExecuteDeleteAsync();
        await db.WitnessLetters.Where(l => l.FromUserId == userId || l.ToUserId == userId).ExecuteDeleteAsync();
        await db.DirectMessages.Where(m => m.SenderId == userId || m.RecipientId == userId).ExecuteDeleteAsync();
        await db.Reviews.Where(r => r.ReviewerId == userId).ExecuteDeleteAsync();
        await db.SelfReflections.Where(r => r.UserId == userId || r.PartnerUserId == userId).ExecuteDeleteAsync();
        await db.UserBlocks.Where(b => b.BlockerUserId == userId || b.BlockedUserId == userId).ExecuteDeleteAsync();
        await db.Reports.Where(r => r.ReporterUserId == userId).ExecuteDeleteAsync();
        await db.EmailNotificationPreferences.Where(e => e.UserId == userId).ExecuteDeleteAsync();
        await db.Sessions.Where(s => s.TeacherId == userId || s.StudentId == userId).ExecuteDeleteAsync();
    }
}
