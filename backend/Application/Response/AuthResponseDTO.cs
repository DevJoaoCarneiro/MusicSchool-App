namespace Application.Response
{
    public class AuthResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public AuthData? Data { get; set; }
    }

    public class AuthData
    {
        public string Token { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public AuthUserData User { get; set; } = new();
    }

    public class AuthUserData
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}