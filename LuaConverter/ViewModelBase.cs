using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace LuaConverter
{
        public abstract class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            /// <summary>
            /// Informuje widok o zmianie danych. Mechanizm bindowania danych.
            /// </summary>
            /// <param name="propertyName">Nazwa zmiennej, która uległa zmianie</param>
            protected virtual void OnPropertyChanged(string propertyName)
            {
                this.VerifyPropertyName(propertyName);
                PropertyChangedEventHandler handler = this.PropertyChanged;
                if (handler != null)
                {
                    var e = new PropertyChangedEventArgs(propertyName);
                    handler(this, e);
                }
            }

            [Conditional("DEBUG")]
            [DebuggerStepThrough]
            public void VerifyPropertyName(string propertyName)
            {
                if (TypeDescriptor.GetProperties(this)[propertyName] == null)
                {
                    string msg = "Invalid property name: " + propertyName;
                    throw new Exception(msg);
                    //else
                    //    Debug.Fail(msg);
                }
            }

        }
}
