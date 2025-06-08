using mcqTestBackend.Model;

namespace mcqTestBackend.Service.IService
{
    public interface ITokenService
    {
        Task<string> CreateToken(ApplicationUser user, string role);
    }
}
