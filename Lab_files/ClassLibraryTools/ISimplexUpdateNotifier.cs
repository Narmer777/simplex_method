using System;

namespace ClassLibraryTools
{
    /// <summary>
    /// Делегат для обработки событий обновления таблицы.
    /// </summary>
    /// <param name="sender"><see cref="Label"/>, который вызвал событие.</param>
    /// <param name="e">Данные события.</param>
    public delegate void TableUpdatedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Интерфейс для объектов, которые уведомляют об обновлении симплекс-таблицы.
    /// </summary>
    public interface ISimplexTableNotifier
    {
        /// <summary>
        /// Событие, возникающее при обновлении симплекс-таблицы.
        /// </summary>
        event TableUpdatedEventHandler TableUpdated;
    }
}