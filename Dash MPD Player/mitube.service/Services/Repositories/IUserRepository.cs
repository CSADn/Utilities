using mitube.service.Models;

namespace mitube.service.Services.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
}
