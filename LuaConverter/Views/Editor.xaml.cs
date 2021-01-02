using LuaConverter.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LuaConverter.Views
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : UserControl
    {
        public Editor()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int fontSize;
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            if (int.TryParse(text, out fontSize))
            {
                ((EditorVM)DataContext).FontSize = fontSize;
                Trace.WriteLine(fontSize);
            }
        }
    }
}
