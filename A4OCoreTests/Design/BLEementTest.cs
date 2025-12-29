
using A4OCore.Design;

using A4OCore.Store;
using A4OCore.Utility;
using Microsoft.Extensions.DependencyInjection;
using A4ODto;
using A4OCore.Models;


namespace A4OCore.BLCore
{
    public enum EnumElement { pippo=1, pluto ,paperino}
        public enum EnumTable { _=1 }

    public class Class1 : ElementBLA4O 
    {
        public Class1(IStoreA4O storeManager, IA4O_CheckEnumRepository checkEnum) : base(storeManager, checkEnum)
        {
        }
        

        public override DesignElement  Design
        {
            get
            {

                DesignElement  e = new DesignElement ("TESTCL", DesignElementDtoConst.ROOT_NAME, DesignElement.EnumToDictionary<EnumElement>(), DesignElement.EnumToDictionary<EnumTable>());
                e.Add(new ValueDesignBase ("pippo", ValueDesignType.STRING, (int)(object)EnumElement.pippo, SingleValueTable));
                e.Add(new ValueDesignBase ("pluto", ValueDesignType.INT, (int)(object)EnumElement.pluto, SingleValueTable));
                e.Add(new ValueDesignBase ("paperino", ValueDesignType.INT, (int)(object)EnumElement.paperino, SingleValueTable));
                return e;

            }

        }

        

        public override void OnAction(string actionName)
        {
            throw new NotImplementedException();
        }

        public override void OnAfterSave()
        {
            throw new NotImplementedException();
        }

        public override void OnButton(string buttonName, int idx)
        {
            throw new NotImplementedException();
        }

        public override void OnChange(params (string valueName, int idx)[] changedValues)
        {
            throw new NotImplementedException();
        }

        public override List<MessageA4O> OnCheck()
        {
            throw new NotImplementedException();
        }

        public override void OnSave()
        {
            throw new NotImplementedException();
        }

        
    }

    [TestClass()]
    public class BLEementTest
    {
        Class1 class1;
        [TestInitialize]
        public void Setup()
        {

            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            services.AddSingleton<Class1>();
            var _provider = services.BuildServiceProvider();

            class1 = _provider.GetRequiredService<Class1>();

            class1.CheckEnumerator();
        }

        [TestMethod]
        public void Test()
        {
            ElementA4O element = new ElementA4O(ElementA4O.Root)
            {

            };
            class1.CurrentElement = element;

            //class1.StroreManager.Initialize();
            class1.SetValue(EnumElement.pippo.ToInt(), "Ciao!!");
            class1.SetValue(EnumElement.pluto.ToInt(), 123);

            var r = class1[Class1.SingleValueTable,0];

            class1.Save();

            //class1.StroreManager.Load()

        }


    }
}