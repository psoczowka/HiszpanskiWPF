using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace HiszpanskiWpf
{
    public class DataLoader
    {
        public static LanguageData LoadData(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Nie znaleziono pliku JSON", filePath);

            string json = File.ReadAllText(filePath);
            // Zwracamy LanguageData
            return JsonConvert.DeserializeObject<LanguageData>(json);
        }
    }
}
