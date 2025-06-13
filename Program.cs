using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OracleToDotNetMapper
{
    class Program
    {
        static string ConnectionString = "Data Source=(DESCRIPTION=(LOAD_BALANCE=on)(ADDRESS=(PROTOCOL=TCP)(HOST=cluster-bd.adea.com.mx)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=adeatol)));Persist Security Info=True;User ID=INFONAVIT_NOH_APP;Password=C4pd0R25W3N";
        //static string ConnectionString = "Data Source=(DESCRIPTION=(LOAD_BALANCE=on)(ADDRESS=(PROTOCOL=TCP)(HOST=Adeatoldbdev12.adea.com.mx)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=dev)));Persist Security Info=True;User ID=JESPINOZAT;Password=DZ43t3sPv25";

        static void Main(string[] args)
        {
            Console.WriteLine("Enter schema name: ");
            var schema = "MEXWEB";
            var tables = GetTables(schema, new string[] { "TRANSACCION_WEBMX" });

            foreach (var table in tables)
            {
                var columns = GetColumns(schema, table);
                var classCode = GenerateClassCode(schema, table, columns);
                File.WriteAllText($"{ToPascalCase(table)}.cs", classCode);
                Console.WriteLine($"Generated: {ToPascalCase(table)}.cs");
            }

            Console.WriteLine("Done.");
        }

        static List<string> GetTables(string schema, string[] tablas = null)
        {
            var tables = new List<string>();
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand($"SELECT table_name FROM all_tables WHERE owner = :schema{(tablas == null || tablas.Length == 0 ? "" :  $" AND table_name IN ({string.Join(",", tablas.Select(x=> $"'{x}'"))})")}", conn))
                {
                    cmd.Parameters.Add(new OracleParameter("schema", schema.ToUpper()));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            tables.Add(reader.GetString(0));
                    }
                }
            }
            return tables;
        }

        static List<(string ColumnName, string DataType, bool IsNullable, int? Precision, int? Scale, int? Length)> GetColumns(string schema, string table)
        {
            var columns = new List<(string, string, bool, int?, int?, int?)>();
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                SELECT column_name, data_type, nullable, data_precision, data_scale, data_length
                FROM all_tab_columns
                WHERE owner = :my_schema AND table_name = :my_table
                ORDER BY column_id";

                using (var cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("my_schema", schema.ToUpper()));
                    cmd.Parameters.Add(new OracleParameter("my_table", table.ToUpper()));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(0);
                            var type = reader.GetString(1);
                            var nullable = reader.GetString(2) == "Y";
                            var precision = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                            var scale = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4);
                            var length = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                            columns.Add((name, type, nullable, precision, scale, length));
                        }
                    }
                }
            }

            return columns;
        }

        static string MapOracleTypeToCSharp(string oracleType, bool isNullable, int? precision, int? scale)
        {
            oracleType = oracleType.ToUpperInvariant();

            switch (oracleType)
            {
                // Numéricos
                case "NUMBER":
                    if (scale.HasValue && scale.Value > 0)
                        return isNullable ? "decimal?" : "decimal";

                    if (precision.HasValue)
                    {
                        if (precision.Value <= 5)
                            return isNullable ? "short?" : "short";
                        else if (precision.Value <= 10)
                            return isNullable ? "int?" : "int";
                        else if (precision.Value <= 18)
                            return isNullable ? "long?" : "long";
                    }

                    return isNullable ? "decimal?" : "decimal";

                case "DECIMAL":
                case "DEC":
                case "NUMERIC":
                    return isNullable ? "decimal?" : "decimal";

                case "INTEGER":
                    return isNullable ? "int?" : "int";

                case "SMALLINT":
                    return isNullable ? "short?" : "short";

                case "FLOAT":
                case "DOUBLE PRECISION":
                    return isNullable ? "double?" : "double";

                case "REAL":
                case "BINARY_FLOAT":
                    return isNullable ? "float?" : "float";

                case "BINARY_DOUBLE":
                    return isNullable ? "double?" : "double";

                // Textuales
                case "VARCHAR2":
                case "NVARCHAR2":
                case "CHAR":
                case "NCHAR":
                case "CLOB":
                case "NCLOB":
                case "LONG":
                case "XMLTYPE":
                    return "string";

                // Fechas y tiempos
                case "DATE":
                case "TIMESTAMP":
                case "TIMESTAMP WITH TIME ZONE":
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                case var ts when ts.StartsWith("TIMESTAMP"):
                    return isNullable ? "DateTime?" : "DateTime";

                case "INTERVAL YEAR TO MONTH":
                case "INTERVAL DAY TO SECOND":
                case var iv when iv.StartsWith("INTERVAL"):
                    return isNullable ? "TimeSpan?" : "TimeSpan";

                // Binarios
                case "RAW":
                case "LONG RAW":
                case "BLOB":
                case "BFILE":
                    return "byte[]";

                // Identificadores
                case "ROWID":
                case "UROWID":
                    return "string";

                // Booleano
                case "BOOLEAN":
                    return isNullable ? "bool?" : "bool";

                // Fallback
                default:
                    return "object";
            }
        }

        static HashSet<string> GetPrimaryKeyColumns(string schema, string table)
        {
            var primaryKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
            SELECT acc.column_name
            FROM all_constraints ac
            JOIN all_cons_columns acc
              ON ac.constraint_name = acc.constraint_name
             AND ac.owner = acc.owner
            WHERE ac.constraint_type = 'P'
              AND ac.owner = :my_schema
              AND ac.table_name = :my_table";

                using (var cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("my_schema", schema.ToUpper()));
                    cmd.Parameters.Add(new OracleParameter("my_table", table.ToUpper()));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            primaryKeys.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return primaryKeys;
        }

        static string GenerateClassCode(string schema, string tableName, List<(string ColumnName, string DataType, bool IsNullable, int? Precision, int? Scale, int? Length)> columns)
        {
            var className = ToPascalCase(tableName);
            var code = $"[Table(\"{tableName}\", Schema = \"{schema}\")]\npublic class {className}\n{{\n";
            var primaryKeys = GetPrimaryKeyColumns(schema, tableName);
            foreach (var col in columns)
            {
                var csType = MapOracleTypeToCSharp(col.DataType, col.IsNullable, col.Precision, col.Scale);
                var propName = ToPascalCase(col.ColumnName);
                if (primaryKeys.Contains(col.ColumnName))
                {
                    int order = columns.FindIndex(c => string.Equals(c.ColumnName, col.ColumnName, StringComparison.OrdinalIgnoreCase));
                    code += $"    [Key, Column(Order = {order})]\n";
                }
                code += $"    [Column(\"{col.ColumnName}\")]\n";
                if (!col.IsNullable && csType != "string" && !csType.EndsWith("[]"))
                    code += "    [Required]\n";
                if (csType == "string" && col.Length.HasValue &&
                    !(col.DataType == "CLOB" || col.DataType == "NCLOB" || col.DataType == "LONG"))
                {
                    int actualLength = col.Length.Value;
                    if ((col.DataType == "NCHAR" || col.DataType == "NVARCHAR2") && actualLength % 2 == 0)
                    {
                        actualLength /= 2;
                    }

                    code += $"    [StringLength({actualLength})]\n";
                }
                else if (csType == "byte[]" && col.Length.HasValue &&
                        !(col.DataType == "BLOB" || col.DataType == "LONG RAW"))
                {
                    code += $"    [MaxLength({col.Length.Value})]\n";
                }
                code += $"    public {csType} {propName} {{ get; set; }}\n\n";
            }
            code += "}\n";
            return code;
        }

        static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            var words = input.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
                words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLowerInvariant());
            return string.Join("", words);
        }
    }
}
