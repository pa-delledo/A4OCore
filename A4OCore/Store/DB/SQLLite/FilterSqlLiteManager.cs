using Microsoft.Data.Sqlite;

namespace A4OCore.Store.DB.SQLLite
{
    internal class FilterSqlLiteManager
    {

        public FilterSqlLiteManager(FilterA4O filter)
        {
            Filter = filter;
        }
        FilterA4O Filter;

        public List<SqliteParameter> Parameters { get; } = new List<SqliteParameter>();

        private String GetQueryFilter(string mainTableName, string valueTableName)
        {
            string sqlFilter = " SELECT distinct main.id FROM " + mainTableName + " as main " +
        " left join " + valueTableName + " as  val " +
        " on val.id=main.id ";
            sqlFilter = SqlAddWhereFilter(sqlFilter);

            if (Filter.OrderBy.HasValue)
            {
                sqlFilter += " order by main." + Filter.OrderBy.Value.ToString() + " ";
                sqlFilter += Filter.Ascending ? "" : " desc ";
            }

            if (Filter.Top.HasValue)
            {

                string parNameLim = UtilitySqlLite.GetParamName("@limit", this.Parameters);
                sqlFilter += " LIMIT " + parNameLim + " ";
                this.Parameters.Add(new SqliteParameter(parNameLim, Filter.Top.Value));

                if (Filter.Offset.HasValue)
                {
                    string parName = UtilitySqlLite.GetParamName("@offSet", this.Parameters);
                    sqlFilter += " OFFSET " + parName + " ";
                    this.Parameters.Add(new SqliteParameter(parName, Filter.Offset.Value));
                }
            }
            return sqlFilter;

        }
        private const string FORMAT_DATE_SQL = "yyyy-MM-dd HH:mm:ss";
        private string SqlAddWhereFilter(string sqlFilter)
        {
            List<string> where = new List<string>();
            if (Filter.Ids != null)
            {
                where.Add("(" +
                    (Filter.InvertSelectionIds ? " not " : " ") +
                    (Filter.Ids.Length == 0 ? " 1=0 " : $" main.id in ({string.Join(',', Filter.Ids)})"
                    ) +
                    ") ");
            }
            if (Filter.ParentName != null)
            {
                where.Add("(" +
                 (Filter.ParentName.Length == 0 ? " 1=0 " : $" main.id in ('{string.Join("','", Filter.ParentName.Select(x => x?.Replace("'", "''")))}')"
                 ) +
                 ") ");

            }
            if (Filter.ParentIds != null)
            {
                where.Add("(" +
                 (Filter.InvertSelectionParentIds ? " not " : " ") +
                 (Filter.ParentIds.Length == 0 ? " 1=0 " : $" main.idParent in ({string.Join(",", Filter.ParentIds)})"
                 ) +
                 ") ");
            }


            if (Filter.Deleted.HasValue)
            {
                where.Add(" ( main.deleted =  " + (Filter.Deleted.Value ? "1" : "0") + ") ");

            }


            if (Filter.Date.HasValue)
            {

                if (Filter.Date.Value.to.HasValue)
                {
                    string parName = UtilitySqlLite.GetParamName("@dateTo", this.Parameters);
                    where.Add("(" +
                        (Filter.InvertSelectionDate ? " not " : " ") +
                         " main.Date <= " + parName
                        + " ) ");
                    this.Parameters.Add(new SqliteParameter(parName, Filter.Date.Value.to.Value.ToString(FORMAT_DATE_SQL)));
                }
                if (Filter.Date.Value.from.HasValue)
                {
                    string parName = UtilitySqlLite.GetParamName("@dateFrom", this.Parameters);
                    where.Add("(" +
                        (Filter.InvertSelectionDate ? " not " : " ") +
                         " main.Date >= " + parName
                        + " ) ");
                    this.Parameters.Add(new SqliteParameter(parName, Filter.Date.Value.from.Value.ToString(FORMAT_DATE_SQL)));
                }
            }
            if (Filter.DateChange.HasValue)
            {

                if (Filter.DateChange.Value.to.HasValue)
                {
                    string parName = UtilitySqlLite.GetParamName("@dateChangeTo", this.Parameters);
                    where.Add("(" +
                        (Filter.InvertSelectionDateChange ? " not " : " ") +
                         " main.DateChange <= " + parName
                        + " ) ");
                    this.Parameters.Add(new SqliteParameter(parName, Filter.DateChange.Value.to.Value.ToString(FORMAT_DATE_SQL)));
                }
                if (Filter.DateChange.Value.from.HasValue)
                {
                    string parName = UtilitySqlLite.GetParamName("@dateChangeFrom", this.Parameters);
                    where.Add("(" +
                        (Filter.InvertSelectionDateChange ? " not " : " ") +
                         " main.DateChange >= " + parName
                        + " ) ");
                    this.Parameters.Add(new SqliteParameter(parName, Filter.DateChange.Value.from.Value.ToString(FORMAT_DATE_SQL)));
                }
            }
            sqlFilter +=
                (where.Count() == 0 ? "" : " where ( " + string.Join(" and ", where) + " ) ");
            return sqlFilter;
        }

        public string GetSql(string mainTableName, string valueTableName)
        {
            string sql = "";

            if (Filter == null)
            {
                sql = "SELECT m.*,v.* FROM " + mainTableName + " m " +
                " left join " + valueTableName + " v on m.id = v.id";
                return sql;
            }

            string sqlResult = "SELECT t.*,tv.* FROM " + mainTableName + " t " +
                " left join " + valueTableName + " tv on t.id = tv.id";

            if (Filter.ResultValues != null)
            {

                sqlResult += " and tv.infoData in(" +
                string.Join(",", Filter.ResultValues) +
                ")";

            }


            sqlResult += " WHERE t.ID IN (SELECT id FROM Filtered)";



            if (Filter.OrderBy.HasValue)
            {
                sqlResult += " order by t." + Filter.OrderBy.Value.ToString() + " ";
                sqlResult += Filter.Ascending ? "" : " desc ";
                sqlResult += " , t.id ";
            }
            else
            {
                sqlResult += " order by t.id ";
            }
            sqlResult += ";";

            sql = "WITH Filtered AS ("
                + GetQueryFilter(mainTableName, valueTableName)
                + ") " + sqlResult;
            return sql;

        }


    }
}
