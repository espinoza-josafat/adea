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
