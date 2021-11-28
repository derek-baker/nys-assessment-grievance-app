using Library.Models.Entities;
using Library.Services.Auth;
using Library.Services.Clients.Database.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using Library.Services.Time;
using FluentAssertions;
using System.Text.Json;
using Library.Services.Crypto;

namespace UnitTests
{
    public class AuthServiceTests : BaseTest
    {
        private readonly Mock<IUserRepository> _usersMock = new();
        private readonly Mock<ISessionRepository> _sessionsMock = new();
        private readonly Mock<ITimeService> _timeMock = new();

        [Fact]
        public async void ValidateSession_SessionOverload_ReturnsTrueWhenSessionValid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var user = _fixtureGenerator
                .Build<User>()
                .With(u => u.UserId, userId)
                .Create();
            var session = _fixtureGenerator
                .Build<Session>()
                .With(s => s.UserId, userId)
                .With(s => s.ValidUntil, now.AddDays(1))
                .Create();
            
            _usersMock
                .Setup(m => m.GetUserById(It.IsAny<Guid>()))
                .Returns(Task.Run(() => user));

            _sessionsMock
                .Setup(m => m.GetUserSession(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.Run(() => session));

            _timeMock
                .Setup(m => m.GetTime())
                .Returns(now);

            var sut = new AuthService(
                _usersMock.Object, 
                _sessionsMock.Object, 
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var (IsValidSession, UserName) = await sut.ValidateSession(session);

            // Assert
            IsValidSession.Should().BeTrue();
        }

        [Fact]
        public async void ValidateSession_SessionOverload_ReturnsFalseWhenSessionInvalid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var user = _fixtureGenerator
                .Build<User>()
                .With(u => u.UserId, userId)
                .Create();
            var session = _fixtureGenerator
                .Build<Session>()
                .With(s => s.UserId, userId)
                .With(s => s.ValidUntil, now.AddDays(-1))
                .Create();

            _usersMock
                .Setup(m => m.GetUserById(It.IsAny<Guid>()))
                .Returns(Task.Run(() => user));

            _sessionsMock
                .Setup(m => m.GetUserSession(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.Run(() => session));

            _timeMock
                .Setup(m => m.GetTime())
                .Returns(now);

            var sut = new AuthService(
                _usersMock.Object, 
                _sessionsMock.Object, 
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var (IsValidSession, UserName) = await sut.ValidateSession(session);

            // Assert
            IsValidSession.Should().BeFalse();
        }

        [Fact]
        public async void ValidateSession_StringOverload_ReturnsTrueWhenSessionValid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var user = _fixtureGenerator
                .Build<User>()
                .With(u => u.UserId, userId)
                .Create();
            var session = _fixtureGenerator
                .Build<Session>()
                .With(s => s.UserId, userId)
                .With(s => s.ValidUntil, now.AddDays(1))
                .Create();

            _usersMock
                .Setup(m => m.GetUserById(It.IsAny<Guid>()))
                .Returns(Task.Run(() => user));

            _sessionsMock
                .Setup(m => m.GetUserSession(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.Run(() => session));

            _timeMock
                .Setup(m => m.GetTime())
                .Returns(now);

            var sut = new AuthService(
                _usersMock.Object, 
                _sessionsMock.Object, 
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var actual = await sut.ValidateSession(JsonSerializer.Serialize(session));

            // Assert
            actual.Should().BeTrue();
        }

        [Fact]
        public async void ValidateSession_StringOverload_ReturnsFalseWhenSessionNotValid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var user = _fixtureGenerator
                .Build<User>()
                .With(u => u.UserId, userId)
                .Create();
            var session = _fixtureGenerator
                .Build<Session>()
                .With(s => s.UserId, userId)
                .With(s => s.ValidUntil, now.AddDays(-1))
                .Create();

            _usersMock
                .Setup(m => m.GetUserById(It.IsAny<Guid>()))
                .Returns(Task.Run(() => user));

            _sessionsMock
                .Setup(m => m.GetUserSession(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.Run(() => session));

            _timeMock
                .Setup(m => m.GetTime())
                .Returns(now);

            var sut = new AuthService(
                _usersMock.Object, 
                _sessionsMock.Object, 
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var actual = await sut.ValidateSession(JsonSerializer.Serialize(session));

            // Assert
            actual.Should().BeFalse();
        }

        [Fact]
        public async void GenerateSecurityCode_ReturnsExpectedWhenUserExists()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var userEmail = "me@me.com";
            var user = _fixtureGenerator
                .Build<User>()
                .With(u => u.UserId, userId)
                .With(u => u.UserName, userEmail)
                .Create();
            
            _usersMock
                .Setup(m => m.GetUser(It.IsAny<string>()))
                .Returns(Task.Run(() => user));

            var sut = new AuthService(
                _usersMock.Object, 
                _sessionsMock.Object, 
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var (IsSuccess, Code, IsBuiltIn) = await sut.GenerateSecurityCode(userEmail);

            // Assert
            IsSuccess.Should().BeTrue();
            Code.Should().HaveValue();
        }

        [Fact]
        public async void GenerateSecurityCode_ReturnsExpectedWhenUserDoesntExist()
        {
            // Arrange
            var now = DateTime.UtcNow;
            
            _usersMock
                .Setup(m => m.GetUser(It.IsAny<string>()))
                .Returns(Task.Run(() => { User user = null; return user; }));

            var sut = new AuthService(
                _usersMock.Object,
                _sessionsMock.Object,
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var (IsSuccess, Code, IsBuiltIn) = await sut.GenerateSecurityCode("notLegit@notLegit.com");

            // Assert
            IsSuccess.Should().BeFalse();
            Code.Should().NotHaveValue();
        }

        [Fact]
        public void ValidateSecurityCode_ReturnsTrueWhenCodeIsValid()
        {
            // Arrange
            var now = DateTime.Now;
            var salt = "98asdfi98349t8a9($*&Y#($YKSudefh";
            var user = _fixtureGenerator
                .Build<User>()
                .With(u => u.Salt, salt)
                .Create();
            var code = HashService.GenerateSecurityCode(
                user.Salt, 
                now.ToString(format: "MM/dd/yyyy hh:mm"));

            _timeMock
                .Setup(m => m.GetTime())
                .Returns(now);

            var sut = new AuthService(
                _usersMock.Object,
                _sessionsMock.Object,
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var actual = sut.ValidateSecurityCode(code, user);

            // Assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void ValidateSecurityCode_ReturnsFalseWhenCodeIsNotValid()
        {
            // Arrange
            var now = DateTime.Now;
            var salt = "98asdfi98349t8a9($*&Y#($YKSudefh";
            var user = _fixtureGenerator
                .Build<User>()
                .With(u => u.Salt, salt)
                .Create();
            var code = 1234;

            _timeMock
                .Setup(m => m.GetTime())
                .Returns(now);

            var sut = new AuthService(
                _usersMock.Object,
                _sessionsMock.Object,
                _timeMock.Object,
                new Contracts.Settings());

            // Act
            var actual = sut.ValidateSecurityCode(code, user);

            // Assert
            actual.Should().BeFalse();
        }
    }
}
