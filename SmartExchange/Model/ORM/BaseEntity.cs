using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartExchange.Model.ORM
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }
        DateTime DateCreated { get; set; }
        int Index { get; set; }
    }
    public class BaseEntity : IBaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }

        [NotMapped]

        public DateTime DateCreatedLocal { get => DateCreated.ToLocalTime(); set { } }
        public int Index { get; set; }

    }


    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IBaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            _ = builder.HasKey(e => e.Id);

            _ = builder.Property(er => er.DateCreated)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

            _ = builder.Property(e => e.Index)
                .ValueGeneratedOnAdd();

            _ = builder.HasIndex(c => c.Index)
                .IsUnique();
        }
    }
}
