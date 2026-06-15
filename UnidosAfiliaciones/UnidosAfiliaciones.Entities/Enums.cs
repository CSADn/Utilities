namespace UnidosAfiliaciones.Entities
{
    public enum EstadosAfiliaciones
    {
        Ingresada = 1,
        Transcripta,
        Firmada,
        EnPartido,
        EnJusticia,
        Aceptada,
        Rechazada
    }

    public enum EstadosCiviles
    {
        Soltero = 1,
        Casado,
        Divorciado,
        Viudo
    }

    public enum EstadosUsuarios
    {
        Alta = 1,
        Baja,
        Modificado
    }
}
