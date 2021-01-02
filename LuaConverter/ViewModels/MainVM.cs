using LuaConverter.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuaConverter.ViewModels
{
    class MainVM : ViewModelBase
    {
        public static MainVM GetInstance { get; private set; }
        public static FileManager GetFileManager { get; private set; }
        private ViewModelBase currentView = new LoadFileVM();
        public ViewModelBase CurrentView
        {
            get
            {
                return currentView;
            }

            set
            {
                if (currentView != value)
                {
                    currentView = value;
                    OnPropertyChanged(nameof(CurrentView));
                }
            }
        }
        public MainVM()
        {
            GetInstance = this;
            GetFileManager = new FileManager();
        }
    }
}
