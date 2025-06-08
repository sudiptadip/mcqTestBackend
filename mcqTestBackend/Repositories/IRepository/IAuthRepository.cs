using mcqTestBackend.Dtos.Auth;

namespace mcqTestBackend.Repositories.IRepository
{
    public interface IAuthRepository
    {
        Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserDTO model);
        Task<LoginUserResponseDTO> LoginAsync(LoginUserDTO model);
    }
}
