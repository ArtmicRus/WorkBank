using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkBank.Domain.Models;

namespace WorkBank.Infrostructure.Persistence.Database.Configurations
{
    public class CreditEntityTypeConfiguration : IEntityTypeConfiguration<Credit>
    {
        public void Configure(EntityTypeBuilder<Credit> builder)
        {
            builder
                .Property(c => c.Summa)
                .IsRequired();

            builder
                .Property(c => c.Period)
                .IsRequired();

            builder
                .HasOne(c => c.Person)
                .WithMany()
                .HasForeignKey(c => c.PersonId); // Указываем внешний ключ
        }
    }
}
