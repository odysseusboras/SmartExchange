using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartExchange.Model.ORM
{
    [Table("Pair")]
    public class Pair : BaseEntity
    {
        public string? Name { get; set; }
        public decimal StepSize { get; set; } = 0;
    }
    public class PairComparer : IEqualityComparer<Pair>
    {
        public bool Equals(Pair x, Pair y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Pair obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    internal class PairConfiguration : BaseEntityConfiguration<Pair>
    {
        public override void Configure(EntityTypeBuilder<Pair> builder)
        {
            _ = builder.HasIndex(c => c.Name);

            _ = builder.Property(e => e.StepSize)
                .HasPrecision(20, 8);

            base.Configure(builder);

        }
    }
}
