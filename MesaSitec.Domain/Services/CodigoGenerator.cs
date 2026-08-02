namespace MesaSitec.Domain.Services;

public static class CodigoGenerator
{
    public static string Generar(int correlativo, DateTime fechaCreacion)
    {
        return $"SOL-{fechaCreacion.Year:D4}-{correlativo:D5}";
    }
}
