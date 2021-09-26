using iText.Forms;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using Library.Models;
using Library.Services.Filesystem;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace Library.Services.PDF
{
    public static class PdfFillService
    {
        const string NYSRP525 = "NYS_RP525_NO_SIGNATURE";

        public static readonly string FallbackSignatureImage = "iVBORw0KGgoAAAANSUhEUgAAApgAAAB+CAYAAACAs2F/AAACcklEQVR4nO3WQQkAMAzAwPo3vZoIDMqdgjwzDwAAQvM7AACAWwwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQMJgAAKYMJAEDKYAIAkDKYAACkDCYAACmDCQBAymACAJAymAAApAwmAAApgwkAQMpgAgCQMpgAAKQWAZVxC7qKyIQAAAAASUVORK5CYII=";

        private static void updateField(PdfAcroForm form, string fieldName, string fieldValue)
        {
            var textInput = form.GetField(fieldName);
            textInput.SetValue((fieldValue ?? ""));
        }

        public static string GetPathToBlankNysRp525()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                // TODO: Refactor to config or something
                "NYS_RP525.pdf"
            );
        }

        /// <summary>
        /// Used during filling of NYS RP-525 after online bar review.
        /// The PDF used should not have the signature button.
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        public static string GetPathToFillableNysRp525Form(string pdfName = NYSRP525)
        {
            var fillablePdfPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                $"{pdfName}.pdf"
            );
            return fillablePdfPath;
        }

        /// <summary>
        /// Used during prefill of offline NYS RP-525
        /// The PDF used should have the signauture button
        /// </summary>
        /// <returns></returns>
        public static string GetPathForTempNysRp525()
        {
            return Path.Combine(
                Path.GetTempPath(),
                $"{MagicStringsService.NysRp525StorageObjectPrefix}_{DateTime.Now:yyyy-M-dd-HH-mm-FFF}.pdf"                
            );
        }

        public static void PrefillRp525(
            string blankPdfToFillPath,
            string outputPdfPath,
            NysRp525PrefillData data
        )
        {
            Contract.Requires(data != null);

            // Clean '$' from all string props in data.
            PropertyInfo[] properties = typeof(NysRp525PrefillData).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var propValue = property.GetValue(data);
                // INTENT: If the value is null, we can pretend it's a bool 
                // (because if it was set it would be) so we don't try to clean it.
                // Also, We don't need to clean it if null.
                Type actualType = (propValue ?? false).GetType();
                Type expectedType = typeof(string);
                if (actualType == expectedType)
                {
                    string cleanPropVal = propValue.ToString().Replace("$", "");
                    property.SetValue(data, cleanPropVal);
                }
            }

            using var reader = new PdfReader(blankPdfToFillPath);
            using var writer = new PdfWriter(outputPdfPath);
            using var newPdf = new PdfDocument(reader, writer);
            
            PdfAcroForm form = PdfAcroForm.GetAcroForm(newPdf, true);
            //var fields = form.GetFormFields();

            updateField(
                form,
                "City, town, village or county", data.Muni
            );

            var possibleCarriageReturn =
                ((data.OwnerNameLine2 ?? "").Length > 0)
                    ? "\n" : "";
            var ownerNameAndAddress =
                $"{data.OwnerNameLine1} {possibleCarriageReturn}{data.OwnerNameLine2} \n" +
                $"{data.OwnerAddressLine1} \n" +
                $"{data.OwnerAddressLine2} {possibleCarriageReturn}{data.OwnerAddressLine3}";

            updateField(
                form, 
                "Name and address of complainant", 
                ownerNameAndAddress    
            );
            updateField(
                form,
                "Tax Map Section/Block/Lot #",
                data.TaxMapNum
            );
            updateField(
                form, 
                "Location of property if different than address of Complainant1", 
                $"{data.LocationStreetAddress} {data.LocationVillage}"
            );
            updateField(
                form, 
                "Location of property if different than address of Complainant2",
                $"{data.LocationCityTown} {data.LocationCounty}"
            );
            updateField(
                form,
                "Land",
                "N\\A"
            );
            updateField(
                form,
                "Tentative assessed value",
                AddCommasToCurrencyString(data.TotalVal).Substring(1).Replace(".00", "")
            );
            updateField(
                form, 
                "Date",
                DateTime.Now.ToString("M/d/yyyy")
            );
            newPdf.Close();

            // TODO: Refactor and unit-test
            static string AddCommasToCurrencyString(string amountStr)
            {
                bool isSuccessful = decimal.TryParse(
                    amountStr,
                    out decimal amountDec
                );
                if (isSuccessful)
                {
                    return string.Format("{0:C}", amountDec);
                }
                return "NaN";
            }
        }

        /// <summary>
        /// Modify temp PDF and save in outputPdfPath
        /// </summary>        
        private static void addSignature(
            PdfImage pdfImage,            
            string outputPdfPath,
            string signatureImagePath
        )
        {
            // Setup for new file
            string tempFilePath = TempFileService.GetFilePath("524_Copy_For_Signature.pdf");

            TempFileService.CopyFileToTemp(outputPdfPath, tempFilePath);            
            
            using var reader = new PdfReader(tempFilePath);
            using var writer = new PdfWriter(outputPdfPath);
            using var pdfDocument = new PdfDocument(reader, writer);

            var document = new Document(pdfDocument);

            // Load image from disk
            ImageData imageData = ImageDataFactory.Create(signatureImagePath);
            
            var image = 
                new iText.Layout.Element.Image(imageData)
                    .SetHeight(height: pdfImage.Height)
                    //.SetAutoScaleWidth(autoScale: true)
                    .SetFixedPosition(
                        pdfImage.PageNumber, pdfImage.FloatLeft, pdfImage.FloatBottom
                    );
            // This adds the image to the page
            document.Add(image);

            // Don't forget to close the document.
            // When you use Document, you should close it rather than PdfDocument instance
            document.Close();

            TempFileService.RemoveFile(tempFilePath);
        }

        public static void FillRp524(
            string blankPdfToFillPath,
            string outputPdfPath,
            NysRp524FormData data,
            string signature4Path,
            string signature5Path
        )
        {
            Contract.Requires(data != null);
            using var reader = new PdfReader(blankPdfToFillPath);
            using var writer = new PdfWriter(outputPdfPath);
            using var newPdf = new PdfDocument(reader, writer);

            PdfAcroForm form = PdfAcroForm.GetAcroForm(newPdf, true);
            var fields = form.GetFormFields();

            updateField(form, "Year", data.TwoCharAssessmentYear);
            updateField(form, "City, Town, Village or County", data.Muni);
            updateField(form, "Name1", data.OwnerNameLine1);
            updateField(form, "Name2", data.OwnerNameLine2);
            updateField(form, "Mailing Address1", data.OwnerAddressLine1);
            updateField(form, "Mailing Address2", data.OwnerAddressLine2);
            updateField(form, "Mailing Address3", data.OwnerAddressLine3);
            updateField(form, "E-mail Address", data.Email);
            updateField(form, "Day Phone Number", data.DayPhone);
            updateField(form, "Evening Phone Number", data.EveningPhone);

            updateField(form, "Name, Address and telephone no. of representative of owner1", data.RepInfo);
            updateField(form, "Street Address", data.LocationStreetAddress);
            updateField(form, "Village", data.LocationVillage);
            updateField(form, "City/Town", data.LocationCityTown);
            updateField(form, "County", data.LocationCounty);
            updateField(form, "School District", data.SchoolDistrict);
            updateField(form, "Tax Map Number or Section/Block/Lot", data.TaxMapNum);

            if (data.ResidenceCheck == true)
            {
                updateField(form, "Type of Property1", "yes");
            }
            if (data.CommercialCheck == true)
            {
                updateField(form, "Type of Property2", "yes");
            }
            if (data.FarmCheck == true)
            {
                updateField(form, "Type of Property3", "yes");
            }
            if (data.IndustrialCheck == true)
            {
                updateField(form, "Type of Property4", "yes");
            }
            if (data.VacantCheck == true)
            {
                updateField(form, "Type of Property5", "yes");
            }
            if (data.OtherCheck == true)
            {
                updateField(form, "Type of Property6", "yes");
            }
            updateField(form, "Description1", data.PropertyDescription);
            updateField(form, "Land", data.LandVal);
            updateField(form, "Total", data.TotalVal);
            updateField(form, "Property owner's estimate of market value of property as of valuation date", data.MarketValueEstimate);

            // END PAGE 1


            if (data.CheckPurchasePrice == true)
            {
                updateField(form, "Number 1", "yes");
            }
            updateField(form, "Purchase price of property", data.Two_One_PurchasePrice);
            updateField(form, "Date of purchase", data.DateOfPurchase);
            if (data.TermsCashCheck == true)
            {
                updateField(form, "Terms1", "yes");
            }
            if (data.TermsContractCheck == true)
            {
                updateField(form, "Terms2", "yes");
            }
            if (data.TermsOtherCheck == true)
            {
                updateField(form, "Terms3", "yes");
            }
            updateField(form, "Relationshop between seller and purchaser", data.RelationshipBetweenParties);
            updateField(form, "Personal Property, if any, included in purchase price", data.PersonalProperty);
            if (data.PropertyOfferedForSaleCheck == true)
            {
                updateField(form, "Number 2", "yes");
            }
            updateField(form, "When and for how long", data.PropertyOfferedForSaleDate);
            updateField(form, "How offered", data.PropertyOfferedForSaleHow);
            updateField(form, "Asking Price", data.PropertyOfferedForSaleAskingPrice);
            if (data.PropertyRecentlyAppraisedCheck == true)
            {
                updateField(form, "Number 3", "yes");
            }
            updateField(form, "When", data.PropertyRecentlyAppraisedDate);
            updateField(form, "By Whom", data.PropertyRecentlyAppraisedAppraiser);
            updateField(form, "Purpose of appraisal", data.PropertyRecentlyAppraisedPurpose);
            updateField(form, "Appraised Value", data.PropertyRecentlyAppraisedValue);
            if (data.PropertyDescCheck == true)
            {
                updateField(form, "Number 4", "yes");
            }
            updateField(form, "Description of any buildings or improvements located on the property1", data.PropertyDescText);
            if (data.BuildingsRecentlyAlteredCheck == true)
            {
                updateField(form, "Number 5", "yes");
            }
            updateField(form, "Cost", data.RemodelledCost);
            updateField(form, "Date Started", data.RemodelledDateStart);
            updateField(form, "Date Completed", data.RemodelledDateEnd);
            if (data.PropertyIsIncomeProducingCheck == true)
            {
                updateField(form, "Number 6", "yes");
            }
            if (data.Two_Seven_Check == true)
            {
                updateField(form, "Number 7", "yes");
            }

            // END PAGE 2

            if (data.CheckAssessedValueHigherA == true)
            {
                updateField(form, "The assessment is unequal for the following reason-a", "yes");
            }
            if (data.CheckAssessedValueHigherB == true)
            {
                updateField(form, "The assessment is unequal for the following reason-b", "yes");
            }

            updateField(form, "Percent1", data.TextComplainantBelieves);
            if (data.CheckEqualizationA == true)
            {
                updateField(form, "The latest state equalization rate for the city, town or village", "yes");
            }
            updateField(form, "Percent2", data.p3_2_a_pct);

            if (data.CheckAssessedValueHigherB == true)
            {
                updateField(form, "The latest residential assessment ratio", "yes");
            }
            updateField(form, "Percent3", data.p3_2_b_pct);

            if (data.CheckAssessedValueHigherC == true)
            {
                updateField(form, "Statement of the assessor or other local official", "yes");
            }
            updateField(form, "Percent4", data.p3_2_c_pct);

            if (data.CheckAssessedValueHigherD == true)
            {
                updateField(form, "Other explain", "yes");
            }

            updateField(form, "Value of property from Part one #7", data.three_three_text);
            updateField(form, "Complaint believes the assessment should be reduced to", data.three_4_text);

            if (data.three_b_1_check == true)
            {
                updateField(form, "The assessed value exceeds the full value of the property", "yes");
            }
            updateField(form, "Assessed value of property", data.three_b_1_a_text);
            updateField(form, "Complainant believes that assessment should be reduced to full value", data.three_b_1_b_text);

            if (data.three_b_2_check == true)
            {
                updateField(form, "The taxable assessed value is excessive because of the denial of all or portion of a partial exemption", "yes");
            }
            updateField(form, "Specify exemption", data.three_b_2_a_text);
            updateField(form, "Amount of exemption claimed", data.three_b_2_b_text);
            updateField(form, "Amount granted, if any", data.three_b_2_c_text);

            if (data.three_b_1_check == true)
            {
                updateField(form, "Improper calculation of transition assessment", "yes");
            }
            updateField(form, "Transition assessment", data.three_b_3_a_text);
            updateField(form, "Transition assessment claimed", data.three_b_3_b_text);


            if (data.three_c_1_check == true)
            {
                updateField(form, "Property is wholly exempt1", "yes");
            }
            if (data.three_c_2_check == true)
            {
                updateField(form, "Property is entirely outside the boundaries of the city, town, village, school district", "yes");
            }
            if (data.three_c_3_check == true)
            {
                updateField(form, "Property has been assessed and entered on the assessment roll", "yes");
            }
            if (data.three_c_4_check == true)
            {
                updateField(form, "Property cannot be identified from description or tax map number on the assessment roll", "yes");
            }
            if (data.three_c_5_check == true)
            {
                updateField(form, "Property is special franchise propert, the assessment of which exceeds the final assessment", "yes");
            }
            // END 3.C

            if (data.three_d_0_check == true)
            {
                updateField(form, "The property is misclassified for the following reason1", "yes");
            }
            if (data.three_d_1_check == true)
            {
                updateField(form, "The property is misclassified for the following reason2", "yes");
            }
            if (data.three_d_2_check == true)
            {
                updateField(form, "The property is misclassified for the following reason3", "yes");
            }
            updateField(form, "Class designation on the assessment roll", data.three_d_0_text);
            updateField(form, "Complainant believes class designation should be", data.three_d_1_text);

            updateField(form, "Homestead", data.three_d_homestead_allocation_text);
            updateField(form, "Homestead Claimed Allocation", data.three_d_homestead_claimed_text);
            updateField(form, "Non-Homestead", data.three_d_nonhomestead_allocation_text);
            updateField(form, "Non-Homestead Claimed Allocation", data.three_d_nonhomestead_claimed_text);

            // END PAGE 3

            updateField(form, "Name", data.four_one_text);
            updateField(form, "Desigante", data.four_2_text);
            updateField(form, "City/Town/Village County of", data.four_three_text);
            updateField(form, "Year Tentative Assessment Roll", data.four_four_text);
            updateField(form, "Date1", data.four_date_text);
            updateField(form, "Date2", data.five_date_text);

            newPdf.Close();

            float imageHeight = 20;
            int floatLeft = 375;
            addSignature(
                pdfImage: new PdfImage(pageNum: 4, imageHeight: imageHeight, floatLeft: floatLeft, floatBottom: 638),
                outputPdfPath: outputPdfPath,
                signatureImagePath: signature4Path // <== The existence/lifetime of this file is governed by calling code.
            );
            addSignature(
                pdfImage: new PdfImage(pageNum: 4, imageHeight: imageHeight, floatLeft: floatLeft, floatBottom: 525),
                outputPdfPath: outputPdfPath,
                signatureImagePath: signature5Path // <== The existence/lifetime of this file is governed by calling code.
            );
        }

        public static string FillNysRp525(
            string blankPdfToFillPath,
            string outputPdfPath,
            NysRps525OnlineFormData data,            
            string signaturePath
        )
        {
            Contract.Requires(data != null);
            using var reader = new PdfReader(blankPdfToFillPath);
            using var writer = new PdfWriter(outputPdfPath);
            using var newPdf = new PdfDocument(reader, writer);

            PdfAcroForm form = PdfAcroForm.GetAcroForm(newPdf, true);
            var fields = form.GetFormFields();

            static string convertCheckboxDataForFill(string value)
            {
                return (value == "true") ? "yes" : "";
            }

            updateField(form, "City, town, village or county", data.Admin_Rp525_Muni);
            updateField(form, "Tax Map Section/Block/Lot #", data.Admin_Rp525_TaxMapId);
            updateField(form, "Name and address of complainant", data.Admin_Rp525_ComplainantInfoTextArea);
            updateField(form, "Location of property if different than address of Complainant1", data.Admin_Rp525_Location1);
            updateField(form, "Location of property if different than address of Complainant2", data.Admin_Rp525_Location2);
            updateField(form, "Tentative assessed value", data.Admin_Rp525_Tentative);
            if (convertCheckboxDataForFill(data.Admin_Rp525_Check2a).Length > 0)
            {
                updateField(form, "Has been reduced to an assessed value of land", data.Admin_Rp525_Check2a);
            }
            updateField(form, "Land", data.Admin_Rp525_Land);
            updateField(form, "Total", data.Admin_Rp525_Total);
            if (convertCheckboxDataForFill(data.Admin_Rp525_Check2ai).Length > 0)
            {
                updateField(form, "Assessment has been reduced to amount claimed", data.Admin_Rp525_Check2ai);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_Check2b).Length > 0)
            {
                updateField(form, "Has not been reduced", data.Admin_Rp525_Check2b);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_AssessedCheck).Length > 0)
            {
                updateField(form, "Assessed Valuation", data.Admin_Rp525_AssessedCheck);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_ExemptionCheck).Length > 0)
            {
                updateField(form, "Exemption", data.Admin_Rp525_ExemptionCheck);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_ClassificationCheck).Length > 0)
            {
                updateField(form, "Classification", data.Admin_Rp525_ClassificationCheck);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_OtherCheck).Length > 0)
            {
                updateField(form, "Other", data.Admin_Rp525_OtherCheck);
            }
            updateField(form, "Current full market value of your property", data.Admin_Rp525_3a);
            if (convertCheckboxDataForFill(data.Admin_Rp525_3a1).Length > 0)
            {
                updateField(form, "The proof of value you presented was adequate to support reduction granted", data.Admin_Rp525_3a1);
            }

            if (convertCheckboxDataForFill(data.Admin_Rp525_3a2).Length > 0)
            {
                updateField(form, "The proof of value you presented was inadequate", data.Admin_Rp525_3a2);
            }
            updateField(form, "Inadequate because", data.Admin_Rp525_3a2text);
            if (convertCheckboxDataForFill(data.Admin_Rp525_3a2i).Length > 0)
            {
                updateField(form, "The supporting data was insufficient", data.Admin_Rp525_3a2i);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3a2ii).Length > 0)
            {
                updateField(form, "Sales were not comparable to your property", data.Admin_Rp525_3a2ii);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3a2iii).Length > 0)
            {
                updateField(form, "The wirtten appraisal was incomplete", data.Admin_Rp525_3a2iii);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3a2iv).Length > 0)
            {
                updateField(form, "The income and expense statement was incomplete", data.Admin_Rp525_3a2iv);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3a2v).Length > 0)
            {
                updateField(form, "The construction cost details were incomplete", data.Admin_Rp525_3a2v);
            }

            if (convertCheckboxDataForFill(data.Admin_Rp525_3b).Length > 0)
            {
                updateField(form, "Uniform percentage of value applicable in this assessing unit", data.Admin_Rp525_3b);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3b1).Length > 0)
            {
                updateField(form, "The proof of assessment ratio that you presented was adequate to support reduction granted", data.Admin_Rp525_3b1);
            }

            if (convertCheckboxDataForFill(data.Admin_Rp525_3b2).Length > 0)
            {
                updateField(form, "The proof of assessment ratio that you presented was inadequate", data.Admin_Rp525_3b2);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3b2i).Length > 0)
            {
                updateField(form, "Insufficient evidence was used in calculaitng an assessment ratio", data.Admin_Rp525_3b2i);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3b2ii).Length > 0)
            {
                updateField(form, "Sufficient evidence was presented by the assessor to refute the residential Assessment Ratio or Equalization rate", data.Admin_Rp525_3b2ii);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3b2iii).Length > 0)
            {
                updateField(form, "The State ratios are inapplicable due to revaluation", data.Admin_Rp525_3b2iii);
            }

            if (convertCheckboxDataForFill(data.Admin_Rp525_3b2iv).Length > 0)
            {
                updateField(form, "The ratio that you presented was not the correct RARThe construction cost details were incomplete", data.Admin_Rp525_3b2iv);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3b2v).Length > 0)
            {
                updateField(form, "The rate that you presented was not the correct State Equalization Rate", data.Admin_Rp525_3b2v);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3c1).Length > 0)
            {
                updateField(form, "Correct1", data.Admin_Rp525_3c1);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_3c2).Length > 0)
            {
                updateField(form, "Incorrect1", data.Admin_Rp525_3c2);
            }

            updateField(form, "The correct inventory should indicate the following1", data.Admin_Rp525_4_1);
            updateField(form, "The correct inventory should indicate the following2", data.Admin_Rp525_4_2);
            updateField(form, "The correct inventory should indicate the following3", data.Admin_Rp525_4_3);
            updateField(form, "The correct inventory should indicate the following4", data.Admin_Rp525_4_4);

            updateField(form, "Taxable assessed value was determined to be", data.Admin_Rp525_5);
            if (convertCheckboxDataForFill(data.Admin_Rp525_5_1_Check).Length > 0)
            {
                updateField(form, "Your request for exemption has been granted", data.Admin_Rp525_5_1_Check);
            }
            updateField(form, "Your request for exemption has been grated in the amount of", data.Admin_Rp525_5_1_Text);
            if (convertCheckboxDataForFill(data.Admin_Rp525_5_2_Check).Length > 0)
            {
                updateField(form, "Your request for exemption has been denied", data.Admin_Rp525_5_2_Check);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_6a1).Length > 0)
            {
                updateField(form, "Correct2", data.Admin_Rp525_6a1);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_6a2).Length > 0)
            {
                updateField(form, "Incorrect2", data.Admin_Rp525_6a2);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_6a2i).Length > 0)
            {
                updateField(form, "The class designation should be homestead", data.Admin_Rp525_6a2i);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp5256a2ii).Length > 0)
            {
                updateField(form, "Incorrect", data.Admin_Rp5256a2ii);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_6b1).Length > 0)
            {
                updateField(form, "Correct3", data.Admin_Rp525_6b1);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_6b2).Length > 0)
            {
                updateField(form, "Incorrect3", data.Admin_Rp525_6b2);
            }
            updateField(form, "Class designation should be allocated homestead in the amount of", data.Admin_Rp525_6b2i);
            updateField(form, "Non-homestead in the amount of", data.Admin_Rp525_6b2ii);
            if (convertCheckboxDataForFill(data.Admin_Rp525_7).Length > 0)
            {
                updateField(form, "Complaint has been dismissed because of willfull neglect or refusal to attend board hearing", data.Admin_Rp525_7);
            }
            updateField(form, "Factors in addition to or other than those listed that affected the determination1", data.Admin_Rp525_8_1);
            updateField(form, "Factors in addition to or other than those listed that affected the determination2", data.Admin_Rp525_8_2);
            updateField(form, "Factors in addition to or other than those listed that affected the determination3", data.Admin_Rp525_8_3);
            updateField(form, "Factors in addition to or other than those listed that affected the determination4", data.Admin_Rp525_8_4);
            //updateField(form, "Clear Form", data.);
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_All).Length > 0)
            {
                updateField(form, "All concur", data.Admin_Rp525_9_All);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_NotAll).Length > 0)
            {
                updateField(form, "All concur except2", data.Admin_Rp525_9_NotAll);
            }
            updateField(form, "Name1", data.Admin_Rp525_9_Name1);
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_Against1).Length > 0)
            {
                updateField(form, "Against1", data.Admin_Rp525_9_Against1);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_Abstain1).Length > 0)
            {
                updateField(form, "Abstain1", data.Admin_Rp525_9_Abstain1);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_Absent1).Length > 0)
            {
                updateField(form, "Absent1", data.Admin_Rp525_9_Absent1);
            }
            updateField(form, "Name2", data.Admin_Rp525_9_Name2);
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_Against2).Length > 0)
            {
                updateField(form, "Against2", data.Admin_Rp525_9_Against2);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_Abstain2).Length > 0)
            {
                updateField(form, "Abstain2", data.Admin_Rp525_9_Abstain2);
            }
            if (convertCheckboxDataForFill(data.Admin_Rp525_9_Absent2).Length > 0)
            {
                updateField(form, "Absent2", data.Admin_Rp525_9_Absent2);
            }
            updateField(form, "Date", data.Admin_Rp525_SignDate);

            form.FlattenFields();
            //updateField(form, "Button1", data.);
            newPdf.Close();
            //float imageHeight = 20;
            //int floatLeft = 210;            
            addSignature(
                pdfImage: new PdfImage(pageNum: 2, imageHeight: 20, floatLeft: 210, floatBottom: 58),
                outputPdfPath: outputPdfPath,
                signatureImagePath: signaturePath // <== The existence/lifetime of this file is governed by calling code.
            );
            return outputPdfPath;
        }
    }
}
