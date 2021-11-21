using App.Services.Auth;
using FluentAssertions;
using System;
using Xunit;

namespace UnitTests
{
    public class CookieFactoryServiceTests: BaseTest
    {
        [Fact]
        public void BuildCookieOptions_ReturnsExpected()
        {
            // Arrange
            var now = DateTime.UtcNow;
            // Act
            var actual = CookieFactoryService.BuildCookieOptions(now);

            // Assert
            actual.Expires.Should().Be(now);
        }
    }
}
