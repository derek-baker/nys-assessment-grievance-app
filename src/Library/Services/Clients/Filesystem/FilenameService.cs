using Library.Models;
using Library.Services.Time;
using System;

namespace Library.Services.Filesystem
{
    public static class FilenameService
    {
        public static string GetPathToFillableRp524()
        {
            return System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "NYS_RP524_WithSignatureButtons_WithFillableBoardOnly.pdf"
            );
        }

        /// <summary>
        /// The format of the referenced file is important as it allows  
        /// us to prefill RP-525 data for online bar review.
        /// </summary>
        public static string CreateRp524AnswersJsonFilename(string grievanceId)
        {
            return $"{MagicStringsService.Rp524AnswersFileObjectPrefix}_{grievanceId}_{TimeService.GetFormattedDate(DateTime.Now)}.json";
        }

        public static string CreateFilename(string fileName, string extension = ".pdf")
        {
            return TempFileService.SanitizeFileName(fileName).Replace(
                extension,
                $"_{DateTime.Now:yyyy-dd-M_HH-mm-ss-FFF}{extension}"
            );
        }

        public static string CreateRp524FileName(NysRp524FormData formData)
        {
            string propType =
                (formData.ResidenceCheck == true) ? "RES"
                    : (formData.CommercialCheck == true) ? "COM"
                        : (formData.FarmCheck == true) ? "FARM"
                            : (formData.VacantCheck == true) ? "VACANT"
                                : (formData.IndustrialCheck == true)
                                    ? "INDUSTRIAL"
                                    : "UNKNOWN";
            
            string taxMapNum =
                formData.TaxMapNum.Trim();
            
            string streetAddress =
                formData.LocationStreetAddress.Trim().Replace(' ', '-');
            
            string formName =
                $"NYS_RP524_{taxMapNum}_{ streetAddress}_{propType}_{DateTime.Now:yyyy-dd-M_HH-mm-ss-FFF}.pdf";

            return formName;
        }
    }
}
