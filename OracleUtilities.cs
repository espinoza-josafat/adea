using Oracle.ManagedDataAccess.Client;

string connStr = "User Id=usuario;Password=clave;Data Source=servidor:puerto/servicio;";
using (OracleConnection conn = new OracleConnection(connStr))
{
    conn.Open();
    OracleCommand cmd = new OracleCommand("empleado_pkg.contar_empleados", conn);
    cmd.CommandType = System.Data.CommandType.StoredProcedure;

    // Parámetro de retorno (primer parámetro)
    cmd.Parameters.Add("RETURN_VALUE", OracleDbType.Int32, System.Data.ParameterDirection.ReturnValue);

    cmd.ExecuteNonQuery();

    int cantidad = Convert.ToInt32(cmd.Parameters["RETURN_VALUE"].Value);
    Console.WriteLine("Total empleados: " + cantidad);
}


using Oracle.ManagedDataAccess.Client;

string connStr = "User Id=usuario;Password=clave;Data Source=servidor:puerto/servicio;";
using (OracleConnection conn = new OracleConnection(connStr))
{
    conn.Open();

    string sql = "SELECT empleado_seq.NEXTVAL FROM dual";
    OracleCommand cmd = new OracleCommand(sql, conn);

    object result = cmd.ExecuteScalar();
    decimal nuevoId = Convert.ToDecimal(result);

    Console.WriteLine("Nuevo ID: " + nuevoId);
}


// Obtener la hora UTC actual
DateTime utcNow = DateTime.UtcNow;

// Obtener el objeto de zona horaria para México Central
TimeZoneInfo mexicoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

// Convertir la hora UTC a hora de México
DateTime mexicoTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, mexicoTimeZone);

Console.WriteLine("Hora en México Central: " + mexicoTime.ToString("yyyy-MM-dd HH:mm:ss"));


public static DateTime GetMexicoCentralTime()
{
    string[] possibleIds = {
        "Central Standard Time (Mexico)",
        "Central Standard Time"
    };

    foreach (string id in possibleIds)
    {
        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(id);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
        catch (TimeZoneNotFoundException) { }
    }

    throw new Exception("No se encontró la zona horaria de México Central.");
}










using System;
using System.Runtime.Serialization;

namespace PollosHermano.MicroFramework.Tools.Exceptions;

[Serializable]
public class BusinessLogicException : Exception
{
    public BusinessLogicException(string message)
        : base(message)
    {
    }

    public BusinessLogicException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected BusinessLogicException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051 // El tipo o el miembro est�n obsoletos
        : base(info, context)
#pragma warning restore SYSLIB0051 // El tipo o el miembro est�n obsoletos
    {
    }
}




using System;
using System.Runtime.Serialization;

namespace PollosHermano.MicroFramework.Tools.Exceptions;

[Serializable]
public class ProcessingException : Exception
{
    public ProcessingException(string message) : base(message)
    {
    }

    public ProcessingException(string message, Exception innerException) : base(message, innerException)
    {
    }

#pragma warning disable SYSLIB0051 // El tipo o el miembro est�n obsoletos
    protected ProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
#pragma warning restore SYSLIB0051 // El tipo o el miembro est�n obsoletos
    {
    }
}





using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PollosHermano.MicroFramework.Tools.Exceptions;

[Serializable]
public class DataValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public DataValidationException(string message)
        : base(message)
    {
    }

    public DataValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DataValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred")
    {
        Errors = errors;
    }

#pragma warning disable SYSLIB0051 // El tipo o el miembro est�n obsoletos
    protected DataValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
#pragma warning restore SYSLIB0051 // El tipo o el miembro est�n obsoletos
    {
    }
}


