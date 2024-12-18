﻿using Microsoft.EntityFrameworkCore;
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

            builder
                .HasOne(p => p.Passport)
                .WithOne(pp => pp.Person)
                .HasForeignKey<Passport>(pp => pp.PersonId); // Указываем внешний ключ

            builder
                .Property(p => p.IsBlocked)
                .HasColumnName("isBlocked");

            //builder
            //    .HasOne(p => p.Passport)
            //    .WithOne()
            //    .HasForeignKey("PassportId");
        }
    }
}
