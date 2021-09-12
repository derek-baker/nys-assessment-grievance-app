import * as assert from 'assert';
import * as mocha from 'mocha';
import { FormGroup, FormControl } from '@angular/forms';
import { FormDataService } from './../../src/app/services/form-data.service';

mocha.describe('FormDataService', () => {

    mocha.describe('InitFormRef()', () => {
        mocha.it('returns expected', () => {
            // Arrange
            const formGroup = new FormGroup({});
            const sut = new FormDataService();

            // Act / Assert
            assert.doesNotThrow(
                () => sut.InitFormRef(formGroup)
            );
        });
    });

    mocha.describe('SetFormValues()', () => {
        mocha.it('returns expected', () => {
            // Arrange
            const data = {};
            const sut = new FormDataService();
            // Act / Assert
            assert.doesNotThrow(
                () => sut.SetFormValues(data)
            );
        });
    });

    mocha.describe('GetFormValue()', () => {
        mocha.it('returns expected', () => {
            // Arrange
            const expected = 'You read me!';
            const sut = new FormDataService();
            sut.InitFormRef(
                new FormGroup(
                    { ReadMe: new FormControl(expected) }
                )
            );
            // Act
            const actual = sut.GetFormValue('ReadMe');
            // Assert
            assert.strictEqual(actual, expected);
        });
    });

    mocha.describe('ExtractFormValues()', () => {
        mocha.it('returns expected', () => {
            // Arrange
            const value = 'You read me!';
            const expected = { ReadMe: value };

            const sut = new FormDataService();
            sut.InitFormRef(
                new FormGroup(
                    { ReadMe: new FormControl(value) }
                )
            );
            // Act
            const actual = sut.ExtractFormValues();
            // Assert
            assert.strictEqual(JSON.stringify(actual), JSON.stringify(expected));
        });
        mocha.it('returns expected when form contains check', () => {
            // Arrange
            const expected = { IsClickedCheck: false };

            const sut = new FormDataService();
            sut.InitFormRef(
                new FormGroup(
                    { IsClickedCheck: new FormControl() }
                )
            );
            // Act
            const actual = sut.ExtractFormValues();
            // Assert
            assert.strictEqual(JSON.stringify(actual), JSON.stringify(expected));
        });
    });

    mocha.describe('GetBase64ImageFromDataUrl()', () => {
        mocha.it('returns expected when input not malformed', () => {
            // Arrange
            const expected = 'iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==';
            const dataUrl = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==';
            const sut = new FormDataService();
            // Act
            const actual = sut.GetBase64ImageFromDataUrl(dataUrl);
            // Assert
            assert.strictEqual(actual, expected);
        });
        mocha.it('returns expected when input is malformed', () => {
            // Arrange
            const expected = '';
            const dataUrl = 'iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==';
            const sut = new FormDataService();
            // Act
            const actual = sut.GetBase64ImageFromDataUrl(dataUrl);
            // Assert
            assert.strictEqual(actual, expected);
        });
    });

});
