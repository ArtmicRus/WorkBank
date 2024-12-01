using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkBank.Domain.Models;

namespace WorkBank.Database.Configurations
{
    public class PersonEntityTypeConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder
                .Property(pers => pers.FirstName)
                .IsRequired();

            builder
                .Property(pers => pers.LastName);
        }
    }
}
