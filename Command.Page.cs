using System.Collections.Generic;

namespace ConsoleFileManager
{
    /// <summary>реализует команду Page</summary>
    class PageCommand : Command
    {
        public override string Names => "pg, page";
        public override string ShortDescription => "sets and changes page size";
        public override string DetailedDescription =>
            "  {0} N - sets page size as N lines";

        public override string Execute(List<string> args)
        {
            if (args.Count < 2)
                return "Expected page size value";

            if (!int.TryParse(args[1], out int pageSize) || (0 >= pageSize || pageSize > 100))
                return "Invalid page size value";

            FileList.PageSize = pageSize;
            return null;
        }
    }
}
