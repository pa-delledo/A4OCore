using A4ODto.View;

namespace A4OCore.Models
{
    public interface IViewBLA4O

    {

        public string[] ColumnNames { get; }

        public List<List<CellViewA4ODto>> GetRows(Dictionary<string, object> parameters);
        public string ViewName { get; }

        //[Inject]
        //public IKernel kernel;



    }
}
