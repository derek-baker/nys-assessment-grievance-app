namespace Library.Services.Image
{
    public interface IImageService
    {
        void ConvertBase64ToJpg(string base64Img, string savePath);
    }
}