using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Contexts
{
    public class SupportChatbotContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SupportChatbotContext(DbContextOptions<SupportChatbotContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Agent)
                .WithMany()
                .HasForeignKey(cs => cs.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditLog = new AuditLog
                {
                    TableName = entry.Metadata.GetTableName() ?? string.Empty,
                    Action = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    UserId = GetCurrentUserId()
                };

                if (entry.State == EntityState.Added)
                {
                    auditLog.NewValue = SerializeValues(entry.CurrentValues);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditLog.OldValue = SerializeValues(entry.OriginalValues);
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditLog.OldValue = SerializeValues(entry.GetDatabaseValues());
                    auditLog.NewValue = SerializeValues(entry.CurrentValues);
                }

                auditEntries.Add(auditLog);
            }

            if (auditEntries.Any())
            {
                AuditLogs.AddRange(auditEntries);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private string SerializeValues(PropertyValues? values)
        {
            if (values == null) return string.Empty;

            var dict = values.Properties.ToDictionary(
                p => p.Name,
                p => values[p]?.ToString()
            );

            return System.Text.Json.JsonSerializer.Serialize(dict);
        }
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "System";
        }
    }
}