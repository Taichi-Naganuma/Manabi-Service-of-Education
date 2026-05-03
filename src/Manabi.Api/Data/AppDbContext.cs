using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Models;

namespace Manabi.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WitnessLetter> WitnessLetters => Set<WitnessLetter>();
    public DbSet<StorySnippet> StorySnippets => Set<StorySnippet>();
    public DbSet<EmailNotificationPreference> EmailNotificationPreferences => Set<EmailNotificationPreference>();
    public DbSet<SelfReflection> SelfReflections => Set<SelfReflection>();
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<AccountDeletionLog> AccountDeletionLogs => Set<AccountDeletionLog>();
    public DbSet<UserActionLog> UserActionLogs => Set<UserActionLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TeacherProfile>()
            .Property(t => t.Skills)
            .HasColumnType("text[]");

        builder.Entity<Session>()
            .HasOne(s => s.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Session>()
            .HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DirectMessage>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DirectMessage>()
            .HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DirectMessage>()
            .HasIndex(m => new { m.SenderId, m.RecipientId });

        builder.Entity<Message>()
            .HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.Session)
            .WithOne(s => s.Review)
            .HasForeignKey<Review>(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WitnessLetter>()
            .HasOne(l => l.FromUser)
            .WithMany()
            .HasForeignKey(l => l.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WitnessLetter>()
            .HasOne(l => l.ToUser)
            .WithMany()
            .HasForeignKey(l => l.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WitnessLetter>()
            .HasIndex(l => l.ToUserId);

        builder.Entity<StorySnippet>()
            .HasOne(s => s.Author)
            .WithMany()
            .HasForeignKey(s => s.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StorySnippet>()
            .HasIndex(s => new { s.IsPublished, s.Category });

        builder.Entity<EmailNotificationPreference>()
            .HasKey(e => e.UserId);

        builder.Entity<EmailNotificationPreference>()
            .HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<EmailNotificationPreference>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SelfReflection>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SelfReflection>()
            .HasOne(r => r.PartnerUser)
            .WithMany()
            .HasForeignKey(r => r.PartnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserBlock>()
            .HasOne(b => b.Blocker)
            .WithMany()
            .HasForeignKey(b => b.BlockerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserBlock>()
            .HasOne(b => b.Blocked)
            .WithMany()
            .HasForeignKey(b => b.BlockedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserBlock>()
            .HasIndex(b => new { b.BlockerUserId, b.BlockedUserId })
            .IsUnique();

        builder.Entity<Report>()
            .HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserActionLog>()
            .HasIndex(l => new { l.UserId, l.EventName, l.CreatedAt });
    }
}
