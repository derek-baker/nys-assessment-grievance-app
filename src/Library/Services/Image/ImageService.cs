using System;
using System.IO;
using System.IO.Abstractions;

namespace Library.Services.Image
{
    public class ImageService : IImageService
    {
        readonly IFileSystem _fileSystem;

        public ImageService(IFileSystem fs)
        {
            this._fileSystem = fs;
        }

        public ImageService() : this(
            fs: new FileSystem() // Use default implementation which calls System.IO
        )
        {
        }

        public void ConvertBase64ToJpg(string base64Img, string savePath)
        {
            var bytes = Convert.FromBase64String(base64Img);
            //using var imageFile = new FileStream(savePath, FileMode.Create);
            //imageFile.Write(bytes, 0, bytes.Length);
            //imageFile.Flush();
            using var imageFile = _fileSystem.FileStream.Create(savePath, FileMode.Create);
            imageFile.Write(bytes, 0, bytes.Length);
            imageFile.Flush();
            imageFile.Close();
        }
    }
}
