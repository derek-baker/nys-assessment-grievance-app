import * as assert from 'assert';
import * as mocha from 'mocha';
import { SortService } from '../../src/app/services/sort.service';

const getSut = () => new SortService();

mocha.describe(SortService.name, () => {

    mocha.describe(getSut().BuildCompareFunc.name, () => {
        mocha.it('Builds compare func that sorts as expected', () => {
            // Arrange
            const field = 'a';
            const list = [{[field]: 2}, {[field]: 3}, {[field]: 1}];
            const sut = getSut();

            // Act
            const compareFunc = sut.BuildCompareFunc(field, false, null);
            // Assert
            assert.deepEqual(
                [{[field]: 1}, {[field]: 2}, {[field]: 3}],
                list.sort((a, b) => compareFunc(a, b))
            );
        });
    });
});
