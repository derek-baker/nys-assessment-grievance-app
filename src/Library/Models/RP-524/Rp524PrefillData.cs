namespace Library.Models
{
    public class Rp524PrefillData
    {
        public int TwoCharAssessmentYear { get; set; }
        public string Muni { get; set; }
        public string OwnerNameLine1 { get; set; }
        public string OwnerNameLine2 { get; set; }
        public string OwnerAddressLine1 { get; set; }
        public string OwnerAddressLine2 { get; set; }
        public string LocationStreetAddress { get; set; }
        public string LocationCityTown { get; set; }
        public string LocationVillage { get; set; }
        public string LocationCounty { get; set; }
        public string SchoolDistrict { get; set; }
        public string TaxMapNum { get; set; }
        public bool ResidenceCheck { get; set; }
        public bool CommercialCheck { get; set; }
        public bool FarmCheck { get; set; }
        public bool IndustrialCheck { get; set; }
        public bool VacantCheck { get; set; }
        public string PropertyDescription { get; set; }
        public int LandVal { get; set; }
        public int TotalVal { get; set; }
    }
}
