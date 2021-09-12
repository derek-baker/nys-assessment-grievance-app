namespace Library.Models
{
    public class NysRp525PrefillData
    {
        public string Muni { get; set; }

        public string OwnerNameLine1 { get; set; }
        public string OwnerNameLine2 { get; set; }
        public string OwnerAddressLine1 { get; set; }
        public string OwnerAddressLine2 { get; set; }
        public string OwnerAddressLine3 { get; set; }

        public string TaxMapNum { get; set; }

        public string LocationStreetAddress { get; set; }
        public string LocationVillage { get; set; }

        public string LocationCityTown { get; set; }
        public string LocationCounty { get; set; }

        public string TotalVal { get; set; }
    }
}
