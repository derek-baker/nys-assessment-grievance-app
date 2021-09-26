using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Library.Services.Filesystem
{
    public static class TempFileService
    {
        public static string SanitizeFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(name, invalidRegStr, "_");
        }

        public static string GetFilePath(string filename)
        {
            // NOTE: We need to attempt to eliminate the possibility of collisions on filenames.
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{DateTime.Now:yyyy-M-dd_HH-mm-ss-FFF}_{filename}");
            return tempFilePath;
        }

        public static string CreateFileOnDisk(
            string targetFilepath,
            IFormFile file
        )
        {
            Contract.Requires(file != null);

            using var memStream = new MemoryStream();
            file.CopyTo(memStream);
            byte[] fileBytes = memStream.ToArray();

            File.WriteAllBytes(
                targetFilepath,
                fileBytes
            );
            return targetFilepath;
        }

        public static string CopyFileToTemp(string sourcePath, string destinationPath)
        {
            File.Copy(sourcePath, destinationPath);

            return destinationPath;
        }

        /// <summary>
        /// Fire-and-forget cleanup.
        /// </summary>        
        public static void RemoveFile(string filePath)
        {
            var task = Task.Run(
                () =>
                {
                    File.Delete(filePath);

                    // TODO: Logging?
                }
            ).ConfigureAwait(false);            
        }

    }
}
