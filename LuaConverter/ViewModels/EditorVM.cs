using LuaConverter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace LuaConverter.ViewModels
{
    class EditorVM : ViewModelBase
    {
        private Item item;
        private FileManager fileManager;
        private bool onlyEnglish = true;
        //private bool clearUnidentified = false;
        private int fontSize = 16;
        const string url = "https://file5s.ratemyserver.net/items/small/";
        public bool hideUnidentified = true;

        private Visibility visibility = Visibility.Hidden;
        private string searchText = "";
        private ICommand findButton;
        private ICommand saveButton;
        private ICommand leftButton;
        private ICommand rightButton;

        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private string imageUrl;
        private void ChangeVisibility()
        {
            if(hideUnidentified)
            {
                Visibility = Visibility.Hidden;
            }
            else
            {
                Visibility = Visibility.Visible;
            }
        }

        #region properties
        public ICommand FindButton
        {
            get
            {
                return findButton;
            }
        }
        public ICommand SaveButton
        {
            get
            {
                return saveButton;
            }
        }

        public ICommand RightButton
        {
            get
            {
                return rightButton;
            }
        }

        public ICommand LeftButton
        {
            get
            {
                return leftButton;
            }
        }

        public string ToTranslate
        {
            get
            {
                return FileManager.ItemsToTranslate.ToString();
            }
        }
        public Visibility Visibility
        {
            get
            {
                return visibility;
            }
            set
            {
                visibility = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }
        public bool HideUnidentified
        {
            get
            {
                return hideUnidentified;
            }
            set
            {
                hideUnidentified = value;
                OnPropertyChanged(nameof(HideUnidentified));
                ChangeVisibility();
            }
        }
        public bool OnlyEnglish
        {
            get
            {
                return onlyEnglish;
            }
            set
            {
                onlyEnglish = value;
                OnPropertyChanged(nameof(OnlyEnglish));
                FindFirst();
            }
        }
        public Item GetItem
        {
            get
            {
                return item;
            }
            set
            {
                if(item != null && item.IsEnglish && !FileManager.CheckEnglish(item.IdentifiedDisplayName, item.IdentifiedDescriptionName))
                {
                    Translated();
                }
                item = value;
                OnPropertyChanged(nameof(GetItem));
                SetupImage();
                OnPropertyChanged(nameof(IsEnglish));
                OnPropertyChanged(nameof(HideUnidentified));
                ChangeVisibility();
            }
        }

        private void Translated()
        {
            item.IsEnglish = false;
            FileManager.ItemsToTranslate = FileManager.ItemsToTranslate-1;
            OnPropertyChanged(nameof(ToTranslate));
            OnPropertyChanged(nameof(GetItem));
        }

        public string IsEnglish
        {
            get 
            {
                if(item != null && item.IsEnglish)
                {
                    return "Nie przetłumaczone";
                }
                else
                {
                    return "Przetlumaczone";
                }
            }
        }

      
        public int FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }
    

       
        public string GetImage
        {
            get { return imageUrl; }
            set
            {
                imageUrl = value;
                OnPropertyChanged(nameof(GetImage));
            }
        }
        #endregion

        private void SetupImage()
        {
            GetImage = url + item.ID + ".gif";
        }

        List<int> KeyList;
        private int selectedKey;
        public int SelectedKey
        {
            get
            {
                return selectedKey;
            }
            set
            {
                if( value < KeyList.Count && value >= 0 )
                {
                    selectedKey = value;
                    OnPropertyChanged(nameof(SelectedKey));
                    GetItem = fileManager.ItemsDatabase[KeyList[value]];
                }
            }
        }
        public void FindFirst()
        {
            if (OnlyEnglish)
            {
                for (int i = 0; i < KeyList.Count; i++)
                {
                    if (fileManager.ItemsDatabase[KeyList[i]].IsEnglish)
                    {
                        SelectedKey = i;
                        break;
                    }
                }
            }
            else
            {
                SelectedKey = 0;
            }
        }

        public void GoNext(object param)
        {
            if (SelectedKey+1 < KeyList.Count)
            {
                if (OnlyEnglish)
                {
                    for (int i = SelectedKey+1; i < KeyList.Count; i++)
                    {
                        if (fileManager.ItemsDatabase[KeyList[i]].IsEnglish)
                        {
                            SelectedKey = i;
                            break;
                        }
                    }
                }
                else
                {
                    SelectedKey = SelectedKey + 1;
                }
            }
        }

        public void GoPrevious(object param)
        {
            if(SelectedKey-1 >= 0)
            {
                if(OnlyEnglish)
                {
                    for(int i=SelectedKey-1;i>=0;i--)
                    {
                        if(fileManager.ItemsDatabase[KeyList[i]].IsEnglish)
                        {
                            SelectedKey = i;
                            break;
                        }
                    }
                }
                else
                {
                    SelectedKey = SelectedKey-1;
                }
            }
        }

        public void Find(object param)
        {
            int id;
            if(int.TryParse(searchText, out id))
            {
                int index = KeyList.IndexOf(id);
                if (index >= 0)
                {
                    SelectedKey = index;
                }
            }
        }


        private int TickCount = 0;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
            TickCount++;
            Trace.WriteLine("tick");
            if(TickCount%60==0)
            {
                AutoSave();
            }
        }


        public void AutoSave()
        {
            if(FileManager.LoadedFilePath != null)
            {
                string path = FileManager.LoadedFilePath.Substring(0, FileManager.LoadedFilePath.Length - 4) + "_AutoSave.lua";
                fileManager.SaveFileAsync(path);
            }
        }

        public void Save(object param)
        {
            // Configure open file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "itemInfo_true_V5"; // Default file name
            dlg.DefaultExt = ".lua"; // Default file extension
            dlg.Filter = "Lua documents (.lua)|*.lua"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string path = dlg.FileName;
                fileManager.SaveFileAsync(path);
            }
        }

        public int GetKeyCount
        {
            get
            {
                return KeyList.Count;
            }
        }
        public EditorVM()
        {
            findButton = new RelayCommand(Find);
            leftButton = new RelayCommand(GoPrevious);
            rightButton = new RelayCommand(GoNext);
            saveButton = new RelayCommand(Save);
            fileManager = MainVM.GetFileManager;
            KeyList = fileManager.ItemsDatabase.Keys.ToList();
            FindFirst();

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

        }

    }
}
