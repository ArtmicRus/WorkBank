namespace WorkBank.Domain.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Перенёс Dto в Domain потому что если один web-api имеет ссылку на другой, его контроллеры соединяются в 1 сваггер интерфейс
    /// </remarks>
    public class PassportDto
    {
        public string Serie { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;
    }
}
