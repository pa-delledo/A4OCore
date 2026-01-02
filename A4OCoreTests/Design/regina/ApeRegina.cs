using A4OCore.Design;
using A4OCore.Store;
using A4OCoreTests.Design.famiglia;
using A4OCoreTests;
using System.Runtime.Intrinsics.X86;
using static A4OCore.BLCore.regina.ReginaBL;

using A4OCore.Utility;
using Microsoft.Extensions.DependencyInjection;
using A4OCore.Store.DB;
using A4ODto;
using A4OCore.Models;



namespace A4OCore.BLCore.regina

{



    public class ReginaBL : ElementBLA4O
    {
        public ReginaBL(IStoreA4O storeManager, IA4O_CheckEnumRepository checkEnum) : base(storeManager, checkEnum)
        {
            
        }

        public enum EnumReginaElement { anno, nome, marcata, provenienza, colore, giudizio, coloreOptSet , Trovata_Data, Trovata_Stato }
        public enum EnumReginaTable { _ = SingleValueTable , Trovata =2}

        public override DesignElement Design
        {
            get
            {

                DesignElement e = new DesignElement("APE_REGINA", DesignElementDtoConst.ROOT_NAME ,DesignElement.EnumToDictionary<EnumReginaElement>(), DesignElement.EnumToDictionary<EnumReginaTable>());
                e.Add(new ValueDesignBase("Anno", ValueDesignType.INT, EnumReginaElement.anno.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase("Nome", ValueDesignType.STRING, EnumReginaElement.nome.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase("marcata", ValueDesignType.INT, EnumReginaElement.marcata.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase("provenienza", ValueDesignType.STRING, EnumReginaElement.provenienza.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase("colore", ValueDesignType.INT, EnumReginaElement.colore.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase("giudizio", ValueDesignType.INT, EnumReginaElement.giudizio.ToInt(), SingleValueTable));
                e.Add(new ValueDesignBase("Data in cui si è vista", ValueDesignType.DATE, EnumReginaElement.Trovata_Data.ToInt(), EnumReginaTable.Trovata.ToInt() ));
                e.Add(new ValueDesignBase("Stato in cui si è vista", ValueDesignType.INT, EnumReginaElement.Trovata_Stato.ToInt(), EnumReginaTable.Trovata.ToInt() ));

                e.AddOptionSet("colore", EnumReginaElement.coloreOptSet.ToInt(), SingleValueTable, new Dictionary<string, string>() { { "WHITE", "BIANCO ( 1 o 6)" }, { "YELLOW", "GIALLO ( 2 o 7)" }, { "RED", "ROSSO ( 3 o 8)" }, { "GREEN", "VERDE ( 4 o 9)" },  { "BLUE", "BLU  ( 0 o 5)" } });
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
    public class ApeReginaTest
    {
        ReginaView rv;

        [TestInitialize]
        public void Init()
        {

            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            services.AddSingleton<ReginaView>();
            services.AddSingleton<ReginaBL>();
            var _provider = services.BuildServiceProvider();

            
        
            rv = _provider.GetRequiredService<ReginaView>();
            reg = _provider.GetRequiredService<ReginaBL>();

            reg.CheckEnumerator();

        }

        [TestMethod]
        public void TestMenu()
        {
            MenuBLA4O menuBLA4O = new MenuBLA4O()
            {
                Label = "Apiario",
                IsVisible=true
            };
            menuBLA4O.Add(
                new MenuBLA4O()
                {
                    Label = "Regina",

                }

                ).Add(
                new MenuBLA4O()
                {
                    Label = "Famiglia",

                })
                .Add(
                new MenuBLA4O()
                {
                    Label = "Hidden",
                    IsVisible=false
                }


                ).Add(
                new MenuBLA4O()
                {
                    Label = "Arnia",
                    
                });
            var res = menuBLA4O.Menu2String();
            var correctString = "Apiario\r\n\tRegina\r\n\tFamiglia\r\n\tArnia";
            Assert.IsTrue(correctString == res);

        }
        
        ReginaBL reg;
        [TestMethod]
        public void Test()
        {
            ReginaBL r = reg;
            r.CurrentElement = new ElementA4O(ElementA4O.Root);
            //r.StroreManager.Initialize();
            r.SetValue( EnumReginaElement.nome.ToInt(), "PEPPINA");
            r.SetValue(EnumReginaElement.anno.ToInt(), 2025);
            r.SetValue(EnumReginaElement.colore.ToInt(), 1);
            r.SetValue(EnumReginaElement.provenienza.ToInt(), "sdajkldsakl dasklsjd");
            r.SetValue(EnumReginaElement.giudizio.ToInt(), 7);
            r.SetValue(EnumReginaElement.marcata.ToInt(), 1);
            r.SetValue(EnumReginaTable.Trovata.ToInt() , EnumReginaElement.Trovata_Data.ToInt(),0, DateTime.Today.AddDays(-10) );
            r.SetValue(EnumReginaTable.Trovata.ToInt(), EnumReginaElement.Trovata_Data.ToInt(),1, DateTime.Today.AddDays(-5) );
            r.SetValue(EnumReginaTable.Trovata.ToInt(), EnumReginaElement.Trovata_Data.ToInt(),2, DateTime.Today.AddDays(-0) );



            r.Save();

            FilterA4O f = new FilterA4O();
            f.WhereDate(DateTime.Today).SetReultValues(r.Design, EnumReginaElement.Trovata_Data.ToInt());
            List<ElementA4ODto> x = r.Load(f);





            r.CurrentElement = new ElementA4O(ElementA4O.Root);
            r.SetValue(EnumReginaElement.nome.ToInt(), "ANNA");
            r.SetValue(EnumReginaElement.anno.ToInt(), 2024);
            r.SetValue(EnumReginaElement.colore.ToInt(), 2);
            r.SetValue(EnumReginaElement.provenienza.ToInt(), "sdajkldsakl dasklsjd");
            r.SetValue(EnumReginaElement.giudizio.ToInt(), 6);
            r.SetValue(EnumReginaElement.marcata.ToInt(), 1);
            r.Save();
            var aaa = r[ReginaBL.SingleValueTable, 0];
            f = new FilterA4O();
            f.WhereDate(DateTime.Today);

            x = r.Load(f);

            
            



            foreach ( var x2 in x)
            {
                r.CurrentElement = x2;
                r.Delete();
            }



            //class1.StroreManager.Load()

        }


    }
}