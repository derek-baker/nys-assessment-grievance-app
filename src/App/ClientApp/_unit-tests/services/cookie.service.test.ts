import * as assert from 'assert';
import * as mocha from 'mocha';
import { CookieService } from '../../src/app/services/cookie.service';
import { Constants } from './../../src/app/types/enums/constants';

mocha.describe('CookieService', () => {

    mocha.describe('GetCookie', () => {
        mocha.it('should return expected value when there\'s one cookie', () => {
            // Arrange
            const cookieName = "Payload";
            const cookiesValue = '%7B%22TwoCharAssessmentYear%22%3A22%2C%22Muni%22%3A%22Somewhere%22%2C%22OwnerNameLine1%22%3A%22Some%20Person%22%2C%22OwnerNameLine2%22%3A%22Some%20Otherperson%22%2C%22OwnerAddressLine1%22%3A%22222%20Some%20St.%22%2C%22OwnerAddressLine2%22%3A%22Sometown%2C%20ZZ%2012345%22%2C%22LocationStreetAddress%22%3A%22222%20Some%20St.%22%2C%22LocationCityTown%22%3A%22Sometown%22%2C%22LocationVillage%22%3A%22Somevillage%22%2C%22LocationCounty%22%3A%22Somecounty%22%2C%22SchoolDistrict%22%3A%22Some%20District%22%2C%22TaxMapNum%22%3A%221-2-3.4%22%2C%22ResidenceCheck%22%3Atrue%2C%22CommercialCheck%22%3Afalse%2C%22FarmCheck%22%3Afalse%2C%22IndustrialCheck%22%3Afalse%2C%22VacantCheck%22%3Afalse%2C%22PropertyDescription%22%3Anull%2C%22LandVal%22%3A9980%2C%22TotalVal%22%3A9990%7D';
            const cookies = `${cookieName}=${cookiesValue}`;
            const sut = new CookieService();
            // Act
            const actual = sut.GetCookie(cookieName, cookies);
            // Assert
            assert.strictEqual(actual, cookiesValue);
        });
    });

    mocha.describe('InvalidateCookie', () => {
        mocha.it('should not throw', () => {
            // Arrange
            const cookieName = "Payload";
            const sut = new CookieService();
            // Act
            // Assert
            assert.doesNotThrow(
                () => {
                    sut.InvalidateCookie(
                        cookieName,
                        '',
                        0,
                        // @ts-ignore
                        { cookie: ''}
                    );
                }
            );
        });
    });
});
