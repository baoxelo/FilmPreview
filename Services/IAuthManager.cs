using FilmPreview.Model;

namespace FilmPreview.Services
{
    public interface IAuthManager
    {
        Task<bool> ValidateUser(LoginUserDTO userDTO);
        Task<string> CreateToken(LoginUserDTO userDTO);
    }
}
