namespace WorkBank.Domain.Models
{
    /// <summary>
    /// Сущность пользователя
    /// </summary>
    public class Person : BaseEntity
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateOnly Birthdate { get; set; }

        /// <summary>
        /// Признак блокировки пользователя
        /// </summary>
        public bool IsBlocked {  get; set; } = false;

        List<Credit> Credits { get; set; } = new List<Credit>();

        public Passport Passport { get; set; }
    }
}
