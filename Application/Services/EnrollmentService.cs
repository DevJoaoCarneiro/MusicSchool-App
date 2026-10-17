using System;
using System.Collections.Generic;
using System.Text;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class EnrollmentService : IEnrollmentServices
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IInstrumentRepository _instrumentRepository;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository,
            IInstrumentRepository instrumentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _instrumentRepository = instrumentRepository;
        }

        public async Task<EnrollmentResponseDTO> CreateEnrollmentAsync(
            EnrollmentRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    return Error(
                        "Request body is required",
                        "invalid_argument");
                }

                if (request.StudentId == Guid.Empty ||
                    request.InstrumentId == Guid.Empty ||
                    request.StartDate == null)
                {
                    return Error(
                        "StudentId, InstrumentId and StartDate are required",
                        "invalid_argument");
                }

                if (request.MonthlyFee <= 0)
                {
                    return Error(
                        "Monthly fee must be greater than zero",
                        "invalid_argument");
                }

                if (request.DueDay < 1 || request.DueDay > 28)
                {
                    return Error(
                        "Due day must be between 1 and 28",
                        "invalid_argument");
                }

                var student = await _studentRepository
                    .GetByIdAsync(request.StudentId);

                if (student == null)
                {
                    return Error("Student not found", "not_found");
                }

                var instrument = await _instrumentRepository
                    .GetByIdAsync(request.InstrumentId);

                if (instrument == null)
                {
                    return Error("Instrument not found", "not_found");
                }

                var alreadyEnrolled = await _enrollmentRepository
                    .HasActiveEnrollmentAsync(
                        request.StudentId,
                        request.InstrumentId);

                if (alreadyEnrolled)
                {
                    return Error(
                        "Student already has an active enrollment for this instrument",
                        "conflict");
                }

                var enrollment = new Enrollment(
                    request.StudentId,
                    request.InstrumentId,
                    request.StartDate.Value,
                    request.MonthlyFee,
                    request.DueDay);

                await _enrollmentRepository.AddAsync(enrollment);

                return new EnrollmentResponseDTO
                {
                    Message = "Enrollment created successfully",
                    Status = "Success",


                    Data = new EnrollmentData
                    {
                        Id = enrollment.Id,

                        Student = new StudentEnrollmentData
                        {
                            Id = student.Id,
                            Name = student.Name
                        },

                        Instrument = new InstrumentEnrollmentData
                        {
                            Id = instrument.Id,
                            Name = instrument.Name
                        },

                        StartDate = enrollment.StartDate,
                        MonthlyFee = enrollment.MonthlyFee,
                        DueDay = enrollment.DueDay,
                        Status = enrollment.Status.ToString()
                    }
                };
            }
            catch (Exception)
            {
                return Error(
                    "An unexpected error occurred while creating the enrollment",
                    "error");
            }
        }

        private static EnrollmentResponseDTO Error(
            string message,
            string status)
        {
            return new EnrollmentResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }
    }
}
