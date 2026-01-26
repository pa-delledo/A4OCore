using A4OCore.BLCore;
using A4OCore.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace A4OCore.Store
{
    public abstract class Expression
    {
        public abstract string ToSql();
    }

    //public class ItemExpression : Expression
    //{
    //    Expression item { get; set; }
    //    ItemExpression (int item , string operation , params SqliteParameter[] par)
    //    {
    //        par.ParameterName
            
    //    }
    //}
    public class NotExpression : Expression
    {
        Expression item { get; set; }
        NotExpression(Expression item)
        {
            this.item = item;
        }
        public override string ToSql()
        {
            return " not (" + item.ToSql() + ")";
        }
    }
    public class AndExpression : Expression
    {
        List<Expression> items { get; set; } = new List<Expression>();
        AndExpression(params Expression[] items)
        {
            this.items=items.ToList();
        }
        public override string ToSql()
        {
            return "(" + string.Join(" and ", items.Where(x => x != null).Select(x => x.ToSql())) + ")";
        }
    }
    public class OrExpression : Expression
    {
        List<Expression> items { get; set; } = new List<Expression>();
        OrExpression(params Expression[] items)
        {
            this.items = items.ToList();
        }
        public override string ToSql()
        {
            return "(" + string.Join(" or ", items.Where(x => x != null).Select(x => x.ToSql())) + ")";
        }
    }

    public enum OrderByEnum { Id, ParentName, ParentIds, Date, DateChange, Deleted }


    internal enum CustomTypeQuery { Last_N_with_Parent }
    public class FilterA4O
    {
        

        public long[]? Ids { get; private set; }
        public bool InvertSelectionIds { get; private set; } = false;
        public string[]? ParentName { get; private set; }
        public bool InvertSelectionParentIds { get; private set; } = false;
        public long[]? ParentIds { get; private set; }
        public (DateTime? from, DateTime? to)? Date { get; private set; }
        public bool InvertSelectionDate { get; private set; } = false;
        public int? Top { get; private set; }
        public int? Offset { get; private set; }
        public (DateTime? from, DateTime? to)? DateChange { get; private set; }
        public bool InvertSelectionDateChange { get; private set; } = false;
        public bool? Deleted { get; private set; }
        public int[]? ResultValues { get; private set; }
        public bool Ascending { get; private set; }
        public OrderByEnum? OrderBy { get; private set; }
        public Expression FilterItems {  get; private set; }
        public  List<string> CustomFilter { get; private set; }
        public FilterA4O()
        {
        }

        public FilterA4O ResultOrderBy(OrderByEnum? orderByEnum, bool asc = true)
        {
            this.OrderBy = orderByEnum;
            this.Ascending = asc;
            return this;

        }


        public FilterA4O ResultTop(int? top, int? offset = null, OrderByEnum? orderByEnum = null, bool asc = true)
        {
            if (Offset.HasValue && !top.HasValue) { throw new Exception("invalid Operation!!!!"); }
            this.Top = top;
            this.Offset = offset;

            return this.ResultOrderBy(orderByEnum ?? OrderByEnum.Id, asc);
        }


        public FilterA4O WhereId(bool invertSelection, params long[]? ids)
        {
            this.Ids = ids;
            this.InvertSelectionIds = invertSelection;
            return this;
        }
        public FilterA4O WhereId(params long[]? ids)
        {
            this.Ids = ids;
            this.InvertSelectionIds = false;
            return this;
        }


        public FilterA4O WhereParentName(params string[] parentName)
        {
            this.ParentName = parentName;
            return this;
        }

        public FilterA4O WhereParentId(params long[]? ids)
        {
            this.ParentIds = ids;
            return this;
        }
        public FilterA4O WhereIsDeleted(bool? isLogicalDeleted)
        {
            this.Deleted = isLogicalDeleted;
            return this;
        }

        public FilterA4O WhereCustomFilter_Last_N_whit_Parent(int nForParent , params long[] idParents)
        {

            this.CustomFilter = this.CustomFilter ?? new List<string>();
            
            this.CustomFilter .Add(
                JsonSerializer.Serialize(
                new CustomFilterClass()
            {
                CustomFilterName = CustomTypeQuery.Last_N_with_Parent,
                N4Parent = nForParent,
                IdParents = idParents
            } ));

            return this;
        }
        public FilterA4O WhereDate(DateTime? from, DateTime? to = null, bool invertSelectionDate = false)
        {
            this.Date = from == to == null ? null : (from, to);
            this.InvertSelectionDate = invertSelectionDate;
            return this;
        }
        public FilterA4O WhereDateChange(DateTime? from, DateTime? to = null, bool invertSelectionDate = false)
        {
            this.DateChange = from == to == null ? null : (from, to);
            this.InvertSelectionDateChange = invertSelectionDate;
            return this;
        }
        public FilterA4O SetReultValues(ElementBLA4O el, params string[] valuesNames)
        {
            return SetReultValues(el.Design, MapStringToIdItems(el.Design, valuesNames).ToArray());
        }
        public FilterA4O SetReultValues(ElementBLA4O el, params int[] valuesIds)
        {

            return SetReultValues(el.Design, valuesIds);
        }
        private static IEnumerable<int> MapStringToIdItems(DesignElement design, string[] valuesNames)
        {
            return valuesNames.Select(x => design.EnumItems[x]);

        }
        public FilterA4O SetReultValues(DesignElement design, params string[] valuesNames)
        {
            return SetReultValues(design, MapStringToIdItems(design, valuesNames).ToArray());
        }
        public FilterA4O SetReultValues(DesignElement design, params int[] valuesIds)
        {
            this.ResultValues = valuesIds.Select(x => design.ItemsDesignBase.First(d => d.IdElement == x).InfoData).ToArray();
            return this;
        }


        public FilterA4O SetReultValuesInfoData(params int[] infosData)
        {
            this.ResultValues = infosData;
            return this;
        }


    }
}
