using LuaConverter.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace LuaConverter.ViewModels
{
    class LoadFileVM : ViewModelBase
    {
        private string labelText = "Upuść tu plik lua";

        public string LabelText
        {
            get
            {
                return labelText;
            }
            set
            {
                labelText = value;
                OnPropertyChanged(nameof(LabelText));
            }
        }

        public void FileDropEvent(object sender, DragEventArgs e)
        {
            if (!FileManager.FileIsLoaded && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 1)
                {
                    MessageBox.Show("Wybierz tylko jeden plik");
                    return;
                }

                if (files.Length == 1)
                {
                    if (files[0].EndsWith(".lua"))
                    {
                        LabelText = "Wczytuje plik lua";        
                        MainVM.GetFileManager.LoadFileAsync(files[0]);
                    }
                    else
                    {
                        MessageBox.Show("Złe rozszerzenie");
                        return;
                    }
                }
            }
        }

        public LoadFileVM()
        {

        }


    }
}
