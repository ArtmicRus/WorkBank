namespace WorkBank.Domain.Dtos
{
    /// <summary>
    /// Dto паспорта
    /// </summary>
    /// <remarks>
    /// Перенёс Dto в Domain потому что если один web-api имеет ссылку на другой, его контроллеры соединяются в 1 сваггер интерфейс
    /// </remarks>
    public class PassportDto
    {
        /// <summary>
        /// Серия паспорта
        /// </summary>
        public string Serie { get; set; } = string.Empty;

        /// <summary>
        /// Номер паспорта
        /// </summary>
        public string Number { get; set; } = string.Empty;
    }
}
