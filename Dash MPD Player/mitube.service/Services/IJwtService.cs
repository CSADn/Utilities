namespace mitube.service.Services;

public interface IJwtService
{
    string GenerateToken(string username, int userId);
}
