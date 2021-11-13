using AutoFixture;
using FluentAssertions;
using Library.Services.Crypto;
using System;
using Xunit;

namespace UnitTests
{
    public class HashServiceTests
    {
        private readonly Fixture _fixtureGenerator = new Fixture();

        [Fact]
        public void HashData_IsDeterministic()
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
    }
}
