namespace Application.Response
{
    public class TeacherListResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public TeacherListData? Data { get; set; }
    }

    public class TeacherListData
    {
        public List<TeacherListItem> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }
    }

    public class TeacherListItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Specialties { get; set; } = new();
    }
}