using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoiceSubtitle.Helper
{
    public class IniFile
    {
        private readonly string path;
        private Encoding encoding;

        public IniFile(string path)
        {
            this.path = path;
            encoding = Encoding.UTF8;
            Read();
        }

        public IniFile(string path, Encoding encoding)
        {
            this.path = path;
            this.encoding = encoding;
            Read();
        }

        private void Read()
        {
            dictData = new Dictionary<string, string>();

            if (!File.Exists(path))
                return;

            HasFile = true;            
            var lines = File.ReadAllLines(path, encoding);
            foreach (var line in lines)
            {
                var indexSpliter = line.IndexOf("=");
                string key = line.Substring(0, indexSpliter);
                string value = line.Substring(indexSpliter + 1, line.Length - indexSpliter - 1);
            }
        }

        public void Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var e in dictData)
            {
                sb.AppendLine($"{e.Key}={e.Value}");
            }
            File.WriteAllText(path,sb.ToString());
        }

        private Dictionary<string, string> dictData;

        public bool HasFile { get; private set; } = false;

        public string this[string key]
        {
            get
            {
                string value;
                if (dictData.TryGetValue(key, out value))
                    return value;

                return string.Empty;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(key))
                    return;

                if (dictData.ContainsKey(key))
                    dictData.Remove(key);

                dictData.Add(key, value);
            }
        }
    }
}