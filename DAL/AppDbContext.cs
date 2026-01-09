using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Models;
using DAL.Repositories;

namespace DAL;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CV> CVs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<WorkPlace> WorkPlaces { get; set; }
    public DbSet<Education> Educations { get; set; }
    public DbSet<Competence> Competences { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    public DbSet<Institution> Institutions { get; set; }
    public DbSet<WorkExperience> WorkExperiences { get; set; }
    public DbSet<GuestMessage> GuestMessages { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationUser> ConversationUsers { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //Project user
        //make foreignkey combination primary key
        modelBuilder.Entity<ProjectUser>()
            .HasKey(pu => new { pu.ProjectId, pu.UserId });

        modelBuilder.Entity<ProjectUser>()
            .HasOne(pu => pu.Project)
            .WithMany(p => p.ProjectUsers)
            .HasForeignKey(pu => pu.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectUser>()
            .HasOne(pu => pu.User)
            .WithMany(u => u.ProjectUsers)
            .HasForeignKey(pu => pu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // M:N 
        modelBuilder.Entity<ConversationUser>()
            .HasKey(cu => new { cu.ConversationId, cu.UserId });

        modelBuilder.Entity<ConversationUser>()
            .HasOne(cu => cu.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cu => cu.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationUser>()
            .HasOne(cu => cu.User)
            .WithMany(u => u.ConversationUsers)
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:M mellan message och conversations

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // N:1 mellan message och user

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .Property(m => m.Text)
            .IsRequired()
            .HasMaxLength(2000);

        // N:1 mellan guest och user

        modelBuilder.Entity<GuestMessage>()
            .HasOne(g => g.ToUser)
            .WithMany()
            .HasForeignKey(g => g.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GuestMessage>()
            .Property(g => g.GuestName)
            .IsRequired()
            .HasMaxLength(120);

        modelBuilder.Entity<GuestMessage>()
            .Property(g => g.Text)
            .IsRequired()
            .HasMaxLength(2000);


    }
}
