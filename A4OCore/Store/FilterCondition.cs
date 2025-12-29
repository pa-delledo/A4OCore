using System.Collections;

namespace A4OCore.Store
{
    public enum Combinator
    {
        And,
        Or
    }

    public enum Operator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterEqThan,
        LessEqThan,
        Like,
        StartWith,
        EndWith,
        In,
        NotIn,
        Not,
        IsVoid,
        IsNotVoid,
        NoOperator
    }
    public class FilterCondition : FilterBase
    {
        public string Field { get; }
        public Operator Operator { get; }
        public object Value { get; }

        public FilterCondition(string campo, Operator operatore, object valore = null)
        {
            Field = campo;
            Operator = operatore;
            Value = valore;

            CheckFilter();

        }

        private void CheckFilter()
        {
            switch (this.Operator)
            {
                case Operator.IsVoid:
                case Operator.Not:
                case Operator.NoOperator:
                case Operator.IsNotVoid:
                    if (Value != null)
                    {
                        throw new Exception("unary operator");
                    }
                    break;
                case Operator.Like:
                case Operator.StartWith:
                case Operator.EndWith:
                    if (Value == null)
                    {
                        throw new Exception("value cannot be void");
                    }
                    if (!(Value is string))
                    {
                        throw new Exception("value must be string");
                    }
                    break;
                case Operator.In:
                case Operator.NotIn:
                    if (Value == null)
                    {
                        throw new Exception("value cannot be void");
                    }
                    if (!((Value is IEnumerable<int>) ||
                        (Value is IEnumerable<string>) ||
                        (Value is IEnumerable<float>) ||
                        (Value is IEnumerable<DateTime>) ||
                        (Value is IEnumerable<double>)
                        ))
                    {
                        throw new Exception("value must be a enumerable");
                    }

                    break;


            }
        }
    }
    public class FilterBase
    {

        public static FilterBase Filter(string campo, Operator op, object valore) =>
                new FilterCondition(campo, op, valore);

        public static FilterBase Equal(string campo, object valore) =>
            new FilterCondition(campo, Operator.Equal, valore);
        public static FilterBase NotEqual(string campo, object valore) =>
            new FilterCondition(campo, Operator.NotEqual, valore);

        public static FilterBase GreaterThan(string campo, object valore) =>
            new FilterCondition(campo, Operator.GreaterThan, valore);
        public static FilterBase GreaterEqThan(string campo, object valore) =>
            new FilterCondition(campo, Operator.GreaterEqThan, valore);

        public static FilterBase LessThan(string campo, object valore) =>
            new FilterCondition(campo, Operator.LessThan, valore);
        public static FilterBase LessEqThan(string campo, object valore) =>
            new FilterCondition(campo, Operator.LessEqThan, valore);

        public static FilterBase Like(string campo, string pattern) =>
            new FilterCondition(campo, Operator.Like, pattern);
        public static FilterBase StartWith(string campo, string pattern) =>
            new FilterCondition(campo, Operator.Like, pattern);
        public static FilterBase EndWith(string campo, string pattern) =>
            new FilterCondition(campo, Operator.Like, pattern);
        public static FilterBase In(string campo, IEnumerable list) =>
            new FilterCondition(campo, Operator.In, list);
        public static FilterBase NotIn(string campo, IEnumerable list) =>
            new FilterCondition(campo, Operator.In, list);

        public static FilterBase IsVoid(string campo) =>
            new FilterCondition(campo, Operator.IsVoid, null);
        public static FilterBase IsNotVoid(string campo) =>
            new FilterCondition(campo, Operator.IsVoid, null);
        public static FilterBase Not(string campo) =>
            new FilterCondition(campo, Operator.Not, null);
        public static FilterBase NoOperator(string campo) =>
            new FilterCondition(campo, Operator.NoOperator, null);

        public static FilterBase And(params FilterBase[] filtri) =>
            new FilterComposite(Combinator.And, filtri);

        public static FilterBase Or(params FilterBase[] filtri) =>
            new FilterComposite(Combinator.Or, filtri);
    }
    public class FilterComposite : FilterBase
    {
        public Combinator Combinatore { get; }
        public List<FilterBase> Children { get; }

        public FilterComposite(Combinator combinatore, params FilterBase[] figli)
        {
            Combinatore = combinatore;
            Children = figli.ToList();
        }

        //public string ToSql()
        //{
        //    var sep = Combinatore == Combinator.And ? " AND " : " OR ";
        //    return "(" + string.Join(sep, Children.Select(f => f.ToSql())) + ")";
        //}
    }
}
