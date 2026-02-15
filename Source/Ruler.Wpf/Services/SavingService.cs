using Newtonsoft.Json;

using Ruler.Wpf.Models;
using Ruler.Wpf.Properties;
using Ruler.Wpf.Services.Persistence.Strategy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    public class SavingService : IPersistenceStrategy
    {
        // A mapping dictionary makes the code "Open" for extension without large if/else blocks
        private readonly Dictionary<Type, string> _typeToSettingKey = new Dictionary<Type, string>
        {
            { typeof(RulerInfo), "RulerSettingsJson" },
            { typeof(MonitorProfile), "MonitorProfilesJson" }
        };

        public void Save<T>(List<T> objectToSave)
        {
            if (objectToSave == null || !objectToSave.Any()) return;

            Type type = typeof(T);
            if (!_typeToSettingKey.ContainsKey(type))
            {
                throw new NotSupportedException($"Type {type.Name} is not supported by SavingService.");
            }

            object dataToSerialize;

            // Specialized logic for RulerInfo
            if (type == typeof(RulerInfo))
            {
                dataToSerialize = objectToSave
                    .Cast<RulerInfo>()
                    .Where(x => x.SaveType != Enums.SaveTypes.none)
                    .Select(x => PrepareRulerForSave(x))
                    .ToList();
            }
            else
            {
                dataToSerialize = objectToSave;
            }

            string json = JsonConvert.SerializeObject(dataToSerialize);
            Settings.Default[_typeToSettingKey[type]] = json;
            Settings.Default.Save(); // Don't forget to persist the settings!
        }

        public List<T> Load<T>() // Removed string parameter, use the generic T
        {
            Type type = typeof(T);
            if (!_typeToSettingKey.TryGetValue(type, out string settingKey))
            {
                return new List<T>();
            }

            var savedValue = Settings.Default[settingKey];
            if (savedValue == null || string.IsNullOrEmpty(savedValue.ToString()))
            {
                return new List<T>();
            }

            return JsonConvert.DeserializeObject<List<T>>(savedValue.ToString()) ?? new List<T>();
        }

        /// <summary>
        /// Creates a stripped-down copy of the ruler based on its SaveType.
        /// Changed name from 'Sort' to 'PrepareRulerForSave' to reflect what it actually does.
        /// </summary>
        private RulerInfo PrepareRulerForSave(RulerInfo item)
        {
            if (item == null) return null;

            RulerInfo strippedCopy = RulerInfo.GetDefaultRulerInfo();

            // Ensure the SaveType carries over
            strippedCopy.SaveType = item.SaveType;

            switch (item.SaveType)
            {
                case Enums.SaveTypes.all:
                    RulerInfo.CopyInto(item, strippedCopy);
                    break;

                case Enums.SaveTypes.location:
                    strippedCopy.Left = item.Left;
                    strippedCopy.Top = item.Top;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;

                case Enums.SaveTypes.size:
                    strippedCopy.Width = item.Width;
                    strippedCopy.Height = item.Height;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;

                case Enums.SaveTypes.appearance:
                    strippedCopy.Opacity = item.Opacity;
                    strippedCopy.ShowToolTip = item.ShowToolTip;
                    strippedCopy.IsLocked = item.IsLocked;
                    strippedCopy.TopMost = item.TopMost;
                    break;
            }

            return strippedCopy;
        }
    }
}
