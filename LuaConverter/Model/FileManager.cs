using LuaConverter.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace LuaConverter.Model
{
    class FileManager
    {
        public event Action FileLoaded;
        public Dictionary<int, Item> ItemsDatabase { get; private set; }

        private static HashSet<string> WordsDatabase = new HashSet<string>();

        public static int ItemsToTranslate;

        public static string LoadedFilePath;
        public static bool FileIsLoaded = false;


        public FileManager()
        {
            FileLoaded += () => {
                MainVM.GetInstance.CurrentView = new EditorVM();
            };
        }

        public async void SaveFileAsync(string path)
        {
            await Task.Run(async () => {
                StringBuilder output = new StringBuilder("tbl = {\r\n");
                foreach(Item item in ItemsDatabase.Values)
                {
                    output.Append(ItemToString(item));
                }
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "LuaConverter.Resources.EndOfFile.txt";
                string endFile;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        endFile = reader.ReadToEnd();
                    }
                }
                if(endFile != null)
                {
                    output.Append(endFile);
                    //to byte
                    byte[] data = await StringToByte(output.ToString());
                    using (FileStream file = File.Create(path))
                    {
                        file.Write(data, 0, data.Length);
                    }
                }
            });
        }

        private string AddTabulations(string text)
        {
            string[] lines = text.Split("\r\n");
            var tmp = lines.Length - 1;
            for (int i=0;i<tmp;i++)
            {
                lines[i] = "\t\t\t\"" + lines[i]+ "\",";
            }
            lines[tmp] = "\t\t\t\"" + lines[tmp] + "\""; 
            return string.Join("\r\n", lines);
        }

        public string ItemToString(Item item)
        {
            string text = "	[";
            text += item.ID.ToString()+ "] = {\r\n";
            if (item.UnidentifiedResourceName.Length>1)
            {
                text += "\t\tunidentifiedDisplayName = \"" + item.UnidentifiedDisplayName + "\",\r\n";
                text += "\t\tunidentifiedResourceName  = \"" + item.UnidentifiedResourceName + "\",\r\n";
                text += "\t\tunidentifiedDescriptionName = {\r\n" + AddTabulations(item.UnidentifiedDescriptionName) + "\r\n\t\t},\r\n";
            }
            else
            {
                text += "\t\tunidentifiedDisplayName = \"\",\r\n";
                text += "\t\tunidentifiedResourceName  = \"\",\r\n";
                text += "\t\tunidentifiedDescriptionName = {\r\n" + "\t\t},\r\n";
            }
            text += "\t\tidentifiedDisplayName = \"" + item.IdentifiedDisplayName + "\",\r\n";
            text += "\t\tidentifiedResourceName = \"" + item.IdentifiedResourceName + "\",\r\n";
            text += "\t\tidentifiedDescriptionName = {\r\n" + AddTabulations(item.IdentifiedDescriptionName) + "\r\n\t\t},\r\n";
            text += "\t\tslotCount = " + item.SlotCount + ",\r\n";
            text += "\t\tClassNum = " + item.ClassNum + "\r\n\t},\r\n";
            return text;
        }


        /// <summary>
        ///    load file located at path
        /// </summary>
        public async void LoadFileAsync(string path)
        {
            LoadedFilePath = path;
            FileIsLoaded = true;
            await Task.Run(async () => {
                //load words
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "LuaConverter.Resources.words.gz";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (var zip = new GZipStream(stream,CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(zip))
                        {
                            string line;
                            while((line = reader.ReadLine()) != null)
                            {
                                WordsDatabase.Add(line);
                            }
                        }
                    }
                }
                if (File.Exists(path))
                    {
                        byte[] loadedFile;
                        using (FileStream file = new FileStream(path, FileMode.Open))
                        {
                        try
                        {
                            loadedFile = new byte[file.Length];
                            file.Read(loadedFile, 0, (int)file.Length);
                            var fileText = await ByteToString(loadedFile);
                            ItemsDatabase = GetDictionaryFromString(fileText);
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            LoadedFilePath = null;
                            FileIsLoaded = false;
                        }
                        }
                    }
            });
            FileLoaded.Invoke();
        }

        public static string[] GetEmbeddedResourceNames()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

        public Dictionary<int,Item> GetDictionaryFromString(string fileText)
        {
            Dictionary<int, Item> tempDictionary = new Dictionary<int, Item>();
            //item pattern

            Regex regex = new Regex(@"\[(\d{3,})\].*=.*{");
            MatchCollection match = regex.Matches(fileText);
            for(int i=0;i<match.Count;i++)
            {
                int id = int.Parse(match[i].Groups[1].Value);
                int matchIndex = match[i].Index + match[i].Value.Length;
                int[] indexes = new int[8];
                indexes[0] = fileText.IndexOf('=', matchIndex);
                for(int j = 1;j<indexes.Length;j++)
                {
                    indexes[j] = fileText.IndexOf('=', indexes[j-1]+1);
                }

                string unidentifiedDisplayName = TextToObject.OneLineString(ref fileText, indexes[0]);
                string unidentifiedResourceName = TextToObject.OneLineString(ref fileText, indexes[1]);
                string unidentifiedDescriptionName = TextToObject.MultiLineString(ref fileText, indexes[2]);
                string identifiedDisplayName = TextToObject.OneLineString(ref fileText, indexes[3]);
                string identifiedResourceName = TextToObject.OneLineString(ref fileText, indexes[4]);
                string identifiedDescriptionName = TextToObject.MultiLineString(ref fileText, indexes[5]);
                string slotCount = TextToObject.OneLineString(ref fileText, indexes[6]).Replace(",", "");
                string classNum = TextToObject.OneLineString(ref fileText, indexes[7]).Replace(",", "");

                bool isEnglish = CheckEnglish(identifiedDisplayName, identifiedDescriptionName);
                if (isEnglish)
                {
                    ItemsToTranslate++;
                }
                Item item = new Item()
                {
                    ID = id,
                    IdentifiedDescriptionName = identifiedDescriptionName,
                    IdentifiedDisplayName = identifiedDisplayName,
                    IdentifiedResourceName = identifiedResourceName,
                    UnidentifiedDescriptionName = unidentifiedDescriptionName,
                    UnidentifiedDisplayName = unidentifiedDisplayName,
                    UnidentifiedResourceName = unidentifiedResourceName,
                    ClassNum = classNum,
                    SlotCount = slotCount,
                    IsEnglish = isEnglish,
                };
                if (!tempDictionary.ContainsKey(id))
                {
                    tempDictionary.Add(id,item);
                }
                else
                {
                    MessageBox.Show("Przedmiot o id: " + id + " już wystepuje");
                }
            }

            return tempDictionary;
        }

        public static bool CheckEnglish(string text1, string text2)
        {
            int englishCount = 0;
            string[] words1 = text1.Split(" ");
            string[] words2 = text2.Split(" ");
            foreach(string s in words1)
            {
                if(s.Length > 3 && WordsDatabase.Contains(s))
                {
                    englishCount++;
                }
            }
            foreach (string s in words2)
            {
                if (s.Length > 3 && WordsDatabase.Contains(s))
                {
                    englishCount++;
                }
            }

            if(englishCount >= 3)
            {
                return true;
            }
            return false;
        }

        public async Task<byte[]> StringToByte(string text)
        {
            byte[] data = await Task.FromResult(Encoding.GetEncoding(51949).GetBytes(text));
            return data;
        }

        public async Task<string> ByteToString(byte[] array)
        {
            string text = await Task.FromResult(Encoding.GetEncoding(51949).GetString(array));
            return text;
        }
    }
}
