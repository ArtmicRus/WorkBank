namespace WorkBank.Domain.Models
{
    /// <summary>
    /// Сущность пасспорта
    /// </summary>
    public class Passport : BaseEntity
    {
        /// <summary>
        /// Серия
        /// </summary>
        public string Serie { get; set; } = string.Empty;

        /// <summary>
        /// Номер
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Владелец
        /// </summary>
        public long PersonId { get; set; }
        public Person Person { get; set; }
    }
}
