using System;
using System.Collections.Generic;
using System.Text;

namespace LuaConverter.Model
{
    class Item
    {
        public int ID { get; set; }
        public bool IsEnglish { get; set; }
        public string UnidentifiedDisplayName { get; set; }
        public string UnidentifiedResourceName { get; set; }
        public string UnidentifiedDescriptionName { get; set; }
        public string IdentifiedDisplayName { get; set; }
        public string IdentifiedResourceName { get; set; }
        public string IdentifiedDescriptionName { get; set; }
        public string SlotCount { get; set; }
        public string ClassNum { get; set; }
    }
}
