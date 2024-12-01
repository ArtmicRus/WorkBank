namespace WorkBank.Domain.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Перенёс Dto в Domain потому что если один web-api имеет ссылку на другой, его контроллеры соединяются в 1 сваггер интерфейс
    /// </remarks>
    public class RequestDto
    {
        public decimal Summa { get; set; }

        public short Period { get; set; }
    }
}
