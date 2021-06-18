using System;
using System.IO;

namespace ConsoleFileManager
{
    static partial class Utils
    {
        #region отрисовка

        public const char ch1 = '─';
        public const char ch2 = '│';
        public const char ch3 = '┌';
        public const char ch4 = '┐';
        public const char ch5 = '└';
        public const char ch6 = '┘';
        public const char ch7 = '├';
        public const char ch8 = '┤';
        public const char ch9 = '┬';
        public const char ch0 = '┴';
        public const char chA = '┼';

        /// <summary>Отображает горизонтальную линию с заданными граничными символами</summary>
        /// <param name="width">длина линии</param>
        /// <param name="left">крайний левый символ</param>
        /// <param name="right">крайний правый символ</param>
        public static void DrawLine(int width, char left, char right)
        {
            DrawLine(width, left, ch1, right);
        }

        /// <summary>Отображает горизонтальную линию с заданными граничными символами</summary>
        /// <param name="width">длина линии</param>
        /// <param name="left">крайний левый символ</param>
        /// <param name="inside">символ заполняющий промежуток</param>
        /// <param name="right">крайний правый символ</param>
        public static void DrawLine(int width, char left, char inside, char right)
        {
            Console.Write(left); --width;
            while (width > 1)
            {
                Console.Write(inside);
                --width;
            }
            if (width > 0)
                Console.Write(right);
        }

        /// <summary>Отображает строку в пределах рамки заданной ширины</summary>
        /// <param name="width">ширина рамки</param>
        /// <param name="textLine">строка для оборажения</param>
        public static void DrawLine(int width, string textLine)
        {
            Console.Write(ch2);

            int innerWidth = width - 2;
            if (innerWidth > 0)
            {
                if (textLine.Length > innerWidth)
                {
                    if (innerWidth > 3)
                    {
                        textLine = textLine.Substring(0, innerWidth - 3) + "...";
                    }
                    else
                    {
                        textLine = textLine.Substring(0, innerWidth);
                    }
                }
                string fmt = $"{{0,-{innerWidth}}}";
                Console.Write(fmt, textLine);
            }
            if (width > 2)
                Console.Write(ch2);
        }

        #endregion отрисовка

        enum FileOverwriteMode
        {
            Ask,
            IgnoreFile,
            IgnoreAll,
            OverwriteFile,
            OverwriteAll
        }

        /// <summary>Вспомогательный класс для хранения контекста операций копирования и перемещения</summary>
        class OperationContext
        {
            public OperationContext(bool removeSource)
            {
                this.removeSource = removeSource;
            }

            /// <summary>признак необходимости удаления исходных файлов/директорий по ходу операции</summary>
            public readonly bool removeSource;

            /// <summary>текущая мода "обработки перезаписи" файла</summary>
            public FileOverwriteMode overwriteMode = FileOverwriteMode.Ask;

            /// <summary>Общее количество файлов, просмотренных по ходу операции</summary>
            public int TotalFileCount = 0;
            /// <summary>Общее количество файлов, обработанных по ходу операции</summary>
            public int ProcessedFileCount = 0;

            /// <summary>Общее количество директорий, просмотренных по ходу операции</summary>
            public int TotalDirectoryCount = 0;
            /// <summary>Общее количество директорий, обработанных по ходу операции</summary>
            public int ProcessedDirectoryCount = 0;
        }

        public static bool Confirm(string question)
        {
            ConsoleWrite(SeverityLevel.Warn, "{0}\n[y],[Y] - YES, [n],[N] - NO >> ", question);
            while (true)
            {
                switch (char.ToLower(Console.ReadKey(true).KeyChar))
                {
                    default:
                        break;
                    case 'y':
                        Console.WriteLine("[Y]");
                        return true;
                    case 'n':
                        Console.WriteLine("[N]");
                        return false;
                }
            }
        }

