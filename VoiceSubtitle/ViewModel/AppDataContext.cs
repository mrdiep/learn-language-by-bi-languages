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

            string[] filePaths = Directory.GetFiles(FolderManager.FolderCaptionPath, "*.cap");
            foreach (var filepath in filePaths)
            {
                SourcePath source = ParseSource(filepath);
                if (source != null)
                    items.Add(source);
            }

            return items;
        }

        public SourcePath ParseSource(string filepath)
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}