namespace Library.Models
{
    public class PdfImage
    {
        public int PageNumber { get; } 
        public float Height { get; }
        public int FloatLeft { get; } 
        public int FloatBottom { get; } 

        public PdfImage(
            int pageNum, 
            float imageHeight,
            int floatLeft, 
            int floatBottom
        )
        {
            PageNumber = pageNum;
            Height = imageHeight;
            FloatLeft = floatLeft;
            FloatBottom = floatBottom;
        }
    }
}
