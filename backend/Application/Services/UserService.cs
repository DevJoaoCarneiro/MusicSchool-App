using Application.Common;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class UserService : IUserServices
    {
        private const int MinPasswordLength = 6;

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
                    return Result("Parameters is empty or null", ResponseStatus.InvalidArgument);
                }

                if (string.IsNullOrWhiteSpace(userRequestDTO.Name) ||
                    string.IsNullOrWhiteSpace(userRequestDTO.Email) ||
                    string.IsNullOrWhiteSpace(userRequestDTO.Password))
                {
                    return Result("Name, email and password are required", ResponseStatus.InvalidArgument);
                }

                if (!Enum.TryParse<UserRole>(userRequestDTO.Role, ignoreCase: true, out var role) ||
                    !Enum.IsDefined(role) ||
                    int.TryParse(userRequestDTO.Role, out _))
                {
                    return Result(
                        $"Invalid role. Allowed values: {string.Join(", ", Enum.GetNames<UserRole>())}",
                        ResponseStatus.InvalidArgument);
                }

                var email = Normalizer.Email(userRequestDTO.Email);

                if (await _userRepository.ExistsByEmailAsync(email))
                {
                    return Result("Email is already in use", ResponseStatus.Conflict);
                }

                var passwordHash = _securityService.HashPassword(userRequestDTO.Password);

                var newUser = new User(
                    userRequestDTO.Name.Trim(),
                    email,
                    passwordHash,
                    role);

                await _userRepository.AddAsync(newUser);

                return Result("User created successfully", ResponseStatus.Success, ToData(newUser));
            }
            catch (Exception)
            {
                return Result("An unexpected error occurred while creating the user", ResponseStatus.Error);
            }
        }

        public async Task<UserListResponseDTO> GetAllAsync(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1 || pageSize > PaginationLimits.MaxPageSize)
            {
                return new UserListResponseDTO
                {
                    Message = PaginationLimits.InvalidMessage,
                    Status = ResponseStatus.InvalidArgument
                };
            }

            try
            {
                var (items, total) = await _userRepository.GetAllAsync(page, pageSize);

                return new UserListResponseDTO
                {
                    Message = "Users retrieved successfully",
                    Status = ResponseStatus.Success,
                    Data = new UserListData
                    {
                        Items = items.Select(ToData).ToList(),
                        Page = page,
                        PageSize = pageSize,
                        Total = total
                    }
                };
            }
            catch (Exception)
            {
                return new UserListResponseDTO
                {
                    Message = "An unexpected error occurred while retrieving users",
                    Status = ResponseStatus.Error
                };
            }
        }

        public async Task<UserResponseDTO> ChangePasswordAsync(
            Guid userId,
            ChangePasswordRequestDTO request)
        {
            if (userId == Guid.Empty)
                return Result("Invalid user", ResponseStatus.Unauthorized);

            if (request == null ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Result("Current and new passwords are required", ResponseStatus.InvalidArgument);
            }

            if (request.NewPassword.Length < MinPasswordLength)
            {
                return Result(
                    $"New password must be at least {MinPasswordLength} characters long",
                    ResponseStatus.InvalidArgument);
            }

            if (request.NewPassword == request.CurrentPassword)
            {
                return Result(
                    "New password must be different from the current password",
                    ResponseStatus.InvalidArgument);
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    return Result("User not found", ResponseStatus.NotFound);

                if (!_securityService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    return Result("Current password is incorrect", ResponseStatus.Unauthorized);

                user.ChangePassword(_securityService.HashPassword(request.NewPassword));

                await _userRepository.UpdateAsync(user);

                return Result("Password changed successfully", ResponseStatus.Success, ToData(user));
            }
            catch (Exception)
            {
                return Result("An unexpected error occurred while changing the password", ResponseStatus.Error);
            }
        }

        private static UserData ToData(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };

        private static UserResponseDTO Result(
            string message,
            string status,
            UserData? data = null) => new()
            {
                Message = message,
                Status = status,
                Data = data
            };
    }
}
