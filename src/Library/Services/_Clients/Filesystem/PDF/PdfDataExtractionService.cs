using iText.Forms;
using iText.Kernel.Pdf;
using Library.Models;
using Library.Models.RP_524;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace Library.Services.PDF
{
    public static class PdfDataExtractionService
    {
        /// <summary>
        /// Note that this will throw if the file being parsed is a PART of a 
        /// NYS RP-524 because it has the same metadata, but not the same inputs.
        /// </summary>        
        public static Rp524PdfParseResult TryParseRp524(string rp524Path)
        {
            var pdfMetaData = ReadPdfMetadata(pdfPath: rp524Path);
            var isNysRp524 = ValidatePdfDataAsNysRp524(pdfMetaData);
            if (!isNysRp524)
            {
                return new Rp524PdfParseResult(isParsed: false);
            }
            ImmutableDictionary<string, string> rp524Answers = ExtractDataFromPdf(pdfPath: rp524Path);

            NysRp525PrefillData rp525PrefillData = new NysRp525PrefillData();
            bool isParsed;
            string parseMsg;
            try
            {
                rp525PrefillData = ConvertNysRp524Data(rp524Answers);
                isParsed = true;
                parseMsg = "success";
            }
            catch (Exception e)
            {
                isParsed = false;
                parseMsg = e.Message;
                // TODO: Logging
            }            
            return new Rp524PdfParseResult(isParsed: isParsed, data: rp525PrefillData, parseMsg: parseMsg);
        }

        public static ImmutableDictionary<string, string> ReadPdfMetadata(string pdfPath)
        {
            Contract.Requires(pdfPath != null);
            if (!pdfPath.EndsWith("pdf"))
            {
                throw new Exception("File appears to not be a PDF");
            }

            using var reader = new PdfReader(pdfPath);
            using var pdf = new PdfDocument(reader);

            PdfDictionary infoDictionary = pdf.GetTrailer().GetAsDictionary(PdfName.Info);

            var pdfMetadata = new Dictionary<string, string>();
            try
            {
                // There have been some NYS PDFs that have "missing" metadata.
                foreach (PdfName key in infoDictionary.KeySet())
                {
                    pdfMetadata.Add(key.ToString(), infoDictionary.GetAsString(key).ToString());
                }
            }
            catch { /* TODO: logging? */ }
                
            return pdfMetadata.ToImmutableDictionary();
        }

        public static bool ValidatePdfDataAsNysRp524(ImmutableDictionary<string, string> pdfMetaData)
        {
            Contract.Requires(pdfMetaData != null);

            string companyKey = "/Company";
            string titleKey = "/Title";

            if (
                !pdfMetaData.ContainsKey(companyKey)
                ||
                !pdfMetaData.ContainsKey(titleKey)
                ||
                pdfMetaData[companyKey] != "NYS Office of Real Property Services"
                ||
                pdfMetaData[titleKey] != "Form RP-524:3/09:Complaint on Real Property Assessment Before the Board of Assessment Review:rp524"
            )
            {
                return false;
            }
            return true;
        }

        //public static bool ValidatePdfDataAsNysRp525(ImmutableDictionary<string, string> pdfMetaData)
        //{
        //    Contract.Requires(pdfMetaData != null);

        //    string companyKey = "/Company";
        //    string titleKey = "/Title";

        //    if (
        //        !pdfMetaData.ContainsKey(companyKey) 
        //        || 
        //        !pdfMetaData.ContainsKey(titleKey)
        //        ||
        //        pdfMetaData[companyKey] != "NYS Office of Real Property Services"
        //        ||
        //        pdfMetaData[titleKey] != "Form RP-525:9/04:Notice of Determination of Board of Assessment Review:rp525"
        //    )
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        public static ImmutableDictionary<string, string> ExtractDataFromPdf(string pdfPath)
        {
            using var reader = new PdfReader(pdfPath);
            using var newPdf = new PdfDocument(reader);

            PdfAcroForm form = PdfAcroForm.GetAcroForm(newPdf, true);
            var fields = form.GetFormFields();

            var dataFromForm = new Dictionary<string, string>();
            foreach (var key in fields.Keys)
            {
                dataFromForm.Add(key, form.GetField(key).GetValueAsString());
            }
            return dataFromForm.ToImmutableDictionary();
        }

        /// <summary>
        /// Map RP-524 fields to RP-525 fields for prefill purposes.
        /// </summary>
        public static NysRp525PrefillData ConvertNysRp524Data(ImmutableDictionary<string, string> nysRp524Data)
        {
            if (!nysRp524Data.TryGetValue("Total", out string total))
            {
                nysRp524Data.TryGetValue("Total$", out total);
            }
            var data = new NysRp525PrefillData
            {
                Muni = nysRp524Data["City, Town, Village or County"],
                OwnerNameLine1 = nysRp524Data["Name1"],
                OwnerNameLine2 = nysRp524Data["Name2"],
                OwnerAddressLine1 = nysRp524Data["Mailing Address1"],
                OwnerAddressLine2 = nysRp524Data["Mailing Address2"],
                //OwnerAddressLine3 = nysRp524Data["?"],
                TaxMapNum = nysRp524Data["Tax Map Number or Section/Block/Lot"],
                LocationStreetAddress = nysRp524Data["Street Address"],
                LocationVillage = nysRp524Data["Village"],
                LocationCityTown = nysRp524Data["City/Town"],
                LocationCounty = nysRp524Data["County"],
                //TotalVal = nysRp524Data["Total$"]
                //TotalVal = nysRp524Data["Total"]
                TotalVal = total
            };
            return data;
        }
    }
}
