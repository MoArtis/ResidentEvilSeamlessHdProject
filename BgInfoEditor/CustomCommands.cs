using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BgInfoEditor
{
    public class CustomCommands
    {
        static CustomCommands()
        {
            SelectBgInfo = new RoutedCommand("SelectBgInfo", typeof(CustomCommands));
        }

        public static RoutedCommand SelectBgInfo { get; private set; }
    }
}
