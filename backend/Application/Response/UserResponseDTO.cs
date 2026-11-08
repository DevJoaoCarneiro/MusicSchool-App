namespace Application.Response
{
    public class UserResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public UserData? Data { get; set; }
    }

    public class UserData
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }

    public class UserListResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public UserListData? Data { get; set; }
    }

    public class UserListData
    {
        public List<UserData> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }
    }
}
