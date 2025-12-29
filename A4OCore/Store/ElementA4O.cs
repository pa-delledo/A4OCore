using A4ODto;

namespace A4OCore.Store
{
    public class ElementA4O : A4ODto.ElementA4ODto
    {

        public ElementA4O()
        {
            this.Date = DateTime.Today;

        }
        public ElementA4O(ElementA4ODto parent) : this()
        {
            this.IdParent = parent.Id;
            this.ElementNameParent = parent.ElementName;
        }




    }

}
