using System.Collections.Generic;
using System.IO;

namespace ConsoleFileManager
{
    /// <summary>реализует команду Select</summary>
    class SelectCommand : Command
    {
        public override string Names => "s, sel, select";
        public override string ShortDescription => "changes selection";
        public override string DetailedDescription =>
            "  {0}                   - clear selection\n" +
            "  {0} <item_order_number> - set selection to <item_order_number>\n" +
            "  {0} <item_full_path> - set selection to <item_full_path>";

        public override string Execute(List<string> args)
        {
            int selectedNumber = 0;

            if (args.Count > 1)
            {
                if (int.TryParse(args[1], out int p))
                    selectedNumber = p;
                else
                {
                    string fullPath = Path.GetFullPath(args[1]);
                    selectedNumber = FileList.FindByFullPath(fullPath);
                    if (selectedNumber == 0)
                    {
                        return $"File or folder '{fullPath}' is not found in the list.";
                    }
                }
            }
            return FileList.SetSelection(selectedNumber);
        }
    }
}
