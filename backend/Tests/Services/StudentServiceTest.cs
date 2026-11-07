using Application.Common;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Services;

public class StudentServiceTest
{
    private const string ValidCpf = "52998224725";
    private const string ValidGuardianCpf = "11144477735";

    private readonly IStudentRepository _studentRepository = Substitute.For<IStudentRepository>();
    private readonly IEnrollmentRepository _enrollmentRepository = Substitute.For<IEnrollmentRepository>();
    private readonly StudentService _service;

    public StudentServiceTest()
    {
        _service = new StudentService(_studentRepository, _enrollmentRepository);
    }

    private static StudentRequestDTO AdultRequest(string cpf = ValidCpf) => new()
    {
        Name = " Maria ",
        Email = " Maria@Email.COM ",
        Phone = "11999999999",
        Cpf = cpf,
        BirthDate = new DateTime(1990, 1, 1),
        Address = new AddressRequestDTO
        {
            Street = "Rua A",
            Number = "1",
            City = "Sao Paulo",
            State = "sp",
            ZipCode = "01000-000"
        }
    };

    private static Student ExistingStudent() => new(
        "Maria", "maria@email.com", "119", ValidCpf, new DateTime(2015, 1, 1),
        new Address("Rua", "1", "SP", "SP", "01000-000"),
        new Guardian("Mae", "119", "mae@email.com", ValidGuardianCpf));

    [Fact]
    public async Task Create_Should_Reject_Cpf_With_Invalid_Check_Digits()
    {
        var result = await _service.CreateStudent(AdultRequest("12345678900"));

        Assert.Equal(ResponseStatus.InvalidArgument, result.Status);
        await _studentRepository.DidNotReceive().AddAsync(Arg.Any<Student>());
    }

    [Fact]
    public async Task Create_Should_Normalize_Email_Name_And_State()
    {
        var result = await _service.CreateStudent(AdultRequest());

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal("maria@email.com", result.Data!.Email);
        Assert.Equal("Maria", result.Data.Name);
        Assert.Equal("SP", result.Data.Address.State);
    }

    [Fact]
    public async Task Create_Should_Not_Leak_Exception_Message()
    {
        _studentRepository.AddAsync(Arg.Any<Student>())
            .ThrowsAsync(new Exception("duplicate key value violates unique constraint IX_Students_Cpf"));

        var result = await _service.CreateStudent(AdultRequest());

        Assert.Equal(ResponseStatus.Error, result.Status);
        Assert.DoesNotContain("IX_Students", result.Message);
    }

    [Fact]
    public async Task Update_Should_Remove_Guardian_When_Not_Informed()
    {
        var student = ExistingStudent();
        _studentRepository.GetByIdAsync(student.Id).Returns(student);

        var result = await _service.UpdateStudentAsync(student.Id, AdultRequest());

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Null(student.Guardian);
        Assert.Null(result.Data!.Guardian);
        await _studentRepository.Received(1).UpdateAsync(student);
    }

    [Fact]
    public async Task Delete_Should_Return_Conflict_When_Student_Has_Enrollment_History()
    {
        var student = ExistingStudent();
        _studentRepository.GetByIdAsync(student.Id).Returns(student);
        _enrollmentRepository.HasActiveEnrollmentsByStudentAsync(student.Id).Returns(false);
        _enrollmentRepository.HasAnyEnrollmentByStudentAsync(student.Id).Returns(true);

        var result = await _service.DeleteStudentAsync(student.Id);

        Assert.Equal(ResponseStatus.Conflict, result.Status);
        await _studentRepository.DidNotReceive().DeleteAsync(Arg.Any<Student>());
    }

    [Fact]
    public async Task GetAll_Should_Reject_PageSize_Above_Limit()
    {
        var result = await _service.GetAllAsync(null, 1, PaginationLimits.MaxPageSize + 1);

        Assert.Equal(ResponseStatus.InvalidArgument, result.Status);
    }
}
