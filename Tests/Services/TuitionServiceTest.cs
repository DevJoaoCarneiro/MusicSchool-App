using Application.Common;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Services;

public class TuitionServiceTest
{
    private readonly ITuitionRepository _tuitionRepository = Substitute.For<ITuitionRepository>();
    private readonly IEnrollmentRepository _enrollmentRepository = Substitute.For<IEnrollmentRepository>();
    private readonly TuitionService _service;

    public TuitionServiceTest()
    {
        _service = new TuitionService(_tuitionRepository, _enrollmentRepository);
    }

    private static string CurrentMonth => DateTime.Today.ToString("yyyy-MM");

    private static Tuition PendingTuition(decimal fee = 200m)
    {
        var enrollment = new Enrollment(Guid.NewGuid(), Guid.NewGuid(), DateTime.Today.AddMonths(-1), fee, 10);
        return new Tuition(enrollment, CurrentMonth);
    }

    private static RegisterTuitionPaymentRequestDTO Payment(decimal amount, DateOnly? paidAt = null) => new()
    {
        Amount = amount,
        Method = "pix",
        PaidAt = paidAt ?? DateOnly.FromDateTime(DateTime.Today)
    };

    [Theory]
    [InlineData("0001-01")]
    [InlineData("1999-12")]
    public async Task Generate_Should_Reject_Absurd_Reference_Month(string month)
    {
        var result = await _service.GenerateAsync(new GenerateTuitionsRequestDTO { ReferenceMonth = month });

        Assert.Equal(ResponseStatus.InvalidArgument, result.Status);
    }

    [Fact]
    public async Task Generate_Should_Return_Conflict_On_Concurrent_Generation()
    {
        var enrollment = new Enrollment(Guid.NewGuid(), Guid.NewGuid(), DateTime.Today.AddMonths(-1), 100m, 5);
        _enrollmentRepository.GetBillableForMonthAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Enrollment> { enrollment });
        _tuitionRepository.GetEnrollmentIdsByReferenceMonthAsync(CurrentMonth).Returns(new HashSet<Guid>());
        _tuitionRepository.AddRangeAsync(Arg.Any<IEnumerable<Tuition>>())
            .ThrowsAsync(new ConcurrencyConflictException("concurrent"));

        var result = await _service.GenerateAsync(new GenerateTuitionsRequestDTO { ReferenceMonth = CurrentMonth });

        Assert.Equal(ResponseStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Payment_Should_Reject_Partial_Amount()
    {
        var tuition = PendingTuition(200m);
        _tuitionRepository.GetByIdAsync(tuition.Id).Returns(tuition);

        var result = await _service.RegisterPaymentAsync(tuition.Id, Payment(150m));

        Assert.Equal(ResponseStatus.InvalidArgument, result.Status);
        await _tuitionRepository.DidNotReceive().RegisterPaymentIfPendingAsync(Arg.Any<Tuition>());
    }

    [Fact]
    public async Task Payment_Should_Reject_Future_Date()
    {
        var tuition = PendingTuition();
        _tuitionRepository.GetByIdAsync(tuition.Id).Returns(tuition);

        var result = await _service.RegisterPaymentAsync(
            tuition.Id,
            Payment(200m, DateOnly.FromDateTime(DateTime.Today.AddDays(1))));

        Assert.Equal(ResponseStatus.InvalidArgument, result.Status);
    }

    [Fact]
    public async Task Payment_Should_Return_Conflict_When_Paid_Concurrently()
    {
        var tuition = PendingTuition();
        _tuitionRepository.GetByIdAsync(tuition.Id).Returns(tuition);
        _tuitionRepository.RegisterPaymentIfPendingAsync(tuition).Returns(false);

        var result = await _service.RegisterPaymentAsync(tuition.Id, Payment(200m));

        Assert.Equal(ResponseStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Payment_Should_Succeed_With_Full_Amount()
    {
        var tuition = PendingTuition();
        _tuitionRepository.GetByIdAsync(tuition.Id).Returns(tuition);
        _tuitionRepository.RegisterPaymentIfPendingAsync(tuition).Returns(true);

        var result = await _service.RegisterPaymentAsync(tuition.Id, Payment(200m));

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal("Paid", result.Data!.Status);
        Assert.Equal("Pix", result.Data.Method);
    }
}
