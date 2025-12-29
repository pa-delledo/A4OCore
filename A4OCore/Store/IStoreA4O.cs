using A4ODto;

namespace A4OCore.Store
{







    //public class FiltroCondizione 
    //{

    //    public string ToSql()
    //    {
    //        string op = Operatore switch
    //        {
    //            Operatore.Equal => "=",
    //            Operatore.NotEqual => "<>",
    //            Operatore.GreaterThan => ">",
    //            Operatore.LessThan => "<",
    //            Operatore.Like => "LIKE",
    //            _ => throw new NotSupportedException()
    //        };

    //        string val = Valore is string ? $"'{Valore}'" : Valore.ToString();

    //        return $"{Campo} {op} {val}";
    //    }
    //}



    public interface IStoreA4O

    {


        //void Initialize();
        Task DeleteAsync(params long[] ids);
        Task DeleteAsync(params ElementA4ODto[] elementA4O);
        //bool ExistsTable();
        //bool ExistsView(string viewName);
        Task SaveAsync(params ElementA4ODto[] ins);
        void Save(params ElementA4ODto[] ins);
        //void SetFilterElementValueA4O(Func<ElementValueA4O, bool> filter);
        Task<List<ElementA4ODto>> LoadAsync(FilterA4O filter);
        List<ElementA4ODto> Load(FilterA4O filter);

    }

}
