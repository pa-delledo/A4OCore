using A4OCore.BLCore;
using A4ODto;
using A4ODto.View;

namespace A4OCore.Models
{
    public abstract class ViewBLA4O: IViewBLA4O
    {
        public abstract List<ViewRowDto> GetRows(Dictionary<string, object> parameters);
        public abstract string[] ColumnNames { get; }
        public abstract string ViewName { get; }
        public ElementBLA4O CurrentManager { get; set; }

        public ViewBLA4O(IBLRepository bLRepository, string elementNamr)
        {
            this.CurrentManager = bLRepository.GetElement(elementNamr) as ElementBLA4O;
        }
        

    }
}
