namespace A4OCore.Store.DB
{
    public class A4O_CheckEnum
    {


        public int EnumInt;
        public bool IsTable;
        public string? ElementName;
        public string? EnumString;




        public bool Equals(A4O_CheckEnum? other)
        {
            if (this == other) return true;
            if (other == null) return false;

            bool res =
                this.ElementName == other.ElementName &&
                this.IsTable == other.IsTable &&
                this.EnumInt == other.EnumInt &&
                this.EnumString == other.EnumString;
            return res;
        }

    }
}
