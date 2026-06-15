namespace Helpers
{
    /// <summary>
    /// Interfaze que permite usar metodos genericos en el handler para bajar archivos
    /// </summary>
    public interface IEntityFile
    {
        #region Propiedades

        string Filename { get; set; }

        int Length { get; set; }

        byte[] Content { get; set; }

        #endregion
    }
}
