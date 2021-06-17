using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleFileManager
{
    /// <summary>реализует команду Mkdir</summary>
    class CreateCommand : Command
    {
        public override string Names => "mkdir, makedir";
        public override string ShortDescription => "creates new folder";
        public override string DetailedDescription =>
            "  {0} <path> - creates folder\n";

        public override string Execute(List<string> args)
        {
            if (args.Count < 2)
                return "params expected";

            string path = Path.GetFullPath(args[1]);

            string message = null;
            try
            {
                Directory.CreateDirectory(path);
                FileList.Reload();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }
    }
}
