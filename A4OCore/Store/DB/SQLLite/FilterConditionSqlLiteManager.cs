using Microsoft.Data.Sqlite;
using System.Collections;

namespace A4OCore.Store.DB.SQLLite
{
    public static class FilterConditionSqlLiteManager
    {



        private static string ToSqlComposite(FilterBase filterBase, List<SqliteParameter> parameters)
        {
            FilterComposite fc = filterBase as FilterComposite;
            if (fc != null)
            {
                var sep = fc.Combinatore == Combinator.And ? " AND " : " OR ";
                return "(" + string.Join(sep, fc.Children.Select(f => GenerateSqlFilter(f, parameters))) + ")";
            }
            return null;
        }



        public static string GenerateSqlFilter(FilterBase filterBase, List<SqliteParameter> parameters)
        {
            string res = ToSqlComposite(filterBase, parameters);
            if (res != null) return res;
            res = ToSqlCondition(filterBase, parameters);
            if (res != null) return res;
            return null;
        }
        private static string ToSqlCondition(FilterBase filterBase, List<SqliteParameter> parameters)
        {
            FilterCondition fc = filterBase as FilterCondition;
            if (fc == null) return null;



            if (fc.Field.Contains("@@par@@")) throw new Exception("wrong parameter name!!!!");
            string prefixPar = "@" + fc.Field;

            var sql = fc.Operator switch
            {
                Operator.Equal => $"{fc.Field} = @@par@@ ",
                Operator.NotEqual => $"{fc.Field} <> @@par@@",
                Operator.GreaterThan => $"{fc.Field}> @@par@@",
                Operator.LessThan => $"{fc.Field}< @@par@@",
                Operator.Like => $"{fc.Field} LIKE '%' +@@par@@+ '%'",
                Operator.EndWith => $"{fc.Field} LIKE '%' +@@par@@",
                Operator.StartWith => $"{fc.Field} LIKE @@par@@+ '%'",
                Operator.In => $"{fc.Field} in (@@par@@)",
                Operator.NotIn => $"{fc.Field} not in (@@par@@)",
                Operator.GreaterEqThan => $"{fc.Field} >=@@par@@",
                Operator.LessEqThan => $"{fc.Field} >=@@par@@",
                Operator.Not => $"not {fc.Field}",
                Operator.IsVoid => $"{fc.Field} is null",
                Operator.NoOperator => $"({fc.Field})",
                Operator.IsNotVoid => $"not {fc.Field} is null",
                _ => throw new NotSupportedException()
            };



            switch (fc.Operator)
            {

                case Operator.NotIn:
                case Operator.In:
                    {
                        var v = ((IEnumerable)fc.Value).Cast<object>();
                        if (v.Count() == 0)
                        {
                            sql = "1=0";
                            break;
                        }
                        int idx = 1;
                        string allPar = "";
                        foreach (var o in v)
                        {
                            string parName = prefixPar + idx++;
                            parName = UtilitySqlLite.GetParamName(parName, parameters);
                            parameters.Add(new SqliteParameter(parName, o));
                            allPar += "," + parName;

                        }
                        sql = sql.Replace("@@par@@", allPar.Substring(1));


                        break;
                    }

                case Operator.Not:
                case Operator.IsVoid:
                case Operator.NoOperator:
                case Operator.IsNotVoid:
                    break;
                default:
                    {
                        string parName = "";

                        parName = UtilitySqlLite.GetParamName(prefixPar, parameters);
                        parameters.Add(new SqliteParameter(parName, fc.Value));
                        sql = sql.Replace("@@par@@", parName);
                        break;
                    }

            }

            return sql;

        }


    }

    internal class FilterConditionSqlLite : FilterCondition
    {
        public FilterConditionSqlLite(string campo, Operator operatore, object valore = null) : base(campo, operatore, valore)
        {
        }
        public FilterConditionSqlLite FromFilter(FilterCondition filter)
        {


            FilterConditionSqlLite res = new FilterConditionSqlLite(filter.Field, filter.Operator, filter.Value);
            return res;

        }


    }
}

