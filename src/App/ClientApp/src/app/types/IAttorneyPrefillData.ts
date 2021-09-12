export interface IAttorneyPrefillData {
    GroupNo: string;
    GroupName1: string;
    ContactName: string;
    Address1: string;
    Address2: string;
    City: string;
    State: string;
    ZipCode: string;
    Phone1: string;
    Phone2: string;
    FAX1: string;
    FAX2: string;
    Email: string;
}

export class AttorneyPrefillDataHeader implements IAttorneyPrefillData {
    public GroupNo = 'GroupNo';
    public GroupName1 = 'GroupName1';
    public ContactName = 'ContactName';
    public Address1 = 'Address1';
    public Address2 = 'Address2';
    public City = 'City';
    public State = 'State';
    public ZipCode = 'ZipCode';
    public Phone1 = 'Phone1';
    public Phone2 = 'Phone2';
    public FAX1 = 'FAX1';
    public FAX2 = 'FAX2';
    public Email = 'Email';
}
