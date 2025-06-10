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





