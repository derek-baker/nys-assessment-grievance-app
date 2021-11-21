import * as assert from 'assert';
import * as mocha from 'mocha';
import * as jsdom from 'jsdom';
import { FileDownloadService } from '../../src/app/services/file-download-service';

const { JSDOM } = jsdom;
const getSut = (buildBlobFunc?: () => any) => new FileDownloadService(buildBlobFunc);

mocha.describe(FileDownloadService.name, () => {

    mocha.describe(getSut().BuildCsv.name, () => {
        mocha.it('Does not throw', () => {
            // Arrange
            const sut = getSut(() => undefined);
            // Act
            // Assert
            assert.doesNotThrow(() => sut.BuildCsv([{ a: 1, b: 2 }, { a: 2, b: 2 }]));
        });
    });

    // mocha.describe(getSut().DownloadCsv.name, () => {
    //     mocha.it('Does not throw', () => {
    //         // Arrange
    //         const { document } = (new JSDOM(`...`)).window;

    //         const sut = getSut();
    //         const blob: Blob = {
    //             size: undefined,
    //             type: undefined,
    //             arrayBuffer: undefined,
    //             slice: undefined,
    //             stream: undefined,
    //             text: undefined
    //         };
    //         // Act
    //         // Assert
    //         assert.doesNotThrow(() => sut.DownloadCsv(
    //             blob,
    //             'file',
    //             document));
    //     });
    // });
});
