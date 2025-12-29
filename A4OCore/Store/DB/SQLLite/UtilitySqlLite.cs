using A4OCore.Cfg;
using Microsoft.Data.Sqlite;

namespace A4OCore.Store.DB.SQLLite
{
    internal class UtilitySqlLite
    {
        //public static SqliteConnection GetConnection => new SqliteConnection("Data Source=" + Cfg.Cfg.CurrentConfiguration.SQLLiteFile);

        public static string GetParamName(string parName, List<SqliteParameter> parameters)
        {
            int idx = 1;
            if (string.IsNullOrEmpty(parName) || parName.Trim() == "@") throw new Exception("wrong parameter name!!!");
            if (!parName.StartsWith("@"))
            {
                parName = "@" + parName;

            }

            var parNametmp = parName;

            while (parameters.Any(x => x.ParameterName == parName))
            {
                parName = parNametmp + "_" + idx++;
            }

            return parName;
        }

        internal static SqliteConnection GetConnection(ConfigurationA4O cfg)
        {
            return new SqliteConnection("Data Source=" + cfg.SQLLiteFile);
        }
    }
}
