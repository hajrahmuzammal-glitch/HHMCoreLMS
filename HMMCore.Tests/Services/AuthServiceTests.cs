using FluentAssertions;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Auth;
using HHMCore.Core.Entities;
using HHMCore.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HHMCore.Tests.Services;

public sealed class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly Mock<IConfiguration> _config;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManager = new Mock<UserManager<AppUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _config = new Mock<IConfiguration>();

        // JWT config keys read in GenerateTokenAsync and both public methods
        _config.Setup(c => c["JWT:Key"])
               .Returns("HHMCoreLMS@SuperSecretKey#2024!MustBe32Chars");
        _config.Setup(c => c["JWT:Issuer"]).Returns("HHMCore.API");
        _config.Setup(c => c["JWT:Audience"]).Returns("HHMCore.Client");
        _config.Setup(c => c["JWT:DurationInMinutes"]).Returns("60");

        _sut = new AuthService(_userManager.Object, _config.Object);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static AppUser MakeUser() => new()
    {
        Id = "user-001",
        Email = "admin@test.com",
        UserName = "admin@test.com",
        FullName = "Test Admin"
    };

    private static RegisterDto ValidRegister() => new()
    {
        FullName = "Test Admin",
        Email = "admin@test.com",
        Password = "Admin@123",
        ConfirmPassword = "Admin@123",
        Role = AppRoles.Admin
    };

    private static LoginDto ValidLogin() => new()
    {
        Email = "admin@test.com",
        Password = "Admin@123"
    };

    // shared happy-path setup for RegisterAsync
    private void SetupRegisterHappyPath()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<AppUser>()))
                    .ReturnsAsync(new List<string> { AppRoles.Admin });
    }

    // ── RegisterAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidDto_ReturnsSuccessWithToken()
    {
        SetupRegisterHappyPath();

        var result = await _sut.RegisterAsync(ValidRegister());

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.Role.Should().Be(AppRoles.Admin);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(MakeUser());

        var result = await _sut.RegisterAsync(ValidRegister());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already registered");
        _userManager.Verify(m => m.CreateAsync(
            It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_IdentityCreateFails_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Failed(
                        new IdentityError { Description = "Password too weak." }));

        var result = await _sut.RegisterAsync(ValidRegister());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
        _userManager.Verify(m => m.AddToRoleAsync(
            It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_RoleAssignmentFails_DeletesUserAndReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Failed(
                        new IdentityError { Description = "Role not found." }));
        _userManager.Setup(m => m.DeleteAsync(It.IsAny<AppUser>()))
                    .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterAsync(ValidRegister());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("does not exist");
        _userManager.Verify(m => m.DeleteAsync(It.IsAny<AppUser>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailToLowercase()
    {
        AppUser? captured = null;
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .Callback<AppUser, string>((u, _) => captured = u)
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<AppUser>()))
                    .ReturnsAsync(new List<string> { AppRoles.Admin });

        var dto = ValidRegister();
        dto.Email = "ADMIN@TEST.COM";
        await _sut.RegisterAsync(dto);

        captured!.Email.Should().Be("admin@test.com");
        captured.UserName.Should().Be("admin@test.com");
    }
    [Fact]
    public async Task RegisterAsync_TrimsWhitespaceFromEmail()
    {
        AppUser? captured = null;
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .Callback<AppUser, string>((u, _) => captured = u)
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<AppUser>()))
                    .ReturnsAsync(new List<string> { "Admin" });

        var dto = ValidRegister();
        dto.Email = "  admin@test.com  ";
        await _sut.RegisterAsync(dto);

        captured!.Email.Should().Be("admin@test.com");
    }
    [Fact]
    public async Task RegisterAsync_ResponseContainsExpiresAt()
    {
        SetupRegisterHappyPath();

        var before = DateTime.UtcNow;
        var result = await _sut.RegisterAsync(ValidRegister());
        var after = DateTime.UtcNow.AddMinutes(61);

        result.Data!.ExpiresAt.Should().BeAfter(before);
        result.Data.ExpiresAt.Should().BeBefore(after);
    }

    // ── LoginAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(MakeUser());
        _userManager.Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<AppUser>()))
                    .ReturnsAsync(new List<string> { AppRoles.Admin });

        var result = await _sut.LoginAsync(ValidLogin());

        result.Success.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.Email.Should().Be("admin@test.com");
        result.Data.Role.Should().Be(AppRoles.Admin);
    }

    [Fact]
    public async Task LoginAsync_EmailNotFound_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);

        var result = await _sut.LoginAsync(ValidLogin());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid email or password");
        _userManager.Verify(m => m.CheckPasswordAsync(
            It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(MakeUser());
        _userManager.Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(false);

        var result = await _sut.LoginAsync(ValidLogin());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_NormalizesEmailBeforeLookup()
    {
        string? capturedEmail = null;
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .Callback<string>(e => capturedEmail = e)
                    .ReturnsAsync((AppUser?)null);

        var dto = ValidLogin();
        dto.Email = "ADMIN@TEST.COM";
        await _sut.LoginAsync(dto);

        capturedEmail.Should().Be("admin@test.com");
    }
    [Fact]
    public async Task LoginAsync_TrimsWhitespaceFromEmail()
    {
        string? capturedEmail = null;
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .Callback<string>(e => capturedEmail = e)
                    .ReturnsAsync((AppUser?)null);

        var dto = ValidLogin();
        dto.Email = "  admin@test.com  ";
        await _sut.LoginAsync(dto);

        capturedEmail.Should().Be("admin@test.com");
    }
    [Fact]
    public async Task LoginAsync_UserWithNoRole_ReturnsSuccessWithEmptyRole()
    {

        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(MakeUser());
        _userManager.Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<AppUser>()))
                    .ReturnsAsync(new List<string>());

        var result = await _sut.LoginAsync(ValidLogin());

        result.Success.Should().BeTrue();
        result.Data!.Role.Should().BeEmpty();
    }
}
