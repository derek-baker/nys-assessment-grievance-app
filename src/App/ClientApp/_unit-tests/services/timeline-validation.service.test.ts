//import * as assert from 'assert';
//import * as mocha from 'mocha';
//import { TimelineValidationService } from '../../src/app/services/timeline.service';

//mocha.describe('TimelineValidationService', () => {

//    mocha.describe('TestInitialSubmissionDateValidity()', () => {
//        mocha.it('throws when submissionsDateStart occurs after supportingDocsDateEnd', () => {
//            // Arrange
//            const sut = new TimelineValidationService();
//            // Act / Assert
//            assert.throws(
//                () => sut.TestInitialSubmissionDateValidity(
//                    new Date('2020-08-04T00:00:00'),
//                    new Date('2020-08-03T00:00:00'),
//                    new Date('2020-08-02T00:00:00'),
//                    (message) => { return; }
//                )
//            );
//        });

//        mocha.it('returns true when dateNow is between submissionsDateStart and submissionsDateEnd', () => {
//            // Arrange
//            const expected = true;
//            const sut = new TimelineValidationService();

//            // Act
//            const actual = sut.TestInitialSubmissionDateValidity(
//                new Date('2020-08-01T00:00:00'),
//                new Date('2020-08-03T00:00:00'),
//                new Date('2020-08-02T00:00:00'),
//                (message) => { return; }
//            );
//            // Assert
//            assert.strictEqual(actual, expected);
//        });
//        mocha.it('returns false when dateNow is before submissionsDateStart', () => {
//            // Arrange
//            const expected = false;
//            const sut = new TimelineValidationService();

//            // Act
//            const actual = sut.TestInitialSubmissionDateValidity(
//                new Date('2020-08-02T00:00:00'),
//                new Date('2020-08-03T00:00:00'),
//                new Date('2020-08-01T00:00:00'),
//                (message) => { return; }
//            );
//            // Assert
//            assert.strictEqual(actual, expected);
//        });
//        mocha.it('returns false when dateNow is after submissionsDateEnd', () => {
//            // Arrange
//            const expected = false;
//            const sut = new TimelineValidationService();

//            // Act
//            const actual = sut.TestInitialSubmissionDateValidity(
//                new Date('2020-08-01T00:00:00'),
//                new Date('2020-08-02T00:00:00'),
//                new Date('2020-08-03T00:00:00'),
//                (message) => { return; }
//            );
//            // Assert
//            assert.strictEqual(actual, expected);
//        });
//    });

//    mocha.describe('TestSupportingDocsUploadDateValidity()', () => {
//        mocha.it('throws when submissionsDateStart occurs after supportingDocsDateEnd', () => {
//            // Arrange
//            const sut = new TimelineValidationService();
//            // Act / Assert
//            assert.throws(
//                () => sut.TestSupportingDocsUploadDateValidity(
//                    new Date('2020-08-04T00:00:00'),
//                    new Date('2020-08-03T00:00:00'),
//                    new Date('2020-08-02T00:00:00'),
//                    (message) => { return; }
//                )
//            );
//        });

//        mocha.it('returns true when dateNow is between submissionsDateStart and supportingDocsDateEnd', () => {
//            // Arrange
//            const expected = true;
//            const sut = new TimelineValidationService();

//            // Act
//            const actual = sut.TestSupportingDocsUploadDateValidity(
//                new Date('2020-08-01T00:00:00'),
//                new Date('2020-08-03T00:00:00'),
//                new Date('2020-08-02T00:00:00'),
//                (message) => { return; }
//            );
//            // Assert
//            assert.strictEqual(actual, expected);
//        });
//        mocha.it('returns false when dateNow is before submissionsDateStart', () => {
//            // Arrange
//            const expected = false;
//            const sut = new TimelineValidationService();

//            // Act
//            const actual = sut.TestSupportingDocsUploadDateValidity(
//                new Date('2020-08-02T00:00:00'),
//                new Date('2020-08-03T00:00:00'),
//                new Date('2020-08-01T00:00:00'),
//                (message) => { return; }
//            );
//            // Assert
//            assert.strictEqual(actual, expected);
//        });
//        //mocha.it('returns false when dateNow is after submissionsDateEnd', () => {
//        //    // Arrange
//        //    const expected = false;
//        //    const sut = new TimelineValidationService(
//        //        // @ts-ignore
//        //        {}
//        //    );

//        //    // Act
//        //    const actual = sut.TestSupportingDocsUploadDateValidity(
//        //        new Date('2020-08-01T00:00:00'),
//        //        new Date('2020-08-02T00:00:00'),
//        //        new Date('2020-08-03T00:00:00'),
//        //        (message) => { return; }
//        //    );
//        //    // Assert
//        //    assert.strictEqual(actual, expected);
//        //});
//    });
//});
