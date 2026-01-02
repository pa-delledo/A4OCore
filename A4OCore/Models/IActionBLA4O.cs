using A4ODto.Action;

namespace A4OCore.Models
{
    public interface IActionBLA4O
    {
        public List<ActionDto> Execute(Dictionary<string, object> parameters);
        public string ActionName { get; }

    }
}
