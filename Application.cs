using System;
using System.IO;
using System.Text.Json;

namespace ConsoleFileManager
{
    class Application
    {
        internal void Init(string[] args)
        {
            var settings = this.LoadConfig();
            FileList.Init(settings);
        }

        internal void Stop()
        {
            this.SaveConfig();
            Console.WriteLine("The program has been successfully finished.");
        }

        /// <summary>
        /// Основной цикл приложения
        /// </summary>
        internal void Run()
        {
            string message = "Welcome to 'CONSOLE FILE MANAGER' program!\n\n" +
                CommandManager.HelpText();

            for (; ; )
            {
                if (message != null)
                {
                    Console.Clear();
                    Console.WriteLine(message);
                    Console.WriteLine();
                    Console.WriteLine("Press ANY key to continue");
                    Console.ReadKey();
                }
                try
                {
                    Console.CursorVisible = false;
                    Console.Clear();

                    int fullWidth = Console.BufferWidth;
                    FileList.DrawList(fullWidth);
                    FileList.DrawInfo(fullWidth);

                    Console.Write("> ");
                }
                finally
                {
                    Console.CursorVisible = true;
                }

                string cmd = Console.ReadLine()?.Trim();
                message = CommandManager.ProcessCommandLine(cmd, out bool quitProgram);
                if (quitProgram)
                    return;
            }
        }

        const string ConfigFileName = "FmApp.configuration";

        /// <summary>
        /// Загрузка конфигурационного файла
        /// </summary>
        private ApplicationSettings LoadConfig()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(path, ConfigFileName);

            ApplicationSettings settings;
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<ApplicationSettings>(json);
                }
                catch
                {
                }
            }
            return new ApplicationSettings();
        }

        /// <summary>
        /// Сохранение конфигурационного файла
        /// </summary>
        private void SaveConfig()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(path, ConfigFileName);

            var settings = new ApplicationSettings()
            {
                PageSize = FileList.PageSize,
                PageNumber = FileList.PageNumber,
                Folder = FileList.CurrentDirectory,
                Selected = FileList.SelectedPath
            };
            var json = JsonSerializer.Serialize<ApplicationSettings>(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }
    }
}
