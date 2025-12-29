using A4ODto;

namespace A4OCore.Utility
{
    public static class UtilityDesign
    {
        public static int GetIdElementFromInfoData(int InfoData)
        {
            return InfoData & 0b1111111111;
        }

        public static int GetTableFromInfoData(int idDes)
        {
            idDes = idDes >> 10;
            idDes = idDes & 0b1111111111;
            return idDes;
        }

        public static ValueDesignType GetTypeFromInfoData(int idDes)
        {
            idDes = idDes >> 20;
            return (ValueDesignType)idDes;
        }
    }
}
