
using A4ODto;

namespace A4OCore.Models
{

    public class ValueDesignBase : BaseDesignDto
    {
        public ValueDesignBase(string label, int idElement, int tableName, Dictionary<string, string> optionsSetValues)
        {

            this.DesignType = ValueDesignType.OPTIONSET;
            this.IdElement = idElement;
            this.Table = tableName;
            this.Label = label;
            this.OptionsSetValues = optionsSetValues;

        }
        public ValueDesignBase(string label, ValueDesignType designType, int idElement, int tableName)
        {
#if DEBUG
            if (designType == ValueDesignType.OPTIONSET)
            {
                throw new Exception("Use ValueDesignBase(string label, int idElement, int tableName, Dictionary<string, string> optionsSetValues)!!");
            }
#endif
            this.DesignType = designType;
            this.IdElement = idElement;
            this.Table = tableName;
            this.Label = label;

        }



        public string? TypeName
        {
            get
            {
                return this.DesignType.ToString();
            }
        }

        public static ValueDesignType? GetTypeFromInfoData(int idDes)
        {

            var desType = Utility.UtilityDesign.GetTypeFromInfoData(idDes);
            if ((int)desType == 0) return null;
            return desType;
        }




        public static int? GetTableFromInfoData(int idDes)
        {

            int idTable = Utility.UtilityDesign.GetTableFromInfoData(idDes);
            if (idTable <= 0)
                return null;
            return idTable;
        }

        public int? GetIdElementFromInfoData(int idDes)
        {
            int idElement = Utility.UtilityDesign.GetIdElementFromInfoData(idDes);
            if (idElement <= 0)
                return null;
            return idElement;

        }





    }
}
