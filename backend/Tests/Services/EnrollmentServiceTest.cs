using Application.Common;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using NSubstitute;

namespace Tests.Services;

public class EnrollmentServiceTest
{
    private readonly IEnrollmentRepository _enrollmentRepository = Substitute.For<IEnrollmentRepository>();
    private readonly IStudentRepository _studentRepository = Substitute.For<IStudentRepository>();
    private readonly IInstrumentRepository _instrumentRepository = Substitute.For<IInstrumentRepository>();
    private readonly ILessonRepository _lessonRepository = Substitute.For<ILessonRepository>();
    private readonly ITeacherRepository _teacherRepository = Substitute.For<ITeacherRepository>();
    private readonly EnrollmentService _service;

    public EnrollmentServiceTest()
    {
        _service = new EnrollmentService(
            _enrollmentRepository,
            _studentRepository,
            _instrumentRepository,
            _lessonRepository,
            _teacherRepository);
    }

    [Fact]
    public async Task Close_With_Future_End_Date_Should_Only_Be_Blocked_By_Lessons_After_End_Date()
    {
        var enrollment = new Enrollment(Guid.NewGuid(), Guid.NewGuid(), DateTime.Today.AddMonths(-2), 100m, 10);
        _enrollmentRepository.GetByIdAsync(enrollment.Id).Returns(enrollment);

        var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        var result = await _service.CloseEnrollmentAsync(
            enrollment.Id,
            new CloseEnrollmentRequestDTO { EndDate = endDate, Reason = "Mudou de cidade" });

        Assert.Equal(ResponseStatus.Success, result.Status);

        await _lessonRepository.Received(1).HasFutureScheduledLessonsByEnrollmentAsync(
            enrollment.Id,
            Arg.Is<DateTime>(d => d.Date == endDate.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public async Task GetAll_For_Teacher_Without_Profile_Should_Return_Empty_List()
    {
        _teacherRepository.GetByEmailAsync("prof@x.com").Returns((Teacher?)null);

        var result = await _service.GetAllAsync(null, null, null, "prof@x.com", 1, 20);

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Empty(result.Data!.Items);
        await _enrollmentRepository.DidNotReceiveWithAnyArgs()
            .GetAllAsync(default, default, default, default, default, default);
    }

    [Fact]
    public async Task GetAll_For_Teacher_Should_Filter_By_Teacher()
    {
        var teacher = new Teacher("Prof", "prof@x.com", "119", "52998224725", new List<string> { "Piano" });
        _teacherRepository.GetByEmailAsync("prof@x.com").Returns(teacher);
        _enrollmentRepository.GetAllAsync(null, null, null, teacher.Id, 1, 20)
            .Returns((new List<Enrollment>(), 0));

        var result = await _service.GetAllAsync(null, null, null, "prof@x.com", 1, 20);

        Assert.Equal(ResponseStatus.Success, result.Status);
        await _enrollmentRepository.Received(1).GetAllAsync(null, null, null, teacher.Id, 1, 20);
    }
}
