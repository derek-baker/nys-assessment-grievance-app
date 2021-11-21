using AutoFixture;
using FluentAssertions;
using Library.Services.Crypto;
using System;
using Xunit;

namespace UnitTests
{
    public class HashServiceTests: BaseTest
    {
        [Fact]
        public void GenerateSecurityCode_IsDeterministicGivenIdenticalInputs()
        {
            // Arrange
            var valueInput = "fakeValue";
            var saltInput = "fakeSalt";
            // Act
            var actual1 = HashService.GenerateSecurityCode(valueInput, saltInput);
            var actual2 = HashService.GenerateSecurityCode(valueInput, saltInput);

            // Assert
            actual1.Should().Be(actual2);
        }

        [Fact]
        public void HashDataString_IsDeterministic()
        {
            // Arrange
            var password = _fixtureGenerator.Create<string>();
            var salt = HashService.GenerateSalt();

            // Act
            var actual1 = HashService.HashData<string>(password, salt);
            var actual2 = HashService.HashData<string>(password, salt);

            // Assert
            actual1.Should().Be(actual2);
        }

        [Fact]
        public void HashDataByteArray_IsDeterministic()
        {
            // Arrange
            var password = _fixtureGenerator.Create<string>();
            var salt = HashService.GenerateSalt();

            // Act
            var actual1 = HashService.HashData<byte[]>(password, salt);
            var actual2 = HashService.HashData<byte[]>(password, salt);

            // Assert
            CompareByteArrays(actual1, actual2).Should().BeTrue();
        }

        [Fact]
        public void GenerateSalt_IsNotDeterministic()
        {
            // Arrange
            // Act
            var actual1 = HashService.GenerateSalt();
            var actual2 = HashService.GenerateSalt();

            // Assert
            HashService.ConvertSaltToString(actual1)
                .Should()
                .NotBe(HashService.ConvertSaltToString(actual2));
        }

        [Fact]
        public void GenerateSalt_ReturnsExpected()
        {
            // Arrange
            var maxLengthParam = 32;
            // Act
            var actual = HashService.GenerateSalt(maxLengthParam);

            // Assert
            actual.Should().NotBeNull();
            actual.Length.Should().Be(maxLengthParam);
        }

        private static bool CompareByteArrays(ReadOnlySpan<byte> byteArray1, ReadOnlySpan<byte> byteArray2)
            => byteArray1.SequenceEqual(byteArray2);
    }
}
