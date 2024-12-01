using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkBank.Domain.Models;

namespace WorkBank.Database.Configurations
{
    public class PassportEntityTypeConfiguration : IEntityTypeConfiguration<Passport>
    {
        public void Configure(EntityTypeBuilder<Passport> builder)
        {
            builder
                .Property(pass => pass.Number)
                .IsRequired();

            builder
                .Property(pass => pass.Sirie)
                .IsRequired();
        }
    }
}
