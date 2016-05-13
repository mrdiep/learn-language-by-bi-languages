using System.IO;
using System.Reflection;

namespace VoiceSubtitle.Model
{
    public static class FolderManager
    {
        public static string FolderRecordPath { get; private set; }
        public static string FolderTempDownloadCaptionPath { get; private set; }
        public static string AssemblyPath { get; private set; }
        public static string FolderCaptionPath { get; private set; }
        public static string FolderDictionaryPath { get; private set; }

        static FolderManager()
        {
            CreateFolder();
        }

        public static void CreateFolder()
        {
            AssemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            FolderRecordPath = AssemblyPath + $@"\records";
            if (!Directory.Exists(FolderRecordPath))
            {
                Directory.CreateDirectory(FolderRecordPath);
            }

            FolderCaptionPath = AssemblyPath + @"\captions";

            if (!Directory.Exists(FolderCaptionPath))
                Directory.CreateDirectory(FolderCaptionPath);

            FolderDictionaryPath = $@"{AssemblyPath}\Dict";

            FolderTempDownloadCaptionPath = AssemblyPath + @"\temp captions";
            if (!Directory.Exists(FolderTempDownloadCaptionPath))
                Directory.CreateDirectory(FolderTempDownloadCaptionPath);
        }
    }
}