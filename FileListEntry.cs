using System;

namespace ConsoleFileManager
{
    /// <summary> Разновидность элементов списка</summary>
    enum FileListEntryKind
    {
        /// <summary>Диск или устройство хранения</summary>
        Drive,
        /// <summary>Директория</summary>
        Directory,
        /// <summary>Файл</summary>
        File,
    }


    /// <summary>
    /// Перечисление для описания положения элемента в списке (например, файла в списке поддиректорий
    /// и файлов родитетельской директории)
    /// </summary>
    [Flags]
    enum FileListEntryMode
    {
        /// <summary>Внутренний элемент подсписка (например, не первый и не последний файл в директории)</summary>
        Middle = 0,

        /// <summary>Первый элемент подсписка (например, первый файл в директории)</summary>
        First = 1,

        /// <summary>Последний элемент подсписка (например, последний файл в директории)</summary>
        Last = 2,

        /// <summary>Единственный элемент в подсписке, т.е. оджновренменно и первый и последний элемент </summary>
        Only = First | Last
    }

    /// <summary>Описывает элемент списка отображаемых объектов файловой системы (например, диск, директория или файл)</summary>
    class FileListEntry
    {
        /// <summary>Глубина данного элемента списка в файловом дереве</summary>
        public int depth { get; set; }

        /// <summary> Разновидность данного элемента списка</summary>
        public FileListEntryKind Kind { get; set; }

        /// <summary>Положение данного элемента в списке родительской директории</summary>
        public FileListEntryMode Mode { get; set; }

        /// <summary>Название данного элемента списка</summary>
        public string Name { get; set; }

        /// <summary>Полный путь данного элемента списка в файловой системе</summary>
        public string FullPath { get; internal set; }
    }
}
