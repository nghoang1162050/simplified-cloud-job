using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Entities;

public class JobConfiguration : IEntityTypeConfiguration<JobEntity>
{
    public void Configure(EntityTypeBuilder<JobEntity> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.JobId);

        builder.Property(j => j.JobId).HasColumnName("JobId").HasColumnType("varchar(50)");
        builder.Property(j => j.JobName).HasColumnName("JobName").HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(j => j.ProjectId).HasColumnName("ProjectId").HasColumnType("varchar(100)").IsRequired();

        builder.Property(j => j.ComputeType)
            .HasColumnName("ComputeType")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(j => j.Status).HasColumnName("Status").HasColumnType("varchar(50)").IsRequired();
        builder.Property(j => j.InputFileName)
            .HasColumnName("InputFileName")
            .HasColumnType("nvarchar(255)")
            .IsRequired()
            .HasDefaultValue("");
        builder.Property(j => j.OutputFileReference).HasColumnName("OutputFileReference").HasColumnType("nvarchar(1000)");
        builder.Property(j => j.ExecutionDuration).HasColumnName("ExecutionDuration");
        builder.Property(j => j.CreditCost).HasColumnName("CreditCost");
        builder.Property(j => j.CreatedAt).HasColumnName("CreatedAt").IsRequired();
        builder.Property(j => j.RequestHash)
            .HasColumnName("RequestHash")
            .HasColumnType("char(64)")
            .IsRequired();

        builder.HasIndex(j => j.ProjectId).HasDatabaseName("IX_Jobs_ProjectId");
        builder.HasIndex(j => j.Status).HasDatabaseName("IX_Jobs_Status");
        builder.HasIndex(j => j.RequestHash)
            .IsUnique()
            .HasDatabaseName("IX_Jobs_RequestHash_Unique");
    }
}
