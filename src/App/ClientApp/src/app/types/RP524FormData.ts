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
}
