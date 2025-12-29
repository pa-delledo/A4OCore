//using A4OCore.Cfg;
//using A4OCore.Store.DB;
//using A4OCore.Store.DB.SQLLite;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace A4OCore.Store
//{
//    public static class ManagerA4O
//    {
//        public static IA4O_CheckEnumRepository CurrentCheckEnumA4O
//        {
//            get
//            {
//                switch (Cfg.Cfg.CurrentConfiguration.StoreType)
//                {
//                    case StoreEnum.SqlLite:
//                        return new A4O_CheckEnumRepository();
//                    default:
//                        throw new NotImplementedException();
//                }
//            }
//        }



//        public static IStoreA4O CurrentStoreA4O(string tableName , List<DefinitionValue> defValues)
//        {
//            switch (Cfg.Cfg.CurrentConfiguration.StoreType)
//            {
//                case StoreEnum.SqlLite:
//                    return new DBBase(tableName, defValues);
//                default:
//                    throw new NotImplementedException();
//            }


//        }

//    }
//}
