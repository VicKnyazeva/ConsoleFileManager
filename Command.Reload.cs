using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleFileManager
{
    /// <summary>реализует команду Reload</summary>
    class ReloadCommand : Command
    {
        public override string Names => "r, rel, reload";
        public override string ShortDescription => "reload list of files";
        public override string DetailedDescription =>
            "  {0} - reloads files\n";

        public override string Execute(List<string> args)
        {
            string message = null;
            try
            {
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
