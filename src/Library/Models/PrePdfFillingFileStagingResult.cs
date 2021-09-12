namespace Library.Models
{
    public class PrePdfFillingFileStagingResult 
    {
        public string PathToReadFile { get; }
        public string PathToWriteFile { get; }

        public PrePdfFillingFileStagingResult(string pathToReadFile, string pathToWriteFile)
        {
            PathToReadFile = pathToReadFile;
            PathToWriteFile = pathToWriteFile;
        }
    }
}
