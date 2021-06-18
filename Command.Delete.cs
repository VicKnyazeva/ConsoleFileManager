using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleFileManager
{
    /// <summary>реализует команду Delete</summary>
    class DeleteCommand : Command
    {
        public override string Names => "del, delete";
        public override string ShortDescription => "deletes file or folder";
        public override string DetailedDescription =>
            "  {0} <path> - delete file/folder\n";

        public override string Execute(List<string> args)
        {
            string target = args.Count > 1 ? args[1] : null;

            string path = target != null
                ? Path.GetFullPath(target)
                : FileList.SelectedPath;

            string message = null;
            if (path == null)
            {
                Utils.ConsoleWriteLine(Utils.SeverityLevel.Warn, "There is no selection.");
            }
            else if (!Utils.Confirm($"Are you sure to delete '{path}'?"))
            {
                return message;
            }
            else
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Utils.DeleteDirectory(path, true);
                    }
                    else if (File.Exists(path))
                    {
                        Utils.FileDelete(path);
                    }
                    else
                    {
                        Utils.ConsoleWriteLine(Utils.SeverityLevel.Info, "File or directory '{0}' is not found.", path);
                    }
                    FileList.Reload();
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            Utils.ConsoleWait();
            return message;
        }
    }
}
