namespace A4OCore.Models
{
    public class DesignElement : DesignElementBase
    {
        //private readonly Dictionary<string, int> _enumItems;
        //private readonly Dictionary<string, int> _enumTable;

        public DesignElement(string name, string parent, Dictionary<string, int> EnumItems, Dictionary<string, int> EnumTable) : base(name, parent)
        {
            this.EnumItems = EnumItems;
            this.EnumTable = EnumTable;
            CheckOnCostructor();
        }
        public static DesignElement GenerateNew<Element, Table>(string name, string parent) where Table : Enum
                                                                                            where Element : Enum
        {
            return new DesignElement(name, parent, EnumToDictionary<Element>(), EnumToDictionary<Table>());
        }
        public static Dictionary<string, int> EnumToDictionary<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>().ToDictionary(y => y.ToString(), x => (int)(object)x);
        }

        //public Dictionary<string, int> EnumItems => _enumItems;

        //public Dictionary<string, int> EnumTable => _enumTable;
    }
}