        /// <summary>Запрашивает у пользователя моду "обработки перезаписи" файла</summary>
        /// <param name="overwriteMode">текущее значение моды "обработки перезаписи" файла </param>
        /// <param name="filePath">полный путь файла</param>
        /// <returns>Полученная мода "обработки перезаписи" файла</returns>
        private static FileOverwriteMode GetFileOwerwriteMode(FileOverwriteMode overwriteMode, string filePath)
        {
            switch (overwriteMode)
            {
                case FileOverwriteMode.IgnoreAll:
                case FileOverwriteMode.OverwriteAll:
                    return overwriteMode;
            }
            ConsoleWrite(SeverityLevel.Warn, "File '{0}' already exists.\n[i] - ignore this file, [I] - ignore all files, [r] - replace, [R] - replace all files >> ", filePath);
            while (true)
            {
                switch (Console.ReadKey(true).KeyChar)
                {
                    default:
                        break;
                    case 'i':
                        Console.WriteLine("[i]");
                        return FileOverwriteMode.IgnoreFile;
                    case 'I':
                        Console.WriteLine("[I]");
                        return FileOverwriteMode.IgnoreAll;
                    case 'r':
                        Console.WriteLine("[r]");
                        return FileOverwriteMode.OverwriteFile;
                    case 'R':
                        Console.WriteLine("[R]");
                        return FileOverwriteMode.OverwriteAll;
                }
            }
        }

        /// <summary>Копирует/перемещает директорию со всеми вложенными элементами</summary>
        /// <param name="srcDir">путь к исходной папке/файлу</param>
        /// <param name="tgtDir">путь к целевой папке/файлу</param>
        /// <param name="removeSource">признак необходимости удаления исходных файлов/директорий по ходу операции
        /// <para>Если <c>false</c> - исходные файлы и папки остаются (копирование)</para>
        /// <para>Если <c>true</c> - исходные файлы и папки удалются (перемещение)</para>
        /// </param>
        /// <returns>Сводка об операции - если всё прошло успешно, иначе текст с описанием ошибки.</returns>
        public static string CopyDirectory(string srcDir, string tgtDir, bool removeSource = false)
        {
            string operationName = removeSource ? "Move" : "Copy";
            var context = new OperationContext(removeSource);
            
            CopyDirectory(context, srcDir, tgtDir);
        
            Console.WriteLine(operationName + " complete.\n" +
                $"  processed directories: {context.ProcessedDirectoryCount} from {context.TotalDirectoryCount}\n" +
                $"        processed files: {context.ProcessedFileCount} from {context.TotalFileCount}");
            ConsoleWait();
            return null;
        }

        /// <summary>Обрабатывает заданную директорию со всем её содержимым.</summary>
        private static void CopyDirectory(OperationContext context, string srcDir, string tgtDir)
        {
            ++context.TotalDirectoryCount;

            // создаём новую целевую папку target, если её нет,
            // иначе используем существующую папку
            CreateDirectory(tgtDir, out DirectoryInfo target);

            // обрабатываем все подпапки в srcDir
            foreach (var subdir in Directory.EnumerateDirectories(srcDir))
            {
                string name = Path.GetFileName(subdir);
                string path = Path.Combine(target.FullName, name);

                // обрабатываем подпапку
                CopyDirectory(context, subdir, path);
            }

            // обрабатываем все файлы в srcDir
            foreach (var file in Directory.EnumerateFiles(srcDir))
            {
                ++context.TotalFileCount;

                string name = Path.GetFileName(file);
                string path = Path.Combine(target.FullName, name);
                if (!File.Exists(path))
                {
                    if (FileCopy(file, path))
                    {
                        ConsoleWriteLine(SeverityLevel.Done, "File '{0}' - created", path);
                        if (context.removeSource)
                        {
                            // контекс операции перемещения
                            // => пробуем удалить исходный файл
                            FileDelete(file);
                        }
                        // удалось полностью обработать исходный файл
                        // => увеличиваем количество успешно обработанных файлов
                        ++context.ProcessedFileCount;
                    }
                }
                else
                {
                    var mode = GetFileOwerwriteMode(context.overwriteMode, path);
                    switch (mode)
                    {
                        case FileOverwriteMode.IgnoreAll:
                            context.overwriteMode = mode;
                            goto case FileOverwriteMode.IgnoreFile;

                        case FileOverwriteMode.OverwriteAll:
                            context.overwriteMode = mode;
                            goto case FileOverwriteMode.OverwriteFile;

                        case FileOverwriteMode.IgnoreFile:
                            ConsoleWriteLine(SeverityLevel.Info, "File '{0}' - ignored", path);
                            break;

                        case FileOverwriteMode.OverwriteFile:
                            if (FileCopy(file, path, true))
                            {
                                ConsoleWriteLine(SeverityLevel.Done, "File '{0}' - overwrited", path);
                                if (context.removeSource)
                                {
                                    // контекс операции перемещения
                                    // => пробуем удалить исходный файл
                                    FileDelete(file);
                                }
                                // удалось полностью обработать исходный файл
                                // => увеличиваем количество успешно обработанных файлов
                                ++context.ProcessedFileCount;
                            }
                            break;
                    }
                }
            }

            // обработка всего содержимого srcDir завершена

            if (context.removeSource)
            {
                // контекс операции перемещения
                // => пробуем удалить srcDir

                if (DeleteDirectory(srcDir))
                {
                    // удалось => увеличиваем количество успешно обработанных директорий
                    ++context.ProcessedDirectoryCount;
                }
            }
            else
            {
                // увеличиваем количество успешно обработанных директорий
                ++context.ProcessedDirectoryCount;
            }
        }

