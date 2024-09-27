using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GorevTakipProgrami.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<InvitePersonToken> InvitePersonTokens { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<DBTask> Tasks { get; set; }

    public virtual DbSet<TaskHistory> TaskHistories { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA2AB3F807");

            entity.Property(e => e.CommentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Task).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__Comments__TaskId__70A8B9AE");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Comments__UserId__6FB49575");
        });

        modelBuilder.Entity<InvitePersonToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InvitePe__3214EC07ACD9E462");

            entity.HasIndex(e => e.Token, "UQ_InvitePersonTokens_Token").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3214EC072EC683D9");

            entity.HasIndex(e => e.Token, "UQ_PasswordResetTokens_Token").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<DBTask>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__7C6949B10B8A4C10");

            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.TaskName).HasMaxLength(200);
            entity.Property(e => e.TaskStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Baslanmadi");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.AssignedByUser).WithMany(p => p.TaskAssignedByUsers)
                .HasForeignKey(d => d.AssignedByUserId)
                .HasConstraintName("FK__Tasks__AssignedB__6BE40491");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.TaskAssignedToUsers)
                .HasForeignKey(d => d.AssignedToUserId)
                .HasConstraintName("FK__Tasks__AssignedT__6AEFE058");
        });

        modelBuilder.Entity<TaskHistory>(entity =>
        {
            
                entity.ToTable("TaskHistory");

            entity.Property(e => e.ChangeDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewStatus).HasMaxLength(50);

            entity.HasOne(d => d.ChangedByUser).WithMany()
                .HasForeignKey(d => d.ChangedByUserId)
                .HasConstraintName("FK__TaskHisto__Chang__7A3223E8");

            entity.HasOne(d => d.Task).WithMany()
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__TaskHisto__TaskI__793DFFAF");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C870D40ED");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.ProfilePhoto)
                .HasMaxLength(250)
                .HasDefaultValue("default_profile_photo.png");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("Kullanici");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
