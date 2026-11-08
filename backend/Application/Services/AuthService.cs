using Application.Interfaces;
using Application.Common;
using Application.Request;
using Application.Response;
using Domain.Interfaces;

namespace Application.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityService _securityService;
        private readonly ITokenService _tokenService;

        public AuthServices(
            IUserRepository userRepository,
            ISecurityService securityService,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDTO> LoginAsync(AuthRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    return new AuthResponseDTO
                    {
                        Message = "Parâmetros não informados",

                        Status = ResponseStatus.InvalidArgument,

                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return new AuthResponseDTO
                    {
                        Message = "E-mail e senha são obrigatórios",

                        Status = ResponseStatus.InvalidArgument,

                        Data = null
                    };
                }

                var user = await _userRepository.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    return new AuthResponseDTO
                    {
                        Message = "E-mail ou senha inválidos",
                        Status = ResponseStatus.Unauthorized,
                        Data = null
                    };
                }

                var passwordIsValid = _securityService.VerifyPassword(request.Password, user.PasswordHash);

                if (!passwordIsValid)
                {
                    return new AuthResponseDTO
                    {
                        Message = "E-mail ou senha inválidos",
                        Status = ResponseStatus.Unauthorized,
                        Data = null
                    };
                }

                var token = _tokenService.GenerateToken(user);

                return new AuthResponseDTO
                {
                    Message = "Login realizado com sucesso",
                    Status = ResponseStatus.Success,
                    Data = new AuthData
                    {
                        Token = token,
                        ExpiresIn = 3600,
                        User = new AuthUserData
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            Role = user.Role.ToString()
                        }
                    }
                };
            }
            catch (Exception)
            {
                return new AuthResponseDTO
                {
                    Message = "Ocorreu um erro inesperado",

                    Status = ResponseStatus.Error,

                    Data = null
                };
            }
        }
    }
}