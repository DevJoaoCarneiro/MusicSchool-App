using Application.Common;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using NSubstitute;

namespace Tests.Services;

public class CommonTest
{
    [Theory]
    [InlineData("529.982.247-25", true)]
    [InlineData("11144477735", true)]
    [InlineData("12345678900", false)]
    [InlineData("11111111111", false)]
    [InlineData("123", false)]
    public void CpfValidator_Should_Validate_Check_Digits(string cpf, bool expected)
    {
        Assert.Equal(expected, CpfValidator.IsValid(CpfValidator.Normalize(cpf)));
    }

    [Fact]
    public void Normalizer_Should_Convert_Utc_To_Unspecified_Local_Time()
    {
        var utc = new DateTime(2026, 11, 10, 13, 0, 0, DateTimeKind.Utc);

        var result = Normalizer.LocalDateTime(utc);

        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
        Assert.Equal(utc.ToLocalTime().Ticks, result.Ticks);
    }

    [Fact]
    public void Normalizer_Should_Keep_Unspecified_Time()
    {
        var local = new DateTime(2026, 11, 10, 13, 0, 0, DateTimeKind.Unspecified);

        Assert.Equal(local, Normalizer.LocalDateTime(local));
    }

    [Fact]
    public async Task ChangePassword_Should_Reject_Wrong_Current_Password()
    {
        var users = Substitute.For<IUserRepository>();
        var security = Substitute.For<ISecurityService>();
        var user = new User("Ana", "ana@x.com", "hash", UserRole.Secretary);
        users.GetByIdAsync(user.Id).Returns(user);
        security.VerifyPassword("errada", "hash").Returns(false);

        var service = new UserService(users, security);

        var result = await service.ChangePasswordAsync(
            user.Id,
            new ChangePasswordRequestDTO { CurrentPassword = "errada", NewPassword = "Nova#1234" });

        Assert.Equal(ResponseStatus.Unauthorized, result.Status);
        await users.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public void Lesson_Complete_Should_Fail_For_Future_Lesson()
    {
        var enrollment = new Enrollment(Guid.NewGuid(), Guid.NewGuid(), DateTime.Today, 100m, 5);
        var lesson = new Lesson(enrollment, Guid.NewGuid(), DateTime.Now.AddDays(1), 60, "Sala 1");

        Assert.Throws<InvalidOperationException>(() => lesson.Complete());
    }
}
