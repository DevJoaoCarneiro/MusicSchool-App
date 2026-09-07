using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Interfaces;

namespace Application.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityService _securityService;

        public AuthServices(
            IUserRepository userRepository,
            ISecurityService securityService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
        }

        public async Task<AuthResponseDTO> LoginAsync(AuthRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    return new AuthResponseDTO
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return new AuthResponseDTO
                    {
                        Message = "Email and password are required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }
                return new AuthResponseDTO
                {
                    Message = "Login implementation pending",
                    Status = "error",
                    Data = null
                };
            }
            catch (Exception)
            {
                return new AuthResponseDTO
                {
                    Message = "An unexpected error occurred",
                    Status = "error",
                    Data = null
                };
            }
        }
    }
}