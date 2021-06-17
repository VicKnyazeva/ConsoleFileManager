using System.Collections.Generic;

namespace ConsoleFileManager
{
    /// <summary>Базовый класс для реализации команд приложения</summary>
    abstract class Command
    {
        /// <summary>Список с названиями и сокращениями команды, разделёнными запятыми</summary>
        public abstract string Names { get; }
      
        /// <summary>Краткое описание команды</summary>
        public abstract string ShortDescription { get; }
        
        /// <summary>Детализированное описание команды</summary>
        public abstract string DetailedDescription { get; }

        /// <summary>Разбирает элементы командной строки и затем выполняет команду</summary>
        /// <returns>null - если всё прошло успешно, иначе текст с описанием ошибки.</returns>
        public abstract string Execute(List<string> args);
    }
}
