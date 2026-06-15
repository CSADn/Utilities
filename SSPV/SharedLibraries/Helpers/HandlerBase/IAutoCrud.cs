namespace Helpers
{
    public interface IAutoCrud
    {
        bool GenericMethodAllowed(string method);

        string Invoke(string method);
    }
}
