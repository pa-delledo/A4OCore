using A4ODto;
using A4ODto.Action;

namespace A4OCore.Models
{
    public interface IElementBLA4O
    {
        DesignElement Design { get; }
        bool CanRead(ElementA4ODto element);
        bool CanWrite(ElementA4ODto element);
        void Delete();
        Task DeleteAsync();
        Task LoadByIdAsync(long id);
        void OnAction(string actionName);
        void OnButton(string buttonName, int idx);
        List<ActionDto> OnChange(string elementName, int idx);
        void OnOpen(bool isNew);
        List<MessageA4O> OnCheck();
        void CustomizeElementView(ViewValueDto clientValue);
        void OnSave();
        void Save();
        Task SaveAsync();
        //void SetAcl(ElementA4ODto element, List<AclValueA4ODto> acl);
        //void SetDesignValue(ElementValueA4ODto val, ref DesignValueDto designValueDto);

        List<ActionDto> ActionAfterSave();
        List<ActionDto> OnPreSave();
        List<ViewValueDto> GetElementsViewModel(List<(int idElemet, IEnumerable<int>? idx)>? filterElements = null);
        ElementA4ODto CurrentElement { get; set; }
        //ElementA4ODto ElementByParent(long id, ElementA4ODto parent);

        void SetDate(DateTime dat);
        void SetIsDeleted(bool del);
        void CheckEnumerator();
        void Initialize();
    }
}
