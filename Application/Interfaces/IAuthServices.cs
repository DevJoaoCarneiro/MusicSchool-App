using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IAuthServices
    {
        Task<AuthResponseDTO> LoginAsync(AuthRequestDTO request);
    }
}
