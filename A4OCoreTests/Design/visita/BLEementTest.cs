//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using A4OCore.Design;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using A4OCore.Store;
//using A4OCore.BLCore;


//namespace A4OCoreTests.Design.visita
//{
//        public enum EnumElement { data=1, ora , utente , telaiCovata, telaiScorte,telaiNuovi,numeroTotale, uovaPresenti }
//        public enum EnumTable { _=1 }

//    public class Visita : BLElement<EnumElement, EnumTable>
//    {
//        public override DesignElement<EnumElement, EnumTable> Design
//        {
//            get
//            {

//                DesignElement<EnumElement, EnumTable> e = new DesignElement<EnumElement, EnumTable>("VISITA", DesignElement<EnumElement, EnumTable>.ROOT_NAME);
//                e.Add(new ValueDesignBase<EnumElement, EnumTable>("pippo", ValueDesignType.STRING, EnumElement.pippo, SingleValueTable));
//                e.Add(new ValueDesignBase<EnumElement, EnumTable>("pluto", ValueDesignType.INT, EnumElement.pluto, SingleValueTable));
//                e.Add(new ValueDesignBase<EnumElement, EnumTable>("paperino", ValueDesignType.INT, EnumElement.paperino, SingleValueTable));
//                return e;

//            }

//        }

//        public override void OnAction(ElementA4O element, string actionName)
//        {
//            throw new NotImplementedException();
//        }

//        public override void OnAfterSave(ElementA4O element)
//        {
//            throw new NotImplementedException();
//        }

//        public override void OnButton(ElementA4O element, string buttonName, int idx)
//        {
//            throw new NotImplementedException();
//        }

//        public override void OnChange(ElementA4O element, params (string valueName, int idx)[] changedValues)
//        {
//            throw new NotImplementedException();
//        }

//        public override List<MessageA4O> OnCheck(ElementA4O element)
//        {
//            throw new NotImplementedException();
//        }

//        public override void OnSave(ElementA4O element)
//        {
//            throw new NotImplementedException();
//        }

//        public override void SetAcl(ElementA4O element, List<AclValueA4O> acl)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    [TestClass()]
//    public class BLEementTest
//    {
//        [TestMethod]
//        public void Test()
//        {
//            Class1 class1 = new Class1();
//            class1.StroreManager.Initialize();
//            class1.SetValue( EnumElement.pippo, "Ciao!!");
//            class1.SetValue( EnumElement.pluto, 123);

//            var r = class1[Class1.SingleValueTable,0];

//            class1.Save();

//            //class1.StroreManager.Load()

//        }


//    }
//}