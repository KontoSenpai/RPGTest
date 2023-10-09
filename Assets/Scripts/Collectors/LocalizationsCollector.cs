using System.Linq;
using RPGTest.Models;
using RPGTest.Modules.Localization;
using RPGTest.Modules.Localization.Models;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class LocalizationCollectors : ICollector
    {
        static public string LocalizationsAsset = ((TextAsset)Resources.Load("Configs/Localizations")).text;

        private static LocalizationsBank _bank;
        public static LocalizationsBank Bank => _bank ??= Collect<LocalizationsBank>(LocalizationsAsset);

        /// <summary>
        /// Try to get a localized string for the desired language
        /// </summary>
        /// <param name="ID">Line ID to find</param>
        /// <param name="localizedText">translated value</param>
        /// <returns>True if the line is found for desired language, false otherwise</returns>
        public static bool TryGetLocalizedLine(string ID, out string localizedText)
        {
            localizedText = string.Empty;
            var language = SaveFileModel.Instance?.Language ?? Enums.Language.EN;

            if (Bank != null && Bank.Lines != null && TryGetLine(ID, out LocalizationLine line))
            {
                if (line.LocalizedValues.TryGetValue(language, out localizedText)) {
                    return true;
                }
                return false;
            }
            return false;
        }

        public static bool TryGetLine(string ID, out LocalizationLine line)
        {
            line = null;
            if (Bank != null && Bank.Lines != null)
            {
                line = Bank.Lines.SingleOrDefault(x => x.Id == ID);
                return line != null;
            }
            return false;
        }
    }
}

