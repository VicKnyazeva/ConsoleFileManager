using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleFileManager
{
    /// <summary>Данный класс отвечает за отображение страницы списка (дерева) объектов файловой системы</summary>
    class FileList
    {
        /// <summary>Текущий размер страницы (количество объектов файловой системы, отображаемых на странице)</summary>
        public static int PageSize = 22;

        /// <summary>Номер текущий отображаемой страницы списка объектов файловой системы</summary>

        public static int PageNumber = 1;

        /// <summary>Список описателей объектов файловой системы в текущей директории</summary>
        private static List<FileListEntry> list = new List<FileListEntry>();

        internal static int FindByFullPath(string fullPath)
        {
            int result = -1;
            int index = 0;
            foreach (var e in list)
            {
                if (string.Compare(fullPath, e.FullPath, true) == 0)
                {
                    result = index;
                    break;
                }
                ++index;
            }
            return result + 1;
        }

        /// <summary>Полное количество элементов в текущем списке описателей объектов файловой системы</summary>
        public static int TotalCount
        {
            get { return list.Count; }
        }

        /// <summary>Полное количество страниц заданного размера в текущем списке описателей объектов файловой системы</summary>
        public static int PageCount
        {
            get { return (TotalCount + (PageSize - 1)) / PageSize; }
        }

        /// <summary>Инициализирует список файлов по заданным настройкам приложения</summary>
        public static void Init(ApplicationSettings settings)
        {
            if (settings.PageSize.HasValue && settings.PageSize.Value > 0)
            {
                PageSize = settings.PageSize.Value;
            }
            if (settings.PageNumber.HasValue && settings.PageNumber.Value > 0)
            {
                PageNumber = settings.PageNumber.Value;
            }

            if (!string.IsNullOrWhiteSpace(settings.Folder))
            {
                string rootPath = settings.Folder.Trim();
                if (Directory.Exists(rootPath))
                {
                    _CurrentDirectory = rootPath;
                    Directory.SetCurrentDirectory(rootPath);
                }
            }

            Reload();

            TryRestoreSelection(settings.Selected);
        }

        private static void TryRestoreSelection(string selected, bool jumpToSelection = true)
        {
            if (!string.IsNullOrWhiteSpace(selected))
            {
                string selPath = selected.Trim();
                _SelectedIndex = -1;
                int index = 0;
                foreach (var e in list)
                {
                    if (string.Compare(selPath, e.FullPath, true) == 0)
                    {
                        _SelectedIndex = index;
                        break;
                    }
                    ++index;
                }
            }
            if (jumpToSelection && _SelectedIndex >= 0)
            {
                // jump to page with selected item
                PageNumber = (_SelectedIndex / PageSize) + 1;
            }
        }

        #region current directory

        static string _CurrentDirectory = null;

        /// <summary>Текущая директория, для которой построен список</summary>
        public static string CurrentDirectory
        {
            get { return _CurrentDirectory; }
        }

        /// <summary>Устанавливает текущую директорию и строит список для неё.</summary>
        /// <param name="path">Путь к директории, или пустая или null - для оборажения активных устройств хранения.</param>
        /// <returns>null - если всё прошло успешно, иначе текст с описанием ошибки.</returns>
        public static string SetCurrentDirectory(string path)
        {
            // запоминаем полный путь текущего выдления
            string selected = SelectedPath;
            // новая директория другая?
            bool changed = String.Compare(_CurrentDirectory, path, true) != 0;
            try
            {
                List<FileListEntry> newList = new List<FileListEntry>();

                // наполняем новый список для указанной директории
                ScanDir(newList, 0, path);

                // запоминаем путь указанной директории
                _CurrentDirectory = path;

                Directory.SetCurrentDirectory(path);

                // запоминаем новый список
                list = newList;

                // попытаемся восстановить выделение по полному пути передыдущего
                TryRestoreSelection(selected, false);

                if (changed) // текущая директория изменилась
                {
                    // отображаем первую страницу нового списка
                    PageNumber = 1;
                }
                else // текущая директория не изменилась
                {
                    if (PageNumber > PageCount)
                    {
                        // новое количество страниц меньше предыдущего
                        // и текущая страница за его пределами

                        // отображаем последнию страницу из доступных
                        PageNumber = PageCount;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"ERROR OCCURED: {ex.Message}";
            }
            return null;
        }

        #endregion current directory

        /// <summary>Заново строит список для текущей директории.</summary>
        /// <returns>null - если всё прошло успешно, иначе текст с описанием ошибки.</returns>
        public static string Reload()
        {
            try
            {
                List<FileListEntry> newList = new List<FileListEntry>();

                // наполняем новый список для текущей директории
                ScanDir(newList, 0, _CurrentDirectory);

                // запоминаем новый список
                list = newList;

                if (_SelectedIndex >= list.Count)
                {
                    // новый список меньше предыдущего
                    // и текущее выделение за его пределами

                    // ставим выделение на последний элемент нового списка
                    _SelectedIndex = list.Count - 1;
                }

                if (PageNumber > PageCount)
                {
                    // новое количество страниц меньше предыдущего
                    // и текущая страница за его пределами

                    // отображаем последнию страницу из доступных
                    PageNumber = PageCount;
                }
            }
            catch (Exception ex)
            {
                return $"ERROR OCCURED: {ex.Message}";
            }
            return null;
        }

        #region selection

        private static int _SelectedIndex = -1;

        /// <summary>Полный путь выделенного элемента списка, или null если выделение отсутвует.</summary>
        public static string SelectedPath
        {
            get
            {
                if (0 <= _SelectedIndex && _SelectedIndex < list.Count)
                    return list[_SelectedIndex].FullPath;
                else
                    return null;
            }
        }

        /// <summary>Номер выделенного элемента списка, или 0 если выделение отсутвует.</summary>
        public static int Selection
        {
            get
            {
                if (0 <= _SelectedIndex && _SelectedIndex < list.Count)
                    return _SelectedIndex + 1;
                else
                    return 0;
            }
        }

        /// <summary>Устанавливает номер выделенного элемента списка.</summary>
        /// <param name="selectedItemNumber">номер элемента списка, или 0 если выделение отсутвует.</param>
        public static string SetSelection(int selectedItemNumber)
        {
            if (selectedItemNumber == 0)
            {
                // убираем выделение, но остаёмся на текущей странице
                _SelectedIndex = -1;
                return null;
            }
            int index = selectedItemNumber - 1;
            if ((0 <= index && index < list.Count))
            {
                // изменяем выделение
                _SelectedIndex = index;

                // переходим на страницу, содержащую выделенение
                PageNumber = (_SelectedIndex / PageSize) + 1;

                return null;
            }
            return $"Selection must be in range [{1}..{list.Count}]";
        }

        #endregion selection

        /// <summary>Отображает информацию о выделенном элементе списка</summary>
        /// <param name="fullWidth">Ширина области отображения</param>
        public static void DrawInfo(int fullWidth)
        {
            FileListEntry selected = 0 <= _SelectedIndex && _SelectedIndex < list.Count ? list[_SelectedIndex] : null;

            Utils.DrawLine(fullWidth, '╞', '═', '╡');
            Console.WriteLine();
            if (selected == null)
            {
                Utils.DrawLine(fullWidth, string.Empty);
                Console.WriteLine();
                Utils.DrawLine(fullWidth, "- no selection -");
                Console.WriteLine();
                Utils.DrawLine(fullWidth, string.Empty);
                Console.WriteLine();
            }
            else
            {
                int selectionPageNumber = (_SelectedIndex / PageSize) + 1;
                if (selected.Kind == FileListEntryKind.Drive)
                {
                    var drv = DriveInfo.GetDrives().FirstOrDefault(i => i.Name == selected.FullPath);
                    Utils.DrawLine(fullWidth, string.Format("Drive: {0} (see page {1})", drv.Name, selectionPageNumber));
                    Console.WriteLine();
                    Utils.DrawLine(fullWidth,
                        string.Format("  Volume Label: {0,-20}     Size: {1:N0} bytes", drv.VolumeLabel, drv.TotalSize));
                    Console.WriteLine();
                    Utils.DrawLine(fullWidth,
                        string.Format("  Available Free Space: {0:N0} bytes", drv.AvailableFreeSpace));
                    Console.WriteLine();
                }
                else if (selected.Kind == FileListEntryKind.Directory)
                {
                    var dir = new DirectoryInfo(selected.FullPath);
                    Utils.DrawLine(fullWidth, string.Format("Directory: {0} (see page {1})", dir.FullName, selectionPageNumber));
                    Console.WriteLine();
                    Utils.DrawLine(fullWidth,
                        string.Format("  Created : {0,-20}", dir.CreationTime));
                    Console.WriteLine();
                    Utils.DrawLine(fullWidth,
                        string.Format("  Modified: {0,-20} Accessed: {1, -20}", dir.LastWriteTime, dir.LastAccessTime));
                    Console.WriteLine();
                }
                else
                {
                    var file = new FileInfo(selected.FullPath);
                    Utils.DrawLine(fullWidth, string.Format("File: {0} (see page {1})", file.FullName, selectionPageNumber));
                    Console.WriteLine();
                    Utils.DrawLine(fullWidth,
                        string.Format("  Created : {0,-20}     Size: {1:N0} bytes", file.CreationTime, file.Length));
                    Console.WriteLine();
                    Utils.DrawLine(fullWidth,
                        string.Format("  Modified: {0,-20} Accessed: {1, -20}", file.LastWriteTime, file.LastAccessTime));
                    Console.WriteLine();
                }
            }
            Utils.DrawLine(fullWidth, Utils.ch5, Utils.ch6);
            Console.WriteLine();
        }

        #region отрисовка списка-дерева

        /// <summary>Отображает текущую страницу списка файлов в виде дерева</summary>
        /// <param name="fullWidth">Ширина области отображения</param>
        public static void DrawList(int fullWidth)
        {
            // отрисовываем заголовок

            Utils.DrawLine(fullWidth, Utils.ch3, Utils.ch4);
            Console.WriteLine();
            Utils.DrawLine(fullWidth, $"Page {PageNumber} / {PageCount}, Total items: {list.Count}");

            // отрисовываем среднюю часть рамки
            Utils.DrawLine(fullWidth, '╞', '═', '╡');
            Console.WriteLine();

            int start = (PageNumber - 1) * PageSize; // индекс первой отображаемой строки списка на текущей странице
            int end = start + PageSize - 1; // индекс последней отображаемой строки списка на текущей странице

            // массив для ослеживания изменения мод отображения
            // элементов дерева с глубиной == 2
            FileListEntryMode[] modes = new FileListEntryMode[3] {
                FileListEntryMode.First, FileListEntryMode.Last, FileListEntryMode.Last
            };

            // вычисляем количество цифр в номере строки списка
            // чтобы правильно отрисовать соотвествующую колонку
            int digitsInLineNumber = CountDigits(end + 1);

            // формат строки для отображения номера строки списка
            // в колонке соотвествующей ширины
            string lineNumberFormat = $"{{0,{digitsInLineNumber}}}.";

            for (int i = start; i <= end; ++i)
            {
                DrawListLine(fullWidth, i, lineNumberFormat, digitsInLineNumber + 1, modes);
            }
        }

        /// <summary>
        /// Отображает текущую строку страницы списка файлов в виде дерева
        /// </summary>
        /// <param name="fullWidth">Ширина области отображения</param>
        /// <param name="lineIndex">Индекс строки</param>
        /// <param name="lineNumberFormat">Формат строки для отображения номера строки списка в колонке соотвествующей ширины</param>
        /// <param name="lineNumberColumnWidth">Количество цифр в номере строки списка</param>
        /// <param name="modes">Моды отображения</param>
        private static void DrawListLine(int fullWidth, int lineIndex, string lineNumberFormat, int lineNumberColumnWidth, FileListEntryMode[] modes)
        {
            if (lineIndex >= TotalCount)
            {
                // строка за пределами списка =>
                // отображаем только рамку
                Utils.DrawLine(fullWidth, string.Empty);
                Console.WriteLine();
                return;
            }

            var entry = list[lineIndex];

            // отображаем левую границу рамки
            Console.Write(Utils.ch2);

            // отображаем номер с строки списка
            Console.Write(lineNumberFormat, lineIndex + 1);

            #region отображение линий древовидной структуры

            // отрисовка линий древовидной структуры для
            // вышележащих уровней
            if (entry.depth > 0)
            {
                if (modes[0] == FileListEntryMode.Last)
                {
                    // элементы на уровне 0 уже закончились
                    // => не рисуем линий для этого уровня
                    Console.Write(' ');
                }
                else
                {
                    // ниже есть элементы уровня 0 =>
                    // рисуем линию сверху вниз для этого уровня
                    Console.Write(Utils.ch2);
                }
                Console.Write(' ');

                if (entry.depth > 1)
                {
                    if (modes[1] == FileListEntryMode.Last)
                    {
                        // элементы на уровне 1 уже закончились
                        // => не рисуем линий для этого уровня
                        Console.Write(' ');
                    }
                    else
                    {
                        // ниже есть элементы уровня 1 =>
                        // рисуем линию сверху вниз для этого уровня
                        Console.Write(Utils.ch2);
                    }
                    Console.Write(' ');
                }
            }

            // отрисовка линий древовидной структуры для последнего
            // уровня вложенности (перед именем элемента списка)
            switch (entry.Mode)
            {
                case FileListEntryMode.Last:
                    // отрисовка последней строки подсписка
                    modes[entry.depth] = FileListEntryMode.Last;
                    Console.Write(Utils.ch5);
                    break;

                case FileListEntryMode.Only:
                    // отрисовка единственной строки подсписка
                    modes[entry.depth] = FileListEntryMode.Last;
                    if (entry.depth == 0)
                        Console.Write(' ');
                    else
                        Console.Write(Utils.ch5);
                    break;

                case FileListEntryMode.Middle:
                    // отрисовка средней строки подсписка
                    modes[entry.depth] = FileListEntryMode.Middle;
                    Console.Write(Utils.ch7);
                    break;

                case FileListEntryMode.First:
                    // отрисовка первой строки подсписка
                    modes[entry.depth] = FileListEntryMode.First;
                    if (entry.depth == 0)
                        Console.Write(Utils.ch9);
                    else
                        Console.Write(Utils.ch7);
                    break;
            }

            Console.Write(Utils.ch1);

            #endregion отображение линий древовидной структуры

            #region отображение имени элемента списка

            int maxLength = fullWidth - lineNumberColumnWidth - (entry.depth * 2) - 5;
            if (maxLength > 5)
            {
                // формат строки с учётом ширины свободного места 
                string fmt = $" {{0, -{maxLength}}}";
                string displayName = entry.Name;
                if (displayName.Length > maxLength)
                {
                    // имя выходит за пределы свободного места
                    // => обрезаем строку и добавляем в конец строки многоточие
                    displayName = displayName.Substring(0, maxLength - 4) + "...";
                }

                var fc = Console.ForegroundColor;
                var bc = Console.BackgroundColor;
                try
                {
                    #region установка цветов отображения имени

                    if (lineIndex == _SelectedIndex)
                    {
                        // отображаемый элемент выделен

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.BackgroundColor = ConsoleColor.Blue;
                    }
                    else if (entry.Kind == FileListEntryKind.Drive)
                    {
                        // отображаемый элемент соотвествует диску

                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        //Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }
                    else if (entry.Kind == FileListEntryKind.Directory)
                    {
                        // отображаемый элемент соотвествует директории

                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        //Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }
                    else
                    {
                        // отображаемый элемент соотвествует файлу

                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        //Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }

                    #endregion установка цветов отображения имени

                    Console.Write(fmt, displayName);
                }
                finally
                {
                    Console.ForegroundColor = fc;
                    Console.BackgroundColor = bc;
                }
            }

            #endregion отображение имени элемента списка

            // отображаем правую границу рамки
            Console.WriteLine(Utils.ch2);
        }

        /// <summary>Вычисляет количество десятичных цифр для отображения указанного числа.</summary>
        private static int CountDigits(int number)
        {
            int digits = 0;
            while (number != 0) { ++digits; number /= 10; }
            return digits;
        }

        #endregion отрисовка списка-дерева

        /// <summary>Опции итерации элементов объектов файловой системы.</summary>
        static EnumerationOptions enumFsOptions = new EnumerationOptions();

        /// <summary>Дополняет список директориями и файлами системы, которые содержатся в заданной директории</summary>
        /// <param name="list">пополняемый список.</param>
        /// <param name="depth">текущая глубина погружения в поддерево.</param>
        /// <param name="dirPath">путь к директории, содержимое которой дополнит список.</param>
        private static void ScanDir(List<FileListEntry> list, int depth, string dirPath)
        {
            if (depth > 2)
            {
                // глубже 2х не идём
                return;
            }

            // в данной переменной будет последний добавленный элемент в списке текущего родителя
            FileListEntry lastEntry = null;
            // исходное описатель положения элемента в списке текущего родителя
            FileListEntryMode mode = FileListEntryMode.First;
            try
            {
                if (string.IsNullOrEmpty(dirPath))
                {
                    // дополняем список активными устройствами хранения

                    DriveInfo[] discs = DriveInfo.GetDrives();
                    foreach (DriveInfo disc in discs)
                    {
                        if (!disc.IsReady) continue;

                        DirectoryInfo di = new DirectoryInfo(disc.Name);
                        var driveEntry = new FileListEntry
                        {
                            depth = depth,
                            Mode = mode,
                            Kind = FileListEntryKind.Drive,
                            Name = di.FullName,
                            FullPath = di.FullName
                        };
                        list.Add(driveEntry);
                        mode = FileListEntryMode.Middle;
                        lastEntry = driveEntry;
                    }
                }
                else
                {
                    // дополняем список директориями
                    foreach (var path in Directory.EnumerateDirectories(dirPath, "*", enumFsOptions))
                    {
                        var dirEntry = new FileListEntry
                        {
                            depth = depth,
                            Mode = mode,
                            Kind = FileListEntryKind.Directory,
                            Name = Path.GetFileName(path),
                            FullPath = path
                        };
                        list.Add(dirEntry);
                        mode = FileListEntryMode.Middle;
                        lastEntry = dirEntry;

                        // дополним список, тем что содержится в текущей директории
                        ScanDir(list, depth + 1, path);
                    }
                    // дополняем список файлами
                    foreach (var path in Directory.EnumerateFiles(dirPath, "*", enumFsOptions))
                    {
                        var fileEntry = new FileListEntry
                        {
                            depth = depth,
                            Mode = mode,
                            Kind = FileListEntryKind.File,
                            Name = Path.GetFileName(path),
                            FullPath = path
                        };
                        list.Add(fileEntry);
                        mode = FileListEntryMode.Middle;
                        lastEntry = fileEntry;
                    }
                }
            }
            catch (Exception ex)
            {
                // игнорируем ошибки доступа
            }
            if (lastEntry != null)
            {
                // дополняем последний добавленный элемент признаком "последний"
                lastEntry.Mode |= FileListEntryMode.Last;
            }
        }
    }
}
