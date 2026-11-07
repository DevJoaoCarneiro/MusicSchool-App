using Application.Common;
using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using NSubstitute;

namespace Tests.Services;

public class TeacherServiceTest
{
    private readonly ITeacherRepository _teacherRepository = Substitute.For<ITeacherRepository>();
    private readonly IInstrumentRepository _instrumentRepository = Substitute.For<IInstrumentRepository>();
    private readonly TeacherService _service;

    public TeacherServiceTest()
    {
        _service = new TeacherService(_teacherRepository, _instrumentRepository);
        _instrumentRepository.GetAllAsync().Returns(new List<Instrument>
        {
            new("Piano", "Teclado"),
            new("Violão", "Cordas"),
        });
    }

    private static TeacherRequestDTO Request(params string[] specialties) => new()
    {
        Name = "Rafael Souza",
        Email = "rafael@escola.com",
        Phone = "11944443333",
        Cpf = "52998224725",
        Specialties = specialties.ToList(),
    };

    [Fact]
    public async Task Create_With_Unregistered_Specialty_Should_Be_Rejected()
    {
        var result = await _service.CreateTeacher(Request("Piano", "Harpa"));

        Assert.Equal(ResponseStatus.InvalidArgument, result.Status);
        Assert.Contains("Harpa", result.Message);
        await _teacherRepository.DidNotReceiveWithAnyArgs().AddAsync(default!);
    }

    [Fact]
    public async Task Create_Should_Store_Specialties_With_Instrument_Names()
    {
        var result = await _service.CreateTeacher(Request("piano", "VIOLÃO"));

        Assert.Equal(ResponseStatus.Success, result.Status);
        await _teacherRepository.Received(1).AddAsync(Arg.Is<Teacher>(teacher =>
            teacher.Specialties.SequenceEqual(new[] { "Piano", "Violão" })));
    }

    [Fact]
    public async Task Update_With_Unregistered_Specialty_Should_Be_Rejected()
    {
        var teacher = new Teacher("Rafael Souza", "rafael@escola.com", "11944443333", "52998224725", new List<string> { "Piano" });
        _teacherRepository.GetByIdAsync(teacher.Id).Returns(teacher);

        var result = await _service.UpdateTeacher(teacher.Id, Request("Bateria"));

        Assert.Equal(ResponseStatus.InvalidArgument, result.Status);
        await _teacherRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!);
    }
}
