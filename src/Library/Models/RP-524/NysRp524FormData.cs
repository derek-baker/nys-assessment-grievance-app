using Library.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Models
{
    /// <summary>
    /// Used as wrapper for data passed to server from client
    /// </summary>
    public class NysRp524FormData
    {
        public bool BuildingsRecentlyAlteredCheck { get; set; }
        public bool CheckAssessedValueHigherA { get; set; }
        public bool CheckAssessedValueHigherB { get; set; }
        public bool CheckAssessedValueHigherC { get; set; }
        public bool CheckAssessedValueHigherD { get; set; }
        public bool CheckEqualizationA { get; set; }
        public bool CheckPurchasePrice { get; set; }
        public bool CommercialCheck { get; set; }
        public string DateOfPurchase { get; set; }
        public string DayPhone { get; set; }
        public string Email { get; set; }
        public string EveningPhone { get; set; }
        public bool FarmCheck { get; set; }
        public bool IndustrialCheck { get; set; }
        public string LandVal { get; set; }
        public string LocationCityTown { get; set; }
        public string LocationCounty { get; set; }
        public string LocationStreetAddress { get; set; }
        public string LocationVillage { get; set; }
        public string MarketValueEstimate { get; set; }
        public string Muni { get; set; }
        public bool OtherCheck { get; set; }
        public string OwnerAddressLine1 { get; set; }
        public string OwnerAddressLine2 { get; set; }
        public string OwnerAddressLine3 { get; set; }
        public string OwnerNameLine1 { get; set; }
        public string OwnerNameLine2 { get; set; }
        public string PersonalProperty { get; set; }
        public bool PropertyDescCheck { get; set; }
        public string PropertyDescText { get; set; }
        public string PropertyDescription { get; set; }
        public bool PropertyIsIncomeProducingCheck { get; set; }
        public bool PropertyOfferedForSaleCheck { get; set; }
        public string PropertyOfferedForSaleAskingPrice { get; set; }
        public string PropertyOfferedForSaleDate { get; set; }
        public string PropertyOfferedForSaleHow { get; set; }
        public bool PropertyRecentlyAppraisedCheck { get; set; }
        public string PropertyRecentlyAppraisedAppraiser { get; set; }
        public string PropertyRecentlyAppraisedDate { get; set; }
        public string PropertyRecentlyAppraisedPurpose { get; set; }
        public string PropertyRecentlyAppraisedValue { get; set; }
        public string RelationshipBetweenParties { get; set; }
        public string RemodelledCost { get; set; }
        public string RemodelledDateEnd { get; set; }
        public string RemodelledDateStart { get; set; }
        public string RepInfo { get; set; }
        public string RepInfoComplete { get; set; }
        public bool ResidenceCheck { get; set; }
        public string SchoolDistrict { get; set; }
        public string TaxMapNum { get; set; }
        public bool TermsCashCheck { get; set; }
        public bool TermsContractCheck { get; set; }
        public bool TermsOtherCheck { get; set; }
        public string TextComplainantBelieves { get; set; }
        public string TotalVal { get; set; }
        public string TwoCharAssessmentYear { get; set; }
        public string Two_One_PurchasePrice { get; set; }
        public bool Two_Seven_Check { get; set; }
        public bool VacantCheck { get; set; }
        public string five_date_text { get; set; }
        public string five_signature_type { get; set; }
        public string five_signature_name { get; set; }
        public string four_2_text { get; set; }
        public string four_date_text { get; set; }
        public string four_four_text { get; set; }
        public string four_one_text { get; set; }
        public string four_signature_name { get; set; }
        public string four_three_text { get; set; }
        public string p3_2_a_pct { get; set; }
        public string p3_2_b_pct { get; set; }
        public string p3_2_c_pct { get; set; }
        public string six_date_text { get; set; }
        public string six_one_text { get; set; }
        public string six_signature_name { get; set; }
        public bool six_stipulation_check { get; set; }
        public string six_three_text { get; set; }
        public string six_two_text { get; set; }
        public string three_4_text { get; set; }
        public string three_b_1_a_text { get; set; }
        public string three_b_1_b_text { get; set; }
        public bool three_b_1_check { get; set; }
        public string three_b_2_a_text { get; set; }
        public string three_b_2_b_text { get; set; }
        public string three_b_2_c_text { get; set; }
        public bool three_b_2_check { get; set; }
        public bool three_b_3_check { get; set; }
        public string three_b_3_a_text { get; set; }
        public string three_b_3_b_text { get; set; }
        public bool three_c_1_check { get; set; }
        public bool three_c_2_check { get; set; }
        public bool three_c_3_check { get; set; }
        public bool three_c_4_check { get; set; }
        public bool three_c_5_check { get; set; }
        public bool three_d_0_check { get; set; }
        public string three_d_0_text { get; set; }
        public bool three_d_1_check { get; set; }
        public string three_d_1_text { get; set; }
        public bool three_d_2_check { get; set; }
        public string three_d_2_text { get; set; }
        public string three_d_homestead_allocation_text { get; set; }
        public string three_d_homestead_claimed_text { get; set; }
        public string three_d_nonhomestead_allocation_text { get; set; }
        public string three_d_nonhomestead_claimed_text { get; set; }
        public string three_three_text { get; set; }

        public ValidationResult ValidateInstance()
        {
            var requiredProps = new List<string>() {
                "TwoCharAssessmentYear",
                "Muni",
                "OwnerNameLine1",
                "OwnerAddressLine1",
                "Email",
                "RepInfo",
                "LocationStreetAddress",
                "LocationCityTown",
                "LocationCounty",
                "SchoolDistrict",
                "TaxMapNum",
                "LandVal",
                "TotalVal",
                "MarketValueEstimate"
            };
            PropertyInfo[] properties = typeof(NysRp524FormData).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                // The object whose props we are examining is of a type that has only bools and strings as props.
                // This means we can assume if propValue is null, we're inspecting a string.
                var propValueSafe = property.GetValue(this) ?? "";
                Type actualType = (propValueSafe).GetType();
                Type expectedType = typeof(string);

                if (
                    // Only validate string fields
                    actualType == expectedType
                    &&
                    // It's a required prop
                    requiredProps.Contains(property.Name)
                    &&
                    // 
                    !isValid(propValueSafe.ToString())
                )
                {                    
                    return new ValidationResult(
                        isValid: false,
                        message: $"The value for property {property.Name} is invalid. Value: {propValueSafe}"
                    );
                }
            }            
            return new ValidationResult(true, "");

            static bool isValid(string propVal) { return !string.IsNullOrEmpty(propVal); }            
        }

        public string ComputeComplaintType()
        {
            if (
                CheckAssessedValueHigherA
                ||
                CheckAssessedValueHigherB
            )
            {
                return "A.UNEQUAL ASSESSMENT";
            }
            else if (
                three_b_1_check 
                ||
                three_b_2_check
                ||
                three_b_3_check
            )
            {
                return "B. EXCESSIVE ASSESSMENT";
            }
            else if (
                three_c_1_check
                ||
                three_c_2_check
                ||
                three_c_3_check
                ||
                three_c_4_check
                ||
                three_c_5_check
            )
            {
                return "C.UNLAWFUL ASSESSMENT";
            }
            else if (
                three_d_0_check
                ||
                three_d_1_check
                ||
                three_d_2_check
            )
            {
                return "D.MISCLASSIFICATION";
            }
            else
            {
                return "UNKNOWN";
            }

        }
    }
}
