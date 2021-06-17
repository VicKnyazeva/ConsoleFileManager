using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleFileManager
{
    /// <summary>реализует команду List</summary>
    class ListCommand : Command
    {
        public override string Names => "ls, list";
        public override string ShortDescription => "lists all files and folders";
        public override string DetailedDescription =>
            "  {0}                  - views disks list\n" +
            "  {0} -p N             - views page N\n" +
            "  {0} <directory>      - show files and folders in folder <directory>\n" +
            "  {0} <directory> -p N - same as above and view page N\n" +
            "  {0} -p N <directory> - same as above";

        private void ParseParams(List<string> args, out string source, out int pageNumber)
        {
            source = null;
            pageNumber = 1;

            List<string> argsCopy = new List<string>(args);
            argsCopy.RemoveAt(0); // remove command name
            
            if (argsCopy.Count == 0)
            {
                source = string.Empty;
                return;
            }

            int pageParamIndex = argsCopy.IndexOf("-p");
            if (pageParamIndex >= 0)
            {
                argsCopy.RemoveAt(pageParamIndex); // remove "-p"
                if (pageParamIndex < argsCopy.Count && int.TryParse(argsCopy[pageParamIndex], out int p))
                {
                    argsCopy.RemoveAt(pageParamIndex); // remove page number
                    pageNumber = p;
                }
            }

            source = argsCopy.FirstOrDefault()?.Trim();
            if (source != null)
                source = Path.GetFullPath(source);

            if (pageNumber > FileList.PageCount)
                pageNumber = FileList.PageCount;
            if (pageNumber < 1)
                pageNumber = 1;
        }

        public override string Execute(List<string> args)
        {
            ParseParams(args, out string source, out int pageNumber);

            if (source == string.Empty)
            {
                // show disks list
                return FileList.SetCurrentDirectory(null);
            }

            FileList.PageNumber = pageNumber;
            // do file or dir copy here

            if (string.IsNullOrEmpty(source))
            {
                // change page number only
                return null;
            }
            if (!Directory.Exists(source))
            {
                return $"Directory \"{source}\" is not found";
            }
            return FileList.SetCurrentDirectory(source);
        }
    }
}
