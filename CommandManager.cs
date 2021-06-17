using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleFileManager
{
    static class CommandManager
    {
        /// <summary>Отображение названий и сокращений команд в соответвующие команды</summary>
        static readonly Dictionary<string, Command> allCommands = new Dictionary<string, Command>();

        /// <summary>Добавляет (регистрирует) команду</summary>
        /// <param name="command">объект, реализующимй команду</param>
        private static void RegisterCommand(Command command)
        {
            var names = command.Names.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var cmdid in names)
            {
                allCommands.Add(cmdid.Trim(), command);
            }
        }

        /// <summary>Разбирает командную строку и строит список её елементов.
        /// </summary>
        /// <remarks>если элемент командной строки должен содержать пробел,
        /// то тогда он должен выделяться кавычками (например, <code>"привет мир")</code></remarks>
        /// <param name="cmd">командная строка</param>
        /// <returns>список елементов командной строки</returns>
        public static List<string> ParseCommandLine(string cmd)
        {
            List<string> args = new List<string>();
            bool quote = false;
            StringBuilder sb = new StringBuilder();
            char ch = ' ', pc = ' ';
            for (int i = 0; i < cmd.Length; ++i, pc = ch)
            {
                ch = cmd[i];
                if (quote)
                {
                    if (ch != '"')
                    {
                        sb.Append(ch);
                    }
                    else
                    {
                        quote = false;
                    }
                    continue;
                }
                if (char.IsWhiteSpace(ch))
                {
                    if (!char.IsWhiteSpace(pc)) // граница аргумента )
                    {
                        if (sb.Length > 0)
                        {
                            args.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                }
                else
                {
                    if (char.IsWhiteSpace(pc))
                    {
                        // начало аргумента (
                        if (ch == '"')
                        {
                            quote = true;
                            continue;
                        }
                    }
                    sb.Append(ch);
                }
            }
            if (sb.Length > 0)
            {
                args.Add(sb.ToString());
            }
            return args;
        }

        /// <summary>Обрабатывает командную строку</summary>
        /// <param name="commandLine">командная строка</param>
        /// <param name="quitProgram">признак необходимости завершения приложения</param>
        /// <returns>строка с текстом сообщения или описания ошибки, или null</returns>
        public static string ProcessCommandLine(string commandLine, out bool quitProgram)
        {
            quitProgram = commandLine == null;
            if (quitProgram)
            {
                // командная строка отсутвует
                // => пора выйти из приложения

                return null;
            }

            var args = ParseCommandLine(commandLine);
            if (args.Count == 0)
            {
                // командная строка пустая
                // => обработка не требуется

                return null;
            }

            // первый элемент командной строки - название команды
            var cmdName = args[0].ToLower();
            if (!allCommands.TryGetValue(cmdName, out Command command))
            {
                // указанное название не найдено среди зарегистрированных
                // => выводим текст справки со списком всех команд приложения

                return HelpText(cmdName);
            }

            if (command is QuitCommand)
            {
                // указанное название соотвествует встроенной команде
                // выхода из приложения => обрабатываем специальным образом
                quitProgram = true;
                return null;
            }

            // остальные команды обрабатываем обычным образом

            return command.Execute(args);
        }

        /// <summary>Возвращает список всех доступных команд приложения
        /// или детальное описание указанной команды</summary>
        /// <param name="commandId">название команды или его сокращение</param>
        public static string HelpText(string commandId = null)
        {
            StringBuilder sb = new StringBuilder();

            Command command = null;
            commandId = commandId?.ToLower().Trim();
            if (commandId != null && !allCommands.TryGetValue(commandId, out command))
            {
                // запрошенная команда не найдена среди зарегистрированных
                sb.AppendFormat("Command '{0}' is not found.\n\n", commandId);
                commandId = null;
            }
            if (command == null)
            {
                // дополняем списком зарегистрированных команд с их кратким описанием

                sb.AppendLine("Available commands:");
                sb.AppendFormat("  {0,-15} - {1}\n", "<empty command>", "refreshes screen");

                var commands = allCommands.Values.Distinct();
                foreach (var c in commands)
                {
                    sb.AppendFormat("  {0,-15} - {1}\n", c.Names, c.ShortDescription);
                }
            }
            else
            {
                // дополняем описание конкретной команды с её детальным описанием

                sb.Append($"Command '{commandId}' {command.ShortDescription}\n\n");
                sb.AppendFormat(command.DetailedDescription, commandId);
            }
            return sb.ToString();
        }

        static CommandManager()
        {
            // регистрируем команды приложения

            // встроенные (обязательные) команды
            RegisterCommand(new QuitCommand());
            RegisterCommand(new HelpCommand());

            // остальные команды приложения
            RegisterCommand(new SelectCommand());
            RegisterCommand(new PageCommand());
            RegisterCommand(new ListCommand());

            RegisterCommand(new CreateCommand());
            RegisterCommand(new RenameCommand());
            RegisterCommand(new CopyCommand());
            RegisterCommand(new MoveCommand());
            RegisterCommand(new DeleteCommand());
        }

        /// <summary>Реализует встроенную команду выхода из приложения</summary>
        class QuitCommand : Command
        {
            public override string Names => "q, quit";
            public override string ShortDescription => "saves settings and quits the program";
            public override string DetailedDescription => "";

            public override string Execute(List<string> args)
            {
                return null;
            }
        }

        /// <summary>Реализует встроенную команду получения справки</summary>
        class HelpCommand : Command
        {
            public override string Names => "h, help";
            public override string ShortDescription => "shows help text";
            public override string DetailedDescription =>
                "  {0}           - shows commands list\n" +
                "  {0} <command> - shows help for <commands>";

            public override string Execute(List<string> args)
            {
                string command = args.Count > 1 ? args[1] : null;
                return HelpText(command);
            }
        }
    }

}
