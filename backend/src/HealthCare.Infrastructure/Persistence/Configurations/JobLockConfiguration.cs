using HealthCare.Infrastructure.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class JobLockConfiguration : IEntityTypeConfiguration<JobLock>
{
    public void Configure(EntityTypeBuilder<JobLock> builder)
    {
        builder.ToTable("BackgroundJobLocks");
        builder.HasKey(x => x.JobName);

        builder.Property(x => x.JobName).HasMaxLength(100);
        builder.Property(x => x.LockedBy).HasMaxLength(100);
    }
}
