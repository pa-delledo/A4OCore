using A4ODto;

namespace A4OCore.Models
{
    public abstract class DesignElementBase : DesignElementDto
    {


        public virtual bool IsStorable
        {
            get { return true; }
        }






        protected void CheckOnCostructor()
        {
#if DEBUG
            if (ElementName.ToUpperInvariant().StartsWith(DesignElementDtoConst.SYS_TABLE_NAME_START)) throw new Exception($"name cannot start with '{DesignElementDtoConst.SYS_TABLE_NAME_START}'");
            if (DesignElementDtoConst.SUFF_NOT_ALLOWED.Any(x => ElementName.ToUpperInvariant().EndsWith(x))) throw new Exception("name cannot end with " + string.Join(',', DesignElementDtoConst.SUFF_NOT_ALLOWED));

            if (EnumTable != null && (!EnumTable.ContainsKey(DesignElementDtoConst.SingleNameTable) || EnumTable[DesignElementDtoConst.SingleNameTable] != DesignElementDtoConst.SingleValueTable)) throw new Exception($"Enum table must contains \"{DesignElementDtoConst.SingleNameTable}\" = {DesignElementDtoConst.SingleValueTable}");

#endif
        }

        public DesignElementBase(string name, string parent)
        {
            name = name.Trim();

            this.ElementName = name;
            this.ElementNameParent = parent;
            CheckOnCostructor();
        }

        public DesignElementBase(string name, DesignElementBase parent)
        {
            this.ElementName = name;
            this.ElementNameParent = parent.ElementName;
            CheckOnCostructor();
        }
        private Dictionary<int, string> _enumElementRev = null;
        public Dictionary<int, string> EnumElementRev
        {
            get
            {
                if (_enumElementRev == null)
                {
                    _enumElementRev = EnumItems.ToDictionary(kv => kv.Value, kv => kv.Key);
                }
                return _enumElementRev;

            }
        }
        private Dictionary<int, string> _enumTabletRev = null;
        public Dictionary<int, string> EnumTabletRev
        {
            get
            {
                if (_enumTabletRev == null)
                {
                    _enumTabletRev = this.EnumTable.ToDictionary(kv => kv.Value, kv => kv.Key);
                }
                return _enumTabletRev;
            }
        }

        public string GetStringElementFromId(int id)
        {
            return EnumElementRev[id];
        }

        public string GetStringTableFromId(int id)
        {
            return EnumTabletRev[id];
        }

        public void Add(BaseDesignDto a)
        {
#if DEBUG
            if (ItemsDesignBase.Any(x => x.IdElement == a.IdElement)) throw new Exception("IdElement must be unique!!!");
#endif
            ItemsDesignBase.Add(a);
        }

        public bool IsNoLink()
        {
            return this.ElementName == DesignElementDtoConst.NOLINK_NAME;
        }

        public bool IsRoot()
        {
            return this.ElementName == DesignElementDtoConst.ROOT_NAME;
        }

        public void Add(string label, ValueDesignType valueType, string? elementName)
        {
            if (!EnumItems.ContainsKey(elementName))
            {
                throw new Exception("wrong elementName!");
            }

            this.Add(new ValueDesignBase(label, valueType, EnumItems[elementName], DesignElementDtoConst.SingleValueTable));
        }

        public void Add(string label, ValueDesignType valueType, int enEl)
        {
            this.Add(new ValueDesignBase(label, valueType, enEl, DesignElementDtoConst.SingleValueTable));
        }

        public void Add(string label, ValueDesignType valueType, int enEl, int enumTab)
        {
            this.Add(new ValueDesignBase(label, valueType, enEl, enumTab));
        }
        public void AddOptionSet(string label, int enEl, int enumTab, Dictionary<string, string> optSet)
        {
            this.Add(new ValueDesignBase(label, enEl, enumTab, optSet));
        }

        public void Add(string label, ValueDesignType valueType, string? elementName, string? tableName = null)
        {
            if (!EnumItems.ContainsKey(elementName))
            {
                throw new Exception("wrong element Name!");
            }
            tableName = tableName ?? DesignElementDtoConst.SingleNameTable;
            if (!EnumTable.ContainsKey(tableName))
            {
                throw new Exception("wrong table Name!");
            }
            this.Add(new ValueDesignBase(label, valueType, EnumItems[elementName], EnumTable[tableName]));
        }


    }
}
