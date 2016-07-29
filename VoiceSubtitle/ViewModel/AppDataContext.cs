using System;
using System.Collections.Generic;
using System.IO;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.ViewModel
{
    public class AppDataContext
    {
        public List<SourcePath> LoadCaptions()
        {
            var items = new List<SourcePath>();

            var filePaths = Directory.GetFiles(FolderManager.FolderCaptionPath, "*.cap");
            foreach (var filepath in filePaths)
            {
                var source = ParseSource(filepath);
                if (source != null)
                    items.Add(source);
            }

            return items;
        }

        public SourcePath ParseSource(string filepath)
        {
            try
            {
                var lines = File.ReadAllText(filepath).Split('\n');
                var source = new SourcePath()
                {
                    Path = filepath,
                    VideoName = lines[0].Trim(),
                    Video = lines[1].Trim(),
                    PrimaryCaption = lines[2].Trim(),
                    TranslatedCaption = lines[3].Trim(),
                };

                return source;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}