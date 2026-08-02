namespace MesaSitec.Application.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string Codigo { get; }
    public string Title { get; }
    public string TypeUri { get; }
    public Dictionary<string, string[]>? Errores { get; }

    public AppException(
        int statusCode,
        string codigo,
        string title,
        string detail,
        string? typeUri = null,
        Dictionary<string, string[]>? errores = null)
        : base(detail)
    {
        StatusCode = statusCode;
        Codigo = codigo;
        Title = title;
        TypeUri = typeUri ?? $"https://mesasitec.local/errores/{codigo.ToLowerInvariant().Replace('_', '-')}";
        Errores = errores;
    }

    public static AppException NoAutenticado(string detail = "Token ausente, inválido o credenciales incorrectas.") =>
        new(401, "NO_AUTENTICADO", "No autenticado", detail);

    public static AppException OperacionNoPermitida(string detail) =>
        new(403, "OPERACION_NO_PERMITIDA", "Operación no permitida", detail);

    public static AppException RecursoNoEncontrado(string detail = "El recurso solicitado no existe.") =>
        new(404, "RECURSO_NO_ENCONTRADO", "Recurso no encontrado", detail);

    public static AppException TransicionInvalida(string detail) =>
        new(409, "TRANSICION_INVALIDA", "Transición inválida", detail);

    public static AppException AgenteInvalido(string detail = "El agente indicado no es válido.") =>
        new(422, "AGENTE_INVALIDO", "Agente inválido", detail);

    public static AppException MotivoRequerido(string detail) =>
        new(422, "MOTIVO_REQUERIDO", "Motivo requerido", detail);

    public static AppException ParametroInvalido(string detail) =>
        new(400, "PARAMETRO_INVALIDO", "Parámetro inválido", detail);

    public static AppException Validacion(string detail, Dictionary<string, string[]> errores) =>
        new(422, "VALIDACION", "Error de validación", detail, errores: errores);
}
