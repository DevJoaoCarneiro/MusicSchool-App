using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<UserResponseDTO> CreateUser(UserRequestDTO user);

        Task<UserListResponseDTO> GetAllAsync(int page, int pageSize);

        Task<UserResponseDTO> ChangePasswordAsync(
            Guid userId,
            ChangePasswordRequestDTO request);
    }
}
