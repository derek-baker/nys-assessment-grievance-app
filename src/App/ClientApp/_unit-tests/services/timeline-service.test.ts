import * as assert from 'assert';
import * as mocha from 'mocha';
import { Observable } from 'rxjs';
import { TimelineValidationService } from '../../src/app/services/timeline.service';

const httpClientMock = {
    GetTimelineSetting: () =>
        new Observable((subscriber) => {
            subscriber.next(
                {submissionsDateStart: '1/1/22', submissionsDateEnd: '1/5/22', supportingDocsDateEnd: '1/6/22'}
            );
        })
};
// @ts-ignore
const getSut = (http = httpClientMock) => new TimelineValidationService(http);

mocha.describe(TimelineValidationService.name, () => {

    mocha.describe(getSut().TestInitialSubmissionDateValidity.name, () => {
        mocha.it('Returns expected', () => {
            // Arrange
            const sut = getSut();

            // Act
            const actual = sut.TestInitialSubmissionDateValidity(
                new Date(),
                new Date(),
                new Date(),
                () => undefined
            );
            // Assert
            assert.strictEqual(actual, true);
        });
    });

    mocha.describe(getSut().TestSupportingDocsUploadDateValidity.name, () => {
        mocha.it('Returns expected', () => {
            // Arrange
            const sut = getSut();

            // Act
            const actual = sut.TestSupportingDocsUploadDateValidity(
                new Date(),
                new Date(),
                new Date(),
                () => undefined
            );
            // Assert
            assert.strictEqual(actual, true);
        });
    });
});
