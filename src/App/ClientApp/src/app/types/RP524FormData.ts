export class RP524FormDataPreFill {
    public TwoCharAssessmentYear: number;
    public Muni: string;
    public OwnerNameLine1: string;
    public OwnerNameLine2: string;
    public OwnerAddressLine1: string;
    public OwnerAddressLine2: string;
    public LocationStreetAddress: string;
    public LocationCityTown: string;
    public LocationVillage: string;
    public LocationCounty: string;
    public SchoolDistrict: string;
    public TaxMapNum: string;
    public ResidenceCheck: boolean;
    public CommercialCheck: boolean;
    public FarmCheck: boolean;
    public IndustrialCheck: boolean;
    public VacantCheck: boolean;
    public OtherCheck: boolean;
    public PropertyDescription: string;
    public LandVal: number;
    public TotalVal: number;

    constructor() {
        this.TwoCharAssessmentYear = undefined;
        this.Muni = '';
        this.OwnerNameLine1 = '';
        this.OwnerNameLine2 = '';
        this.OwnerAddressLine1 = '';
        this.OwnerAddressLine2 = '';
        this.LocationStreetAddress = '';
        this.LocationCityTown = '';
        this.LocationVillage = '';
        this.LocationCounty = '';
        this.SchoolDistrict = '';
        this.TaxMapNum = '';
        this.ResidenceCheck = false;
        this.CommercialCheck = false;
        this.FarmCheck = false;
        this.IndustrialCheck = false;
        this.VacantCheck = false;
        this.OtherCheck = false;
        this.PropertyDescription = '';
        this.LandVal = undefined;
        this.TotalVal = undefined;
    }

    // public UpdateValues(data: RP524FormDataPreFill | any): void {
    //     this.TwoCharAssessmentYear = (data.TwoCharAssessmentYear) ? data.TwoCharAssessmentYear : undefined;
    //     this.Muni = (data.Muni) ? data.Muni : '';
    //     this.OwnerNameLine1 = (data.OwnerNameLine1) ? data.OwnerNameLine1 : '';
    //     this.OwnerNameLine2 = (data.OwnerNameLine2) ? data.OwnerNameLine2 : '';
    //     this.OwnerAddressLine1 = (data.OwnerAddressLine1) ? data.OwnerAddressLine1 : '';
    //     this.OwnerAddressLine2 = (data.OwnerAddressLine2) ? data.OwnerAddressLine2 : '';
    //     this.LocationStreetAddress = (data.LocationStreetAddress) ? data.LocationStreetAddress : '';
    //     this.LocationCityTown = (data.LocationCityTown) ? data.LocationCityTown : '';
    //     this.LocationVillage = (data.LocationVillage) ? data.LocationVillage : '';
    //     this.LocationCounty = (data.LocationCounty) ? data.LocationCounty : '';
    //     this.SchoolDistrict = (data.SchoolDistrict) ? data.SchoolDistrict : '';
    //     this.TaxMapNum = (data.TaxMapNum) ? data.TaxMapNum : '';
    //     this.ResidenceCheck = (data.ResidenceCheck) ? data.ResidenceCheck : false;
    //     this.CommercialCheck = (data.CommercialCheck) ? data.CommercialCheck : false;
    //     this.FarmCheck = (data.FarmCheck) ? data.FarmCheck : false;
    //     this.IndustrialCheck = (data.IndustrialCheck) ? data.IndustrialCheck : false;
    //     this.VacantCheck = (data.VacantCheck) ? data.VacantCheck : false;
    //     this.OtherCheck = (data.OtherCheck) ? data.OtherCheck : false;
    //     this.PropertyDescription = (data.PropertyDescription) ? data.PropertyDescription : '';
    //     this.LandVal = (data.LandVal) ? data.LandVal : undefined;
    //     this.TotalVal = (data.TotalVal) ? data.TotalVal : undefined;
    // }
}
