namespace UnidosAfiliaciones.Application.Interfaces.Services
{
    public interface ILoginService
    {
        string Cookie { get; }

        void DoMagic();
    }
}