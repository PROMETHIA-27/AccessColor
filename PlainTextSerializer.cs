using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessColor
{
    public static class PlainTextSerialization
    {
        private readonly static String folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AccessColor\\";

        public delegate String? StringFormatter<T>(T value);
        public delegate T StringDeformatter<T>(String strValue);

        public static Boolean SaveFromDictionary<T, U>(Dictionary<T, U> data, String fileName, StringFormatter<T> keyFormatter, StringFormatter<U> valueFormatter, Boolean createMissingFile = true)
            where T : notnull
        {
            var filePath = folderPath + fileName + ".txt";

            var lines = new String[data.Count];

            if (!File.Exists(filePath))
            {
                if (createMissingFile)
                    _ = File.Create(filePath);
                else
                    return false;
            }

            var i = 0;
            foreach (KeyValuePair<T, U> pair in data)
            {
                lines[i] = $"{keyFormatter(pair.Key)} {valueFormatter(pair.Value)}";
                i++;
            }

            File.WriteAllLines(filePath, lines);

            return true;
        }

        public static Boolean LoadToDictionary<T, U>(String fileName, Dictionary<T, U> data, StringDeformatter<T> keyDeformatter, StringDeformatter<U> valueDeformatter, String? backupFilePath = null)
            where T : notnull
        {
            var filePath = folderPath + fileName + ".txt";

            if (!File.Exists(filePath))
            {
                if (backupFilePath is not null)
                    File.Copy(backupFilePath, filePath);
                else
                    return false;
            }

            var lines = File.ReadAllLines(filePath);

            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ');

                if (parts.Length < 2)
                    continue;

                data[keyDeformatter(parts[0])] = valueDeformatter(parts[1]);
            }

            return true;
        }
    }
}
