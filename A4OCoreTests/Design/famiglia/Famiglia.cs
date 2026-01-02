using A4OCore.BLCore;
using A4OCore.Design;
using A4OCore.Models;
using A4OCore.Store;
using A4OCore.Store.DB;
using A4OCore.Utility;
using A4ODto;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4OCoreTests.Design.famiglia
{
    internal class Famiglia : ElementBLA4O
    {
        public Famiglia(IStoreA4O storeManager, IA4O_CheckEnumRepository checkEnum) : base(storeManager, checkEnum)
        {
        }

        public enum EnumElement { name, arnia , regina, visite }
    public enum EnumTable { _ = DefinitionValueConst.SINGLE_VALUE_TABLE_VALUE, visite}

        public override DesignElement Design
        {
            get
            {

                DesignElement e = DesignElement.GenerateNew<EnumElement , EnumTable> ("FAMIGLIA", DesignElementDtoConst.ROOT_NAME);
                e.Add(new ValueDesignBase ("name", ValueDesignType.STRING, EnumElement.name.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase ("arnia", ValueDesignType.INT, EnumElement.arnia.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase ("regina", ValueDesignType.INT, EnumElement.regina.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase ("visite", ValueDesignType.INT, EnumElement.visite.ToInt(), EnumTable.visite.ToInt()));

                return e;

            }

        }

        

        public override void OnAction( string actionName)
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
    public class FamigliaTest
    {
        [TestInitialize]
        public void Init()
        {

            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            services.AddSingleton<Famiglia>();
            var _provider = services.BuildServiceProvider();

            
            fam = _provider.GetRequiredService<Famiglia>();

            fam.CheckEnumerator();

        }
        Famiglia fam;
        [TestMethod]
        public void TestSave()
        {
            Famiglia r = fam;
            fam.CurrentElement=new ElementA4O(ElementA4O.Root);
            //r.StroreManager.Initialize();
            r.SetValue(Famiglia.EnumElement.name.ToInt(), "AAAA");
            r.SetValue(Famiglia.EnumElement.arnia.ToInt(), 1);
            r.SetValue(Famiglia.EnumElement.regina.ToInt(), 2025);
            r.SetValue(Famiglia.EnumTable.visite.ToInt(), Famiglia.EnumElement.visite.ToInt(), 1,123);
            r.SetValue(Famiglia.EnumTable.visite.ToInt(), Famiglia.EnumElement.visite.ToInt(), 2, 321);
            r.SetValue(Famiglia.EnumTable.visite.ToInt(), Famiglia.EnumElement.visite.ToInt(), 3, 5123);
            r.SetValue(Famiglia.EnumTable.visite.ToInt(), Famiglia.EnumElement.visite.ToInt(), 4, 5321);
            r.Save();

        }
    }
}
