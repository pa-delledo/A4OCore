using Microsoft.VisualStudio.TestTools.UnitTesting;
using A4OCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A4OCore.BLCore;
using Microsoft.Extensions.DependencyInjection;
using A4OCore.Store;
using A4OCoreTests.Design.famiglia;
using A4OCore.BLCore.regina;
using A4OCore.Utility;
using A4ODto;

namespace A4OCore.Design.Tests
{
    [TestClass()]
    public class UtilityDesignTests
    {
        [TestInitialize]
        public void Setup()
        {

            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            services.AddSingleton<Class1>();
            var _provider = services.BuildServiceProvider();
            blRep = _provider.GetRequiredService<IBLRepository>();
        }

        IBLRepository blRep;
            //class1 = _provider.GetRequiredService<Class1>();

            //class1.CheckEnumerator();
        
        [TestMethod()]
        public void GetIdElementFromInfoDataTest()
        {
            ElementBLA4O ar = blRep.GetElement("APE_REGINA");
            ElementA4O element = new ElementA4O(ElementA4O.Root);
            ar.CurrentElement = element;
            ar.SetValue(ReginaBL.EnumReginaElement.marcata.ToInt(), 1);
            ar.SetValue(ReginaBL.EnumReginaElement.nome.ToInt(), "peppina");
            var infoData = element.Values[0].InfoData;
                Assert.IsTrue( UtilityDesign.GetIdElementFromInfoData(infoData)== ReginaBL.EnumReginaElement.marcata.ToInt());
            infoData = element.Values[1].InfoData;
            Assert.IsTrue(UtilityDesign.GetIdElementFromInfoData(infoData) == ReginaBL.EnumReginaElement.nome.ToInt());


        }

        [TestMethod()]
        public void GetTableFromInfoDataTest()
        {
            ElementBLA4O ar = blRep.GetElement("APE_REGINA");
            ElementA4O element = new ElementA4O(ElementA4O.Root);
            ar.CurrentElement = element;
            ar.SetValue(ReginaBL.EnumReginaElement.marcata.ToInt(), 1);
            ar.SetValue(ReginaBL.EnumReginaElement.nome.ToInt(), "peppina");
            var infoData = element.Values[0].InfoData;
            var tableId = UtilityDesign.GetTableFromInfoData(infoData);
            Assert.IsTrue(tableId == ReginaBL.EnumReginaTable._.ToInt());
            infoData = element.Values[1].InfoData;
            Assert.IsTrue(UtilityDesign.GetTableFromInfoData(infoData) == ReginaBL.SingleValueTable);


        }

        [TestMethod()]
        public void GetTypeFromInfoDataTest()
        {

            ElementBLA4O ar = blRep.GetElement("APE_REGINA");
            ElementA4O element = new ElementA4O(ElementA4O.Root);
            ar.CurrentElement = element;
            ar.SetValue(ReginaBL.EnumReginaElement.marcata.ToInt(), 1);
            ar.SetValue(ReginaBL.EnumReginaElement.nome.ToInt(), "peppina");
            ar.SetValue(ReginaBL.EnumReginaTable.Trovata.ToInt(), ReginaBL.EnumReginaElement.Trovata_Data.ToInt(), 0 ,DateTime.Today);
            ar.SetValue(ReginaBL.EnumReginaTable.Trovata.ToInt(), ReginaBL.EnumReginaElement.Trovata_Data.ToInt(), 1, DateTime.Today);

            var infoData = element.Values[0].InfoData;
            Assert.IsTrue(UtilityDesign.GetTypeFromInfoData(infoData) == ValueDesignType.INT);
            infoData = element.Values[1].InfoData;
            Assert.IsTrue(UtilityDesign.GetTypeFromInfoData(infoData) == ValueDesignType.STRING);
            infoData = element.Values[2].InfoData;
            Assert.IsTrue(UtilityDesign.GetTypeFromInfoData(infoData) == ValueDesignType.DATE);
            Assert.IsTrue(UtilityDesign.GetTableFromInfoData(infoData) == ReginaBL.EnumReginaTable.Trovata.ToInt());
            Assert.IsTrue(UtilityDesign.GetIdElementFromInfoData(infoData) == ReginaBL.EnumReginaElement.Trovata_Data.ToInt());

            infoData = element.Values[3].InfoData;
            var d=ar.Design.ItemsDesignBase.First(x => x.InfoData == infoData);
            Assert.IsTrue(UtilityDesign.GetTypeFromInfoData(infoData) == ValueDesignType.DATE);
            Assert.IsTrue(UtilityDesign.GetTableFromInfoData(infoData) == ReginaBL.EnumReginaTable.Trovata.ToInt());
            Assert.IsTrue(UtilityDesign.GetIdElementFromInfoData(infoData) == ReginaBL.EnumReginaElement.Trovata_Data.ToInt());




        }
    }
}