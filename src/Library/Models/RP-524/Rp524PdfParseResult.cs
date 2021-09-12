namespace Library.Models.RP_524
{
    public class Rp524PdfParseResult
    {
        public bool IsParsedSuccessfully { get; }
        public string ParseMessage { get; }
        public NysRp525PrefillData ExtractedData { get; }

        public Rp524PdfParseResult(bool isParsed)
        {
            IsParsedSuccessfully = isParsed;
            ExtractedData = null;
            ParseMessage = "";
        }
        public Rp524PdfParseResult(
            bool isParsed, 
            NysRp525PrefillData data,
            string parseMsg
        )
        {
            IsParsedSuccessfully = isParsed;
            ExtractedData = data;
            ParseMessage = parseMsg;
        }
    }
}
