namespace Application.Response
{
    public class StudentListResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public StudentListData? Data { get; set; }
    }

    public class StudentListData
    {
        public List<StudentListItem> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }
    }

    public class StudentListItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }
    }
}