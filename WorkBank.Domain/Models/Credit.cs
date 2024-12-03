namespace WorkBank.Domain.Models
{
    /// <summary>
    /// Сущность кредита
    /// </summary>
    public class Credit : BaseEntity
    {
        /// <summary>
        /// Сумма
        /// </summary>
        public decimal Summa { get; set; }

        /// <summary>
        /// Периол займа
        /// </summary>
        public short Period { get; set; }

        /// <summary>
        /// Занявший пользователь
        /// </summary>
        public long PersonId { get; set; }
        public Person Person { get; set; }
    }
}
