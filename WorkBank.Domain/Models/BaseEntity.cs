namespace WorkBank.Domain.Models
{
    /// <summary>
    /// Базовая сущность для всех основных моделей
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        public long Id { get; set; }
    }
}
