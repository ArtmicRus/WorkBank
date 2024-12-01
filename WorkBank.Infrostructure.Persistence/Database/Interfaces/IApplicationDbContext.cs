using Microsoft.EntityFrameworkCore;
using WorkBank.Domain.Models;

namespace WorkBank.Infrostructure.Persistence.Database.Interfaces
{
    public interface IApplicationDbContext
    {
        /// <summary>
        /// Паспорта
        /// </summary>
        DbSet<Credit> Credits { get; }

        /// <summary>
        /// Паспорта
        /// </summary>
        DbSet<Passport> Passports { get; }

        /// <summary>
        /// Люди
        /// </summary>
        DbSet<Person> Persons { get; }

        /// <summary>
        /// Сохранение изменений
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
