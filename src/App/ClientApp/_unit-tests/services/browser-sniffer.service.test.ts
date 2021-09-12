import * as assert from 'assert';
import * as mocha from 'mocha';
import { BrowserSnifferService } from './../../src/app/services/browser-sniffer.service';
import * as jsdom from 'jsdom';
const { JSDOM } = jsdom;

mocha.describe('BrowserSnifferService', () => {

    mocha.describe('TestBrowserValidity', () => {
        mocha.it('should return true when window is from a valid browser', () => {
            // Arrange
            const expected = true;
            const { document } = (new JSDOM(`...`)).window;

            const sut = new BrowserSnifferService();
            // Act
            const actual = sut.TestBrowserValidity(document, { userAgent: ''});
            // Assert
            assert.strictEqual(actual, expected);
        });

        mocha.it('should return false when window is from an invalid browser', () => {
            // Arrange
            const expected = false;
            const { document } = (new JSDOM(`...`)).window;
            document.documentMode = 'IE';

            const sut = new BrowserSnifferService();
            // Act
            const actual = sut.TestBrowserValidity(document, { userAgent: ''});
            // Assert
            assert.strictEqual(actual, expected);
        });
    });
});
