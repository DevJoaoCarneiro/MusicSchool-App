using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class UserService : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityService _securityService;

        public UserService(IUserRepository userRepository, ISecurityService securityService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
        }

        public async Task<UserResponseDTO> CreateUser(UserRequestDTO userRequestDTO)
        {
            try
            {
                if (userRequestDTO == null)
                {
                    return new UserResponseDTO
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(userRequestDTO.Name) ||
                    string.IsNullOrWhiteSpace(userRequestDTO.Email) ||
                    string.IsNullOrWhiteSpace(userRequestDTO.Password))
                {
                    return new UserResponseDTO
                    {
                        Message = "Name, email and password are required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (!Enum.TryParse<UserRole>(userRequestDTO.Role, ignoreCase: true, out var role) ||
                    !Enum.IsDefined(role))
                {
                    return new UserResponseDTO
                    {
                        Message = $"Invalid role. Allowed values: {string.Join(", ", Enum.GetNames<UserRole>())}",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (await _userRepository.ExistsByEmailAsync(userRequestDTO.Email))
                {
                    return new UserResponseDTO
                    {
                        Message = "Email is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

                string passwordHash = _securityService.HashPassword(userRequestDTO.Password);

                var newUser = new User
                (
                    userRequestDTO.Name,
                    userRequestDTO.Email,
                    passwordHash,
                    role
                );

                await _userRepository.AddAsync(newUser);

                return new UserResponseDTO
                {
                    Message = "User created successfully",
                    Status = "Success",
                    Data = new UserData
                    {
                        Name = newUser.Name,
                        Email = newUser.Email,
                        Role = newUser.Role.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
        }
    }
}
