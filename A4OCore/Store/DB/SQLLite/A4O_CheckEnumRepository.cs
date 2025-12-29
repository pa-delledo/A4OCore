using A4OCore.Cfg;
using A4OCore.Utility;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Dynamic;
namespace A4OCore.Store.DB.SQLLite
{
    public class A4O_CheckEnumRepository : IA4O_CheckEnumRepository
    {

        public A4O_CheckEnumRepository()
        {

        }
        public void CheckEnumChanged(Dictionary<string, int> designDictionary, string elementName, bool isTable)
        {
            // verify the order of elements


            Dictionary<string, object> par = new Dictionary<string, object>();
            par.Add("@et", elementName);
            par.Add("@isTab", isTable);
            var all = Task.Run(() => this.GetAllAsync(" ElementName=@et and isTable=@isTab", par)).GetAwaiter().GetResult();






            bool exception = false;
            foreach (var valore in designDictionary)
            {
                A4O_CheckEnum current = new A4O_CheckEnum()
                {
                    ElementName = elementName,
                    EnumString = valore.Key,
                    EnumInt = valore.Value,
                    IsTable = isTable
                };
                var found = all.FirstOrDefault(x => x.IsTable == current.IsTable && x.ElementName == current.ElementName && x.EnumString == current.EnumString);
                if (found == null)
                {
                    this.Insert(current);
                    all.Add(current);
                    continue;
                }

                if (found.EnumInt == current.EnumInt) continue;
                exception = true;
            }
            if (exception)
            {
                string elName = isTable ? "EnumTable" : "EnumElement";
                throw new Exception($"for {elementName} , Dictionary : {elName} the items must be: " + string.Join(",", all.Select(x => x.EnumString + " = " + x.EnumInt)));
            }
        }

        SqliteConnection Connection => UtilitySqlLite.GetConnection(Cfg);
        ConfigurationA4O Cfg;
        public A4O_CheckEnumRepository(ConfigurationA4O cfg)
        {
            this.Cfg = cfg;
            var exCheckEnum = DBBase.ExistsTable(Connection, A4O_CheckEnumRepository.SQL_CHECK_ENUM_TABLE_NAME);

            if (!exCheckEnum)
            {
                using var conn = Connection;
                conn.Open();
                conn.Execute(A4O_CheckEnumRepository.SQL_CREATE_CHECK_ENUM);
                conn.Close();


            }

        }

        public const string SQL_CHECK_ENUM_TABLE_NAME = "A4O_CheckEnum";
        public const string SQL_CREATE_CHECK_ENUM = " CREATE TABLE " + SQL_CHECK_ENUM_TABLE_NAME +
       " ( " +
       " elementName TEXT, " +
       " enumString TEXT, " +
       " isTable INTEGER, " +
       " enumInt INTEGER, " +
       " PRIMARY KEY (elementName, enumString , isTable ) " +
       " ); ";




        public void Insert(A4O_CheckEnum entry)
        {
            AsyncAwaitUtils.Wait(() => InsertAsync(entry));
        }
        public async Task InsertAsync(A4O_CheckEnum entry)
        {
            using var conn = this.Connection;
            await conn.OpenAsync();

            string sql = "INSERT INTO " + SQL_CHECK_ENUM_TABLE_NAME + " (elementName, EnumString, IsTable, EnumInt)" +
                "VALUES (@elementName, @enumString, @isTable, @enumInt );";
            await conn.ExecuteAsync(sql, new { elementName = entry.ElementName, enumString = entry.EnumString, enumInt = entry.EnumInt, isTable = entry.IsTable ? 1 : 0 });

        }

        public void Update(A4O_CheckEnum entry)
        {
            AsyncAwaitUtils.Wait(() => UpdateAsync(entry));
        }
        public async Task UpdateAsync(A4O_CheckEnum entry)
        {
            using var conn = Connection;
            await conn.OpenAsync();

            string sql = @"
            UPDATE " + SQL_CHECK_ENUM_TABLE_NAME +
            " SET enumInt = @enumInt " +
            "WHERE elementName = @eelementName AND enumString= @enumString AND isTable = @isTable;";
            await conn.ExecuteAsync(sql, new { elementName = entry.ElementName, enumString = entry.EnumString, enumInt = entry.EnumInt, isTable = entry.IsTable ? 1 : 0 });

        }

        public void Delete(A4O_CheckEnum entry)
        {
            AsyncAwaitUtils.Wait(() => DeleteAsync(entry));
        }

        public async Task DeleteAsync(A4O_CheckEnum e)
        {
            if (e == null) return;

            await DeleteAsync(e.ElementName, e.EnumString, e.IsTable);
        }

        public void Delete(string elementName, string enumString, bool isTable)
        {
            AsyncAwaitUtils.Wait(() => DeleteAsync(elementName, enumString, isTable));
        }
        public async Task DeleteAsync(string elementName, string enumString, bool isTable)
        {
            using var conn = Connection;
            await conn.OpenAsync();
            string sql = @"
            DELETE FROM " + SQL_CHECK_ENUM_TABLE_NAME +
            "WHERE elementName = @elementName AND enumString= @enumString AND isTable = @isTable;";
            await conn.ExecuteAsync(sql, new { elementName = elementName, enumString = enumString, isTable = isTable ? 1 : 0 });



        }

        public async Task<A4O_CheckEnum?> GetByKey(string elementName, string enumString, bool isTable)
        {
            using var conn = Connection;
            await conn.OpenAsync();
            string sql = @"
            SELECT elementName, enumString, isTable , enumInt 
            FROM " + SQL_CHECK_ENUM_TABLE_NAME +
            " WHERE elementName = @elementName AND enumString= @enumString AND isTable = @isTable;";

            var checkEnum = await conn.QueryFirstOrDefaultAsync<A4O_CheckEnum>(sql,
                new { elementName = elementName, enumString = enumString, isTable = isTable ? 1 : 0 });


            return checkEnum;
        }




        public async Task<List<A4O_CheckEnum>> GetAllAsync(string filter, Dictionary<string, object> par = null)
        {


            using var conn = Connection;
            await conn.OpenAsync();

            string sql = "SELECT elementName, enumString, isTable , enumInt " +
                "FROM A4O_CheckEnum ";
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


            var list = (await conn.QueryAsync<A4O_CheckEnum>(sql, expando)).ToList();


            return list;
        }

    }
}