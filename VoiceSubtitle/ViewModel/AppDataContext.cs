using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.ViewModel
{
    public class AppDataContext
    {
        public List<SourcePath> LoadCaptions()
        {
            var items = new List<SourcePath>();
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\captions";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string[] filePaths = Directory.GetFiles(folder, "*.cap");
            foreach (var filepath in filePaths)
            {
                SourcePath source = ParseSource(filepath);

                items.Add(source);
            }

            return items;
        }

        public SourcePath ParseSource(string filepath)
        {
            var lines = File.ReadAllLines(filepath);
            var source = new SourcePath()
            {
                Path = filepath,
                VideoName = lines[0],
                Video = lines[1],
                PrimaryCaption = lines[2],
                TranslatedCaption = lines[3],
            };
            return source;
        }
    }
}