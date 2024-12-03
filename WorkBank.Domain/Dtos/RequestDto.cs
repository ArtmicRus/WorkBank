namespace WorkBank.Domain.Dtos
{
    /// <summary>
    /// Dto запроса на кредит
    /// </summary>
    /// <remarks>
    /// Перенёс Dto в Domain потому что если один web-api имеет ссылку на другой, его контроллеры соединяются в 1 сваггер интерфейс
    /// </remarks>
    public class RequestDto
    {
        /// <summary>
        /// Запрашиваемая сумма
        /// </summary>
        public decimal Summa { get; set; }

        /// <summary>
        /// Период займа
        /// </summary>
        public short Period { get; set; }
    }
}