        static bool FileCopy(string src, string tgt, bool overwrite = false)
        {
            if (string.Compare(src, tgt, true) == 0)
            {
                ConsoleWriteLine(SeverityLevel.Fail, "File '{0}' - copy failed\n\t{1}", src,
                    "It is not possible to copy a file over itself.");
                return false;
            }
            try
            {
                File.Copy(src, tgt, overwrite);
                return true;
            }
            catch (Exception ex)
            {
                ConsoleWriteLine(SeverityLevel.Fail, "File '{0}' - copy failed\n\t{1}", src, ex.Message);
                return false;
            }
        }

        public static bool FileDelete(string path)
        {
            try
            {
                File.Delete(path);
                ConsoleWriteLine(SeverityLevel.Done, "File '{0}' - deleted", path);
                return true;
            }
            catch (Exception ex)
            {
                ConsoleWriteLine(SeverityLevel.Fail, "File '{0}' - delete failed\n\t{1}", path, ex.Message);
                return false;
            }
        }

        static bool CreateDirectory(string path, out DirectoryInfo info)
        {
            info = null;
            if (Directory.Exists(path))
            {
                try
                {
                    info = new DirectoryInfo(path);
                    ConsoleWriteLine(SeverityLevel.Info, "Directory '{0}' - already exists", path);
                    return true;
                }
                catch (Exception ex)
                {
                    ConsoleWriteLine(SeverityLevel.Fail, "Directory '{0}' - read failed\n\t{1}", path, ex.Message);
                }
            }
            else
            {
                try
                {
                    info = Directory.CreateDirectory(path);
                    ConsoleWriteLine(SeverityLevel.Done, "Directory '{0}' - created", path);
                    return true;
                }
                catch (Exception ex)
                {
                    ConsoleWriteLine(SeverityLevel.Fail, "Directory '{0}' - create failed\n\t{1}", path, ex.Message);
                }
            }
            return false;
        }

        public static bool DeleteDirectory(string path, bool recursive = false)
        {
            try
            {
                Directory.Delete(path, recursive);
                ConsoleWriteLine(SeverityLevel.Done, "Directory '{0}' - deleted", path);
                return true;
            }
            catch (Exception ex)
            {
                ConsoleWriteLine(SeverityLevel.Fail, "Directory '{0}' - delete failed\n\t{1}", path, ex.Message);
                return false;
            }
        }

        public enum SeverityLevel
        {
            Info,
            Done,
            Warn,
            Fail
        }

        public static void ConsoleWrite(SeverityLevel color, string format, params object[] arg)
        {
            var fc = Console.ForegroundColor;
            var bc = Console.BackgroundColor;
            try
            {
                switch (color)
                {
                    case SeverityLevel.Fail:
#if X
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.BackgroundColor= ConsoleColor.DarkRed;
#else
                        Console.ForegroundColor = ConsoleColor.DarkRed;
#endif
                        break;
                    case SeverityLevel.Warn: Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                    case SeverityLevel.Done: Console.ForegroundColor = ConsoleColor.DarkGreen; break;
                    case SeverityLevel.Info: Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                    default:
                        break;
                }
                Console.Write(format, arg);
            }
            finally
            {
                Console.ForegroundColor = fc;
                Console.BackgroundColor = bc;
            }
        }

        public static void ConsoleWriteLine(SeverityLevel color, string format, params object[] arg)
        {
            ConsoleWrite(color, format, arg);
            Console.WriteLine();
        }

        public static void ConsoleWait()
        {
            Console.Write("Press any key to continue");
            Console.ReadKey(true);
            Console.WriteLine();
        }
    }
}
