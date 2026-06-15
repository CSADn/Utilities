namespace Helpers
{
    public interface IEntityDT<DT, E> 
        where DT: class, new()
        where E: class
    {
        DT Convert(E entity, params object[] parameters);
    }

    public interface IEntityDT
    {
        int Total_Rows { get; set; }

        int Filtered_Rows { get; set; }
    }
}
