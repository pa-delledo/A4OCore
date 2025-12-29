using A4ODto;
using A4ODto.Context;

namespace A4OCore.Models
{
    public class MenuBLA4O : MenuBLA4ODto
    {
        public virtual void CalculateVisibility(ContextDto context)
        {
            IsVisible = true;
        }


        public MenuBLA4O Add(MenuBLA4O subMenu)
        {
            if (subMenu == null) return this;
            this.Childrens = this.Childrens ?? new List<MenuBLA4ODto>();
            this.Childrens.Add(subMenu);
            return this;
        }

    }
}
