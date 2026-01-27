using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace A4OCore.Store.DB.SQLLite
{
    internal class FilterSqlLiteManager
    {

        public FilterSqlLiteManager(string mainTableName , string valueTableName ,FilterA4O filter)
        {
            Filter = filter;
            MainTableName = mainTableName;
            ValueTableName = valueTableName;
        }
        FilterA4O Filter;
        readonly string MainTableName;
        readonly string ValueTableName;

        public List<SqliteParameter> Parameters { get; } = new List<SqliteParameter>();

        private string GetQueryFilter(string mainTableName, string valueTableName)
        {
            string sqlFilter = " SELECT distinct main.id FROM " + mainTableName + " as main " +
        " left join " + valueTableName + " as  val " +
        " on val.id=main.id ";
            sqlFilter = SqlAddJoinFilter(sqlFilter, valueTableName);
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

        private string SqlAddJoinFilter(string sqlFilter, string valueTableName)
        {
            if ((this.Filter?.SimpleFilterConditions?.Count ??0)==0) return sqlFilter;
            StringBuilder sb= new StringBuilder();
            int i = -1;
            var sFilters = this.Filter?.SimpleFilterConditions!;
            int max = sFilters.Count;
            while(++i<max)
            {
                string alias = $"valCond{i}";
                string paramBase = $"@valCondPar{i}";
                sb.AppendLine($" left join {valueTableName} as  {alias} " +
                          $" on {alias}.id=main.id  ");
                string condition= GetQueryFromSimpleFilterConditions(alias , paramBase, sFilters[i]);
                sb.AppendLine(condition);
            }
            return sqlFilter + sb.ToString();
        }

        private string GetQueryFromSimpleFilterConditions(string alias, string paramBase, SimpleFilterCondition simpleFilterCondition)
        {
            var designItem = this.Filter.Design.ItemsDesignBase.First(x => x.IdElement == (int)((object)simpleFilterCondition.Field));
            A4ODto.ValueDesignType desType = designItem.DesignType;
            var condition = GetQueryFromSimpleFilterConditions(alias, paramBase, simpleFilterCondition, desType);
            string result = $" and {alias}.infoData={designItem.InfoData}  {condition} ";
            return result ; 
        }
        private static string GetQueryFromSimpleFilterConditions(string alias,string paramBase, SimpleFilterCondition simpleFilterCondition, A4ODto.ValueDesignType  designType)
        {
            string valueType = "";
            switch (designType)
            {
                case A4ODto.ValueDesignType.FLOAT:
                    valueType = "floatVal";
                    break;

                case A4ODto.ValueDesignType.INT:
                valueType = "intVal";
                    break;
                case A4ODto.ValueDesignType.DATE:
                    valueType = "dateVal";
                    break;
                case A4ODto.ValueDesignType.STRING:
                case A4ODto.ValueDesignType.COMMENT:
                case A4ODto.ValueDesignType.OPTIONSET:
                case A4ODto.ValueDesignType.ATTACHMENT:
                    valueType = "stringVal";
                    break;
                case A4ODto.ValueDesignType.LINK :
                    if(simpleFilterCondition.Value is string)
                    {
                        valueType = "stringVal";
                    }
                    else
                    {
                        valueType = "intVal";
                    }
                    break;

        //case A4ODto.ValueDesignType.VIEW = 7,
        //case A4ODto.ValueDesignType.CALCULATE = 8,
        //case A4ODto.ValueDesignType.BUTTON = 10,
        //case A4ODto.ValueDesignType.ACTION = 12,
        //case A4ODto.ValueDesignType.CRIPTED = 9,
                default:
                    throw new NotImplementedException();
            }
            return GetQueryFromSimpleFilterConditions($"{alias}.{valueType}", simpleFilterCondition, designType);
                //case A4ODto.ValueDesignType.VIEW = 7,
                //case A4ODto.ValueDesignType.CALCULATE = 8,
                //case A4ODto.ValueDesignType.BUTTON = 10,
                //case A4ODto.ValueDesignType.ACTION = 12,
                //case A4ODto.ValueDesignType.CRIPTED = 9,


        }
        private static string GetQueryFromSimpleFilterConditions(
    string paramName,
    SimpleFilterCondition simpleFilterCondition,
    A4ODto.ValueDesignType designType)
        {
            if (simpleFilterCondition == null)
                throw new ArgumentNullException(nameof(simpleFilterCondition));

            string column = $"{paramName}";
            object value = simpleFilterCondition.Value;
            Operator op = simpleFilterCondition.Operator;

            // 1️⃣ Operator che NON vogliono value
            if (op == Operator.IsVoid)
                return $"AND {column} IS NULL";

            if (op == Operator.IsNotVoid)
                return $"AND {column} IS NOT NULL";

            // 2️⃣ Tutti gli altri vogliono value
            if (value == null)
                throw new ArgumentException($"Operator {op} richiede un valore");

            // 3️⃣ IN / NOT IN
            if (op == Operator.In || op == Operator.NotIn)
            {
                if (!(value is System.Collections.IEnumerable enumerable) || value is string)
                    throw new ArgumentException($"{op} richiede una IEnumerable");

                var values = new List<string>();

                foreach (var v in enumerable)
                    values.Add(FormatValue(v, designType));

                if (!values.Any())
                    throw new ArgumentException($"{op} non può essere vuoto");

                string sqlOp = op == Operator.In ? "IN" : "NOT IN";
                return $"AND {column} {sqlOp} ({string.Join(",", values)})";
            }

            // 4️⃣ LIKE / START / END (solo stringhe)
            if (op == Operator.Like || op == Operator.StartWith || op == Operator.EndWith)
            {
                if (designType != A4ODto.ValueDesignType.STRING &&
                    designType != A4ODto.ValueDesignType.COMMENT)
                    throw new ArgumentException($"{op} è valido solo per stringhe");

                string strVal = value.ToString().Replace("'", "''");

                string pattern = op switch
                {
                    Operator.Like => $"%{strVal}%",
                    Operator.StartWith => $"{strVal}%",
                    Operator.EndWith => $"%{strVal}",
                    _ => throw new NotSupportedException()
                };

                return $"AND {column} LIKE '{pattern}'";
            }

            // 5️⃣ Operator standard (=, <, >, ecc.)
            string sqlOperator = op switch
            {
                Operator.Equal => "=",
                Operator.NotEqual => "<>",
                Operator.GreaterThan => ">",
                Operator.LessThan => "<",
                Operator.GreaterEqThan => ">=",
                Operator.LessEqThan => "<=",
                _ => throw new NotSupportedException($"Operator {op} non supportato")
            };

            return $"AND {column} {sqlOperator} {FormatValue(value, designType)}";
        }
        private static string FormatValue(object value, A4ODto.ValueDesignType designType)
        {
            return designType switch
            {
                A4ODto.ValueDesignType.INT =>
                    Convert.ToInt32(value).ToString(),

                A4ODto.ValueDesignType.FLOAT =>
                    Convert.ToDecimal(value).ToString(System.Globalization.CultureInfo.InvariantCulture),

                A4ODto.ValueDesignType.DATE =>
                    $"'{Convert.ToDateTime(value):yyyy-MM-dd HH:mm:ss}'",

                A4ODto.ValueDesignType.STRING or A4ODto.ValueDesignType.COMMENT =>
                    $"'{value.ToString().Replace("'", "''")}'",

                _ => throw new NotImplementedException($"DesignType {designType} non supportato")
            };
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
            if((Filter.CustomFilter?.Count??0) > 0)
            {
                int i = 0;
                foreach (var serFilter in Filter.CustomFilter)
                {
                    i++;
                    CustomFilterClass? customFilter = JsonSerializer.Deserialize<CustomFilterClass>(serFilter);
                    switch (customFilter.CustomFilterName)
                    {
                        case CustomTypeQuery.Last_N_with_Parent:
                        {
                                var g = Sql4LAST_N_WITH_PARENT(i, MainTableName, customFilter);
                                where.Add(g.sql);
                                Parameters.AddRange(g.par);
                            break;
                        }
                    }
                }
            }

            sqlFilter +=
                (where.Count() == 0 ? "" : " where ( " + string.Join(" and ", where) + " ) ");
            return sqlFilter;
        }
        private (string sql , IEnumerable< SqliteParameter> par) Sql4LAST_N_WITH_PARENT(int idxCustom,string mainTableName ,CustomFilterClass customFilter)
        {
            string sql = @$"main.id in ( (
    SELECT id from (select
        p{idxCustom}.id,
        ROW_NUMBER() OVER (
            PARTITION BY p{idxCustom}.idParent
            ORDER BY p{idxCustom}.[date] DESC
        ) AS rn
    FROM {mainTableName} p{idxCustom}
    WHERE p{idxCustom}.idParent IN ({string.Join(',', customFilter.IdParents)})   -- i tuoi N idParent
) as t{idxCustom} 
WHERE t{idxCustom}.rn <= @customParam{idxCustom} ))";
            SqliteParameter sqliteParameter = new SqliteParameter($"@customParam{idxCustom}", customFilter.N4Parent);

            return (sql, [sqliteParameter]);
            //select id from(SELECT
        
            //    p1.id,
            //    ROW_NUMBER() OVER(
            //        PARTITION BY p1.idParent
        
            //        ORDER BY p1.[date] DESC
            //    ) AS rn
        
            //FROM TEST p1
        
            //WHERE p1.idParent IN(1, 1, 1)) as t2   where t2.rn <= 2-- i tuoi N idParent

        }

        
        public string GetSql()
        {
            string sql = "";

            if (Filter == null)
            {
                sql = "SELECT m.*,v.* FROM " + MainTableName + " m " +
                " left join " + ValueTableName + " v on m.id = v.id";
                return sql;
            }

            string sqlResult = "SELECT t.*,tv.* FROM " + MainTableName + " t " +
                " left join " + ValueTableName + " tv on t.id = tv.id";

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
                + GetQueryFilter(MainTableName, ValueTableName)
                + ") " + sqlResult;
            return sql;

        }


    }
}
