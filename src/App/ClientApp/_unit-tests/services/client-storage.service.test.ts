import * as assert from 'assert';
import * as mocha from 'mocha';
import { ClientStorageService } from '../../src/app/services/client-storage.service';
import { IBrowserStorage } from 'src/app/types/IBrowserStorage';

mocha.describe('ClientStorageService', () => {
    const browserStorageMock: IBrowserStorage =  {
        setItem: () => {},
        getItem: () => '',
        removeItem: () => {},
        clear: () => {},
        length: 0,
        key: (index) => 'value'
    };

    mocha.describe('GetData', () => {
        mocha.it('returns expected', () => {
            // Arrange
            const value = 'value';
            // tslint:disable-next-line: object-literal-key-quotes
            const mockStorage = { 'mockKey': value, getItem: () => value };
            const sut = new ClientStorageService('dummy', 'FAKE');
            // @ts-ignore
            const actual = sut.GetData('mockKey', mockStorage);
            // Assert
            assert.strictEqual(actual, value);
        });
    });
});
