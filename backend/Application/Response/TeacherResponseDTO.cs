namespace Application.Response
{
    public class TeacherResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public TeacherData? Data { get; set; }
    }

    public class TeacherData
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public List<string> Specialties { get; set; } = new();
    }
}