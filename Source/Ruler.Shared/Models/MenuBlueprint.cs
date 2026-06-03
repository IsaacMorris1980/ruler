using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

namespace Ruler.Shared.Models
{
    public class MenuBlueprint
    {
        public string Header { get; set; }
        public ICommand Command { get; set; }
        public string ShortcutText { get; set; }
        public bool IsSeparator { get; set; }
        public string InputGestureText { get; set; }
        public bool IsCheckable { get; set; }
        public bool IsChecked { get; set; }
        public ObservableCollection<MenuBlueprint> SubItems { get; set; } = new ObservableCollection<MenuBlueprint>();

        public MenuBlueprint() { }  
        // Quick factory helper for clean layout building
        public MenuBlueprint(string header, ICommand command, string shortcut = "", bool isCheckable = false, bool isChecked = false)
        {
            Header = header;
            Command = command;
            ShortcutText = shortcut;
            IsCheckable = isCheckable;
            IsChecked = isChecked;
        }
        public MenuBlueprint CreateItem(string header, ICommand command, string shortcut = "", bool isCheckable = false, bool isChecked = false)
        {
            return new MenuBlueprint { Header = header, Command = command, ShortcutText = shortcut, IsCheckable = isCheckable, IsChecked = isChecked };
        }

        public MenuBlueprint CreateSeparator()
        {
            return new MenuBlueprint { IsSeparator = true };
        }
    }
}
