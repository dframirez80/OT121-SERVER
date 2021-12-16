using OngProject.Core.DTOs.UserDTOs;
using OngProject.Core.Entities;
using System.Threading.Tasks;

namespace OngProject.Core.Interfaces.IServices
{
    public interface IUserServices
    {
        Task<UserRegistrationDTO> RegisterAsync(UserRegistrationDTO user);

        Task<UserLoginResponseDTO> LoginAsync(UserLoginRequestDTO user);
    }
}