using A4OCore.Cfg;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Dynamic;
namespace A4OCore.Store.DB.SQLLite
{
    public class A4O_MapIdRepository
    {
        ConfigurationA4O Cfg;
        public A4O_MapIdRepository(ConfigurationA4O cfg)
        {
            this.Cfg = cfg;
        }

        public const string SQL_MAPID_TABLE_NAME = "A4O_mapId";
        public const string SQL_CREATE_MAPID = " CREATE TABLE " + SQL_MAPID_TABLE_NAME +
       " ( " +
       " elementName TEXT, " +
       " tableName TEXT, " +
       " columnName TEXT, " +
       " idColumn INTEGER, " +
       " PRIMARY KEY (elementName, tableName, columnName) " +
       " ); ";
        SqliteConnection Connection => UtilitySqlLite.GetConnection(Cfg);



        public void Insert(A4O_MapId entry)
        {
            using var conn = Connection;
            conn.Open();

            string sql = $"INSERT INTO  {SQL_MAPID_TABLE_NAME} (elementName, tableName, columnName, idColumn)" +
                "VALUES (@elementName, @tableName, @columnName, @idColumn);";
            conn.Execute(sql, new { elementName = entry.ElementName, tableName = entry.TableName, columnName = entry.ColumnName, idColumn = entry.IdColumn });



        }

        public void Update(A4O_MapId entry)
        {
            using var conn = Connection;

            string sql = $@"
            UPDATE {SQL_MAPID_TABLE_NAME}
            SET idColumn = @idColumn
            WHERE elementName = @elementName AND tableName = @tableName AND columnName = @columnName;";
            conn.Execute(sql, new
            {
                elementName = entry.ElementName,
                tableName = entry.TableName,
                columnName = entry.ColumnName,
                idColumn = entry.IdColumn
            });

        }

        public void Delete(A4O_MapId e)
        {
            if (e == null) return;

            Delete(e.ElementName, e.TableName, e.ColumnName);
        }

        public void Delete(string elementName, string tableName, string columnName)
        {
            using var conn = Connection;

            string sql = $@"
            DELETE FROM {SQL_MAPID_TABLE_NAME}
            WHERE elementName = @elementName AND tableName = @tableName AND columnName = @columnName;";

            conn.Execute(sql, new
            {
                elementName = elementName,
                tableName = tableName,
                columnName = columnName
            });


        }

        public A4O_MapId? GetByKey(string elementName, string tableName, string columnName)
        {
            using var conn = Connection;

            string sql = $@"
            SELECT elementName, tableName, columnName, idColumn
            FROM  {SQL_MAPID_TABLE_NAME}
            WHERE elementName = @elementName AND tableName = @tableName AND columnName = @columnName;";

            var persona = conn.QueryFirstOrDefault<A4O_MapId>(sql, new
            {
                elementName = elementName,
                tableName = tableName,
                columnName = columnName
            });


            return persona;
        }




        public List<A4O_MapId> GetAll(string filter, Dictionary<string, object> par = null)
        {


            using var conn = Connection;

            string sql = $"SELECT elementName, tableName, columnName, idColumn FROM {SQL_MAPID_TABLE_NAME} ";
            if (!string.IsNullOrEmpty(filter))
            {
                sql += " where " + filter;
            }
            var expando = new ExpandoObject() as IDictionary<string, object>;
            if (par != null)
            {
                foreach (var kv in par)
                    expando.Add(kv.Key, kv.Value);
            }


            var list = conn.Query<A4O_MapId>(sql, expando).ToList();


            return list;
        }

    }
}