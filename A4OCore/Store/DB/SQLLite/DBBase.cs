using A4OCore.Cfg;
using A4OCore.Utility;
using A4ODto;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace A4OCore.Store.DB.SQLLite
{


    public static class ExtendListDefinitionValue
    {
        public static void AddDefinitionValue(this List<DefinitionValueDto> t, params DefinitionValueDto[] values)
        {
            t.AddRange(values);

        }
    }

    public class DBBaseTableInfo
    {
        public const string SUFFIX_TABLE_VALUE = DesignElementDtoConst.SUFF_VALUES_TABLE;
        public const string SUFFIX_VIEW_VALUE = DesignElementDtoConst.SUFF_VALUES_VIEW;

        public string ElementName;
        public string ElementValuesName
        {
            get
            {
                return this.ElementName + SUFFIX_TABLE_VALUE;
            }
        }
        public string ElementValuesViewName
        {
            get
            {
                return this.ElementName + SUFFIX_VIEW_VALUE;
            }
        }
        public List<DefinitionValueDto> ValuesDefinition { get; set; }

    }
    public class DBBase : IStoreA4O
    {
        DBBaseTableInfo TableInfo { get; set; }
        ConfigurationA4O Cfg;
        A4O_MapIdRepository A4O_MapIdRepository;
        public DBBase(ConfigurationA4O cfg, A4O_MapIdRepository a4O_MapIdRepository)
        {
            this.A4O_MapIdRepository = a4O_MapIdRepository;
            this.Cfg = cfg;
        }
        public void SetTableInfo(string tableName, List<DefinitionValueDto> defValues)
        {
            DBBaseTableInfo content = new DBBaseTableInfo()
            {
                ElementName = tableName,
                ValuesDefinition = defValues ?? new List<DefinitionValueDto>()
            };

            this.TableInfo = content;

            Initialize();
        }
        SqliteConnection Connection => UtilitySqlLite.GetConnection(Cfg);
        private const string SQL_CREATE_BASE = "CREATE TABLE IF NOT EXISTS @tableName( " +
            " id INTEGER PRIMARY KEY AUTOINCREMENT, " +
            " elementNameParent  TEXT NOT NULL, " +
            " IdParent INTEGER NOT NULL, " +
            " Date TEXT, " +
            " DateChange TEXT, " +
            " Deleted INTEGER NOT NULL DEFAULT 0 " +
            " ) ;";


        private const string SQL_CREATE_VIEV = " create view @ViewName as" +
            "  select mapId.tableName ,mapId.columnName ,tv.*   " +
            " from @tableNameVal tv inner join A4O_mapId mapId   on mapId.idColumn =tv.infoData  and mapId.elementName='@tableName' ";


        private const string SQL_CREATE_VALUE =
        "CREATE TABLE IF NOT EXISTS @tableName( " +
        "    id INTEGER, " +
        "    idx INTEGER, " +
        "    infoData INTEGER, " +
        "    dateVal TEXT NULL, " +
        "    stringVal TEXT NULL, " +
        "    intVal INTEGER NULL, " +
        "    floatVal FLOAT NULL, " +
        "    PRIMARY KEY (id, infoData, idx) " +
        "    ) ;";





        private const string SQL_DELETE_VAL = " delete from @tableValues where Id in ({0});";
        private const string SQL_DELETE = " delete from @tableName where Id in ({0});" + SQL_DELETE_VAL;


        private const string SQL_REPLACE = " INSERT OR REPLACE INTO @tableName " +
        "(id, elementNameParent , IdParent, Date, DateChange, Deleted) " +
        " VALUES( @id, @elementNameParent , @IdParent, @Date, @DateChange,@Deleted);";



        private const string SQL_INSERT =
" INSERT INTO @tableName " +
" (elementNameParent , IdParent, Date, DateChange, Deleted) " +
" VALUES( @elementNameParent , @IdParent, @Date, @DateChange,@Deleted);";
        private const string SQL_INSERT_VAL_REP = " insert into @tableValues  (id ,idx ,infoData ,dateVal ,stringVal ,intVal ,floatVal ) values ";
        private const string SQL_INSERT_VAL_PARAMS_REPL = "(@SUFF_id ,@SUFF_idx ,@SUFF_infoData ,@SUFF_dateVal ,@SUFF_stringVal ,@SUFF_intVal ,@SUFF_floatVal)";
        private const string SQL_INSERT_VAL_PARAMS_INS = "(@SUFF_id,@SUFF_idx ,@SUFF_infoData ,@SUFF_dateVal ,@SUFF_stringVal ,@SUFF_intVal ,@SUFF_floatVal)";
        private const string SQL_LAST_INSERT_AUTO = "select last_insert_rowid()";
        private const int SQL_MAX_NUM_INSERT_VAL = 100;

        public bool ExistsTable()
        {
            return ExistsTable(TableInfo.ElementName);
        }
        public void ExecuteQuery(string sql, Action<SqliteDataReader> callback, IEnumerable<KeyValuePair<string, object>> par = null)
        {
            ExecuteQuery(this.Connection, sql, callback, par);
        }
        public static void ExecuteQuery(SqliteConnection conn, string sql, Action<SqliteDataReader> callback, IEnumerable<KeyValuePair<string, object>> par = null)
        {
            using (var connection = conn)
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    if (par != null)
                    {
                        foreach (var kvp in par)
                        {
                            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        }
                    }

                    SqliteDataReader r = command.ExecuteReader();
                    callback(r);

                }

            }
        }


        public bool ExistsTable(string tableName)
        {
            return ExistsTable(this.Connection, tableName);
        }
        public static bool ExistsTable(SqliteConnection conn, string tableName)
        {
            using (var connection = conn)
            {
                connection.Open();
                var sql = "SELECT name FROM sqlite_master " +
                    "WHERE type='table' AND name=@t";
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@t", tableName);
                    var r = command.ExecuteReader();
                    return r.Read();

                }

            }
        }

        public bool ExistsView(string viewName)
        {
            return ExistsView(this.Connection, viewName);
        }
        public static bool ExistsView(SqliteConnection conn, string viewName)
        {
            using (var connection = conn)
            {
                connection.Open();
                var sql = " SELECT name FROM sqlite_master " +
                          " WHERE type = 'view' AND name =@v ";
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@v", viewName);
                    SqliteDataReader r = command.ExecuteReader();
                    return r.Read();
                }
            }
        }


        public List<ElementA4ODto> Load(FilterA4O filter)
        {

            return Task.Run(() => this.LoadAsync(filter)).GetAwaiter().GetResult();
        }

        public async Task<List<ElementA4ODto>> LoadAsync(FilterA4O filter)
        {
            FilterSqlLiteManager filterSqlLiteManager = new FilterSqlLiteManager(this.TableInfo.ElementName, this.TableInfo.ElementValuesName ,filter);
            string sql = filterSqlLiteManager.GetSql();
            var r = await LoadByFilter(sql, filterSqlLiteManager.Parameters);
            return r;
        }

        public async Task<List<ElementA4ODto>> LoadByFilter(string sql = null, List<SqliteParameter> par = null)
        {
            using (var connection = this.Connection)
            {
                connection.Open();

                List<ElementA4ODto> res = new List<ElementA4ODto>();
                List<int> ids = new List<int>();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    if (par != null)
                    {
                        command.Parameters.AddRange(par);

                    }
#if DEBUG
                    Debug.WriteLine(sql);
#endif
                    SqliteDataReader r = await command.ExecuteReaderAsync();
                    ElementA4ODto toAdd = new ElementA4ODto();
                    while (await r.ReadAsync())
                    {

                        if (toAdd.Id != (long)r[POS_MAIN_ID]) // first ID
                        {
                            toAdd = MapReaderToElementDB(r);
                            toAdd.Values = new List<ElementValueA4ODto>();
                            res.Add(toAdd);
                        }
                        var val = MapReaderToValuesElementDB(r);
                        if (val != null) toAdd.Values.Add(val);
                    }
                }
                return res;
            }


        }

        private ElementValueA4ODto? MapReaderToValuesElementDB(SqliteDataReader r)
        {
            if (r.IsDBNull("id")) return null;
            var res = new ElementValueA4ODto();
            res.Id = r.GetInt32("id");
            res.Idx = r.GetInt32("idx");
            res.InfoData = r.GetInt32("infoData");
            string tmpDate = r["dateVal"] as string;


            if (DateTime.TryParse(tmpDate, out DateTime dt))
            {
                res.DateVal = dt;
            }
            else
            {
                res.DateVal = null;

            }

            res.StringVal = r["stringVal"] as string;
            res.IntVal = r["intVal"] as long?;
            res.FloatVal = r["floatVal"] as double?;
            return res;
        }

        private const int POS_MAIN_ID = 0;
        private static ElementA4ODto MapReaderToElementDB(SqliteDataReader r)
        {

            ElementA4ODto res = new ElementA4ODto();
            res.Id = r.GetInt32(POS_MAIN_ID); //
            res.ElementNameParent = r.GetString("elementNameParent");
            res.IdParent = r.GetInt32("IdParent");

            res.Date = r.GetDateTime("Date");
            res.DateChange = r.GetDateTime("DateChange");
            res.Deleted = r.GetInt32("Deleted") == 1;
            return res;


        }








        public async Task DeleteAsync(params long[] ids)
        {

            List<(string sql, IDictionary<string, object> par)> queries = new List<(string sql, IDictionary<string, object> par)>();
            string sql = SQL_DELETE.Replace("@tableName", this.TableInfo.ElementName);
            sql = sql.Replace("@tableValues", this.TableInfo.ElementValuesName);
            sql = sql.Replace("{0}", string.Join(",", ids));
            queries.Add((sql, null));
            await ExecuteInTransaction(queries);

        }

        public void Save(params ElementA4ODto[] o)
        {
            Task.Run(() => SaveAsync(o)).GetAwaiter().GetResult();
        }

        public async Task SaveAsync(params ElementA4ODto[] o)
        {
            foreach (ElementA4ODto el in o)
            {
                if (el == null) continue;
                if (el.Id <= 0)
                {
                    await InsertSingle(el);
                }
                else
                {
                    await ReplaceSingle(el);
                }
            }
        }

        private async Task InsertSingle(ElementA4ODto el)
        {
            if (el == null) return;



            long newId = -1;
            await ExecuteInTransaction(
                async (cmd) =>
                {
                    await this.InsertElement(el, cmd);

                },

                async cmd =>
                {
                    cmd.CommandText = SQL_LAST_INSERT_AUTO;
                    object r = await cmd.ExecuteScalarAsync();
                    newId = r as long? ?? -1;
                },

                async cmd =>
                {
                    cmd.CommandText = SQL_DELETE_VAL.Replace("@tableValues", this.TableInfo.ElementValuesName).Replace("{0}", "" + newId);
                    await cmd.ExecuteNonQueryAsync();
                },

                async cmd =>
                {
                    await InsertValues(el.Values, cmd, newId);
                }


                );
            el.Id = newId;
            foreach (var item in el.Values)
            {
                item.Id = newId;
            }

        }

        private async Task InsertValues(IEnumerable<ElementValueA4ODto> els, SqliteCommand cmd, long idElement)
        {

            var elementsToInset = els.Take(SQL_MAX_NUM_INSERT_VAL);
            var othersValues = els.Skip(SQL_MAX_NUM_INSERT_VAL);

            string sqlVal = "";
            string sql = SQL_INSERT_VAL_REP.Replace("@tableValues", this.TableInfo.ElementValuesName).Replace("{0}", idElement.ToString());
            Dictionary<string, object> parVal = new Dictionary<string, object>();
            int idx = 0;

            foreach (var val in elementsToInset)
            {
                if (!FilterElementValueA4O(val)) break;
                var suff = "@VAL" + (++idx) + "_";
                string newVal = SQL_INSERT_VAL_PARAMS_INS.Replace("@SUFF_", suff);
                sqlVal = sqlVal + "," + newVal;
                parVal.Add(suff + "id", idElement);
                parVal.Add(suff + "idx", val.Idx);
                parVal.Add(suff + "infoData", val.InfoData);
                parVal.Add(suff + "dateVal", (object?)val.DateVal ?? DBNull.Value);
                parVal.Add(suff + "stringVal", (object?)val.StringVal ?? DBNull.Value);
                parVal.Add(suff + "intVal", (object?)val.IntVal ?? DBNull.Value);
                parVal.Add(suff + "floatVal", (object?)val.FloatVal ?? DBNull.Value);
            }
            if (sqlVal.Length == 0)
            {
                return;
            }
            sqlVal = sqlVal.Substring(1);
            sql += sqlVal;
            cmd.CommandText = sql;
            foreach (var param in parVal)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            await cmd.ExecuteNonQueryAsync();


            if (othersValues.Count() == 0) return;
            cmd.Parameters.Clear();
            cmd.CommandText = "";

            await InsertValues(othersValues, cmd, idElement);

        }

        private async Task InsertElement(ElementA4ODto el, SqliteCommand cmd)
        {
            string sql = SQL_INSERT.Replace("@tableName", this.TableInfo.ElementName);
            Dictionary<string, object> parEl = new Dictionary<string, object>();
            parEl.Add("@elementNameParent", el.ElementNameParent);
            parEl.Add("@IdParent", el.IdParent);
            parEl.Add("@Date", el.Date);
            parEl.Add("@DateChange", DateTime.Now);
            parEl.Add("@Deleted", el.Deleted);

            cmd.CommandText = sql;

            foreach (var param in parEl)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            await cmd.ExecuteNonQueryAsync();
        }


        private async Task ReplaceSingle(ElementA4ODto el)
        {





            if (el == null) return;

            ExecuteInTransaction(
                async (cmd) =>
                {
                    await this.ReplaceElement(el, cmd);

                },

                async cmd =>
                {
                    await InsertValues(el.Values, cmd, el.Id);
                }

                );
        }

        private async Task ReplaceElement(ElementA4ODto el, SqliteCommand cmd)
        {
            string sql = SQL_REPLACE.Replace("@tableName", this.TableInfo.ElementName);
            Dictionary<string, object> par = new Dictionary<string, object>();
            cmd.Parameters.AddWithValue("@id", el.Id);
            cmd.Parameters.AddWithValue("@elementNameParent", el.ElementNameParent);
            cmd.Parameters.AddWithValue("@IdParent", el.IdParent);
            cmd.Parameters.AddWithValue("@Date", el.Date);
            cmd.Parameters.AddWithValue("@DateChange", DateTime.Now);
            cmd.Parameters.AddWithValue("@Deleted", el.Deleted);
            sql += SQL_DELETE_VAL.Replace("@tableValues", this.TableInfo.ElementValuesName).Replace("{0}", el.Id.ToString());
            cmd.CommandText = sql;


            await cmd.ExecuteNonQueryAsync();
        }

        public void ExecuteNoQuery(string sql, IDictionary<string, object> par)
        {
            ExecuteNoQuery(this.Connection, sql, par);
        }
        public static void ExecuteNoQuery(SqliteConnection conn, string sql, IDictionary<string, object> par)
        {
            List<(string sql, IDictionary<string, object> par)> qry = new List<(string sql, IDictionary<string, object> par)>();
            qry.Add((sql, par));
            ExecuteInTransaction(conn, qry);
        }



        public async Task ExecuteInTransaction(params Func<SqliteCommand, Task>[] toEcecute)
        {
            await ExecuteInTransaction(this.Connection, toEcecute);
        }
        public static async Task ExecuteInTransaction(SqliteConnection liteConn, params Func<SqliteCommand, Task>[] toEcecute)
        {
            using var conn = liteConn;
            await conn.OpenAsync();

            using (var transaction = await conn.BeginTransactionAsync())
            {
                using var cmd = conn.CreateCommand();

                cmd.Transaction = (SqliteTransaction?)transaction;
                foreach (var query in toEcecute)
                {
                    if (query == null) continue;
                    cmd.Parameters.Clear();
                    // Prima query
                    cmd.CommandText = "";
                    List<Task> waitTask = new List<Task>();
                    try
                    {

                        await query(cmd);

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                await transaction.CommitAsync();
            }
        }

        public async Task ExecuteInTransaction(IEnumerable<(string sql, IDictionary<string, object> par)> queries)
        {
            await ExecuteInTransaction(this.Connection, queries);
        }

        public static async Task ExecuteInTransaction(SqliteConnection conn, IEnumerable<(string sql, IDictionary<string, object> par)> queries)
        {


            var f = queries.Select(query =>

            {
                Func<SqliteCommand, Task> r = async (cmd) =>
                 {
                     cmd.CommandText = query.sql;
                     if (query.par != null)
                     {
                         foreach (var param in query.par)
                         {
                             cmd.Parameters.AddWithValue(param.Key, param.Value);
                         }
                     }
                     await cmd.ExecuteNonQueryAsync();
                 };
                return r;
            });

            await ExecuteInTransaction(conn, f.ToArray());


        }



        //using var conn = DBBase.GetConnection;
        //conn.Open();

        //using var transaction = conn.BeginTransaction();
        //using var cmd = conn.CreateCommand();

        //cmd.Transaction = transaction;
        //foreach (var query in queries)
        //{
        //    cmd.Parameters.Clear();
        //    // Prima query
        //    cmd.CommandText = query.sql;
        //    if (query.par != null)
        //    {
        //        foreach (var param in query.par)
        //        {
        //            cmd.Parameters.AddWithValue(param.Key, param.Value);
        //        }
        //    }
        //    try
        //    {
        //        cmd.ExecuteNonQuery();

        //    }
        //    catch (Exception ex)
        //    {
        //        transaction.Rollback();
        //        throw;
        //    }
        //}
        //transaction.Commit();




        private static SortedSet<string> AlreadyInitialized = new SortedSet<string>();
        public void Initialize()
        {

            if (Cfg.Enviroment != A4OCore.Cfg.EnviromentEnum.UnitTest && !AlreadyInitialized.Add(TableInfo.ElementName)) return;
            var ex = this.ExistsTable();
            var exVal = ExistsTable(TableInfo.ElementValuesName);
            var exMapId = ExistsTable(A4O_MapIdRepository.SQL_MAPID_TABLE_NAME);

            var exView = ExistsView(TableInfo.ElementValuesViewName);


            List<(string sql, IDictionary<string, object> par)> queries = new List<(string sql, IDictionary<string, object> par)>();

            if (!ex)
            {
                queries.Add((DBBase.SQL_CREATE_BASE.Replace("@tableName", TableInfo.ElementName), null));
            }
            if (!exVal)
            {
                queries.Add((DBBase.SQL_CREATE_VALUE.Replace("@tableName", TableInfo.ElementValuesName), null));
            }
            if (!exMapId)
            {
                queries.Add((A4O_MapIdRepository.SQL_CREATE_MAPID, null));

            }
            if (!ExistsTable(A4O_User.SQL_USER_TABLE_NAME))
            {
                queries.Add((A4O_User.SQL_CREATE_USER, null));
            }

            if (!exView)
            {
                queries.Add((DBBase.SQL_CREATE_VIEV
                        .Replace("@tableNameVal", TableInfo.ElementValuesName)
                        .Replace("@tableName", TableInfo.ElementName)
                        .Replace("@ViewName", TableInfo.ElementValuesViewName), null));
            }

            AsyncAwaitUtils.Wait(() => ExecuteInTransaction(queries));
            //Task.Run(()=> DBBase.ExecuteInTransaction(queries)).GetAwaiter().GetResult();
            if (this.TableInfo.ValuesDefinition == null) return;

            //A4O_MapIdRepository a4O_MapIdRepository = new A4O_MapIdRepository();

            foreach (var rowDes in this.TableInfo.ValuesDefinition)
            {
                A4O_MapIdRepository.Delete(this.TableInfo.ElementName, rowDes.TableName, rowDes.ColumnName);
                A4O_MapIdRepository.Insert(new A4O_MapId()
                {
                    ElementName = this.TableInfo.ElementName,
                    TableName = rowDes.TableName,
                    ColumnName = rowDes.ColumnName,
                    IdColumn = rowDes.IdColumn
                });

            }

        }

        public async Task DeleteAsync(params ElementA4ODto[] rem)
        {
            await DeleteAsync(rem.Select(x => x.Id).ToArray());
        }

        public bool FilterElementValueA4O(ElementValueA4ODto e)
        {
            if (
            e.IntVal == null &&
            e.StringVal == null &&
            e.FloatVal == null &&
            e.DateVal == null
            ) return false;
            return TableInfo.ValuesDefinition.Any(x => e.InfoData == x.IdColumn);
        }


    }
}