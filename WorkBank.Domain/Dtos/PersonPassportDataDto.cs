namespace WorkBank.Domain.Dtos
{
    /// <summary>
    /// Dto пользователя для возврата в проекте PassportData
    /// </summary>
    /// <remarks>
    /// Перенёс Dto в Domain потому что если один web-api имеет ссылку на другой, его контроллеры соединяются в 1 сваггер интерфейс
    /// </remarks>
    public class PersonPassportDataDto
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; set; }

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
    }
}
