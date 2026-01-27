using A4OCoreTests.Design.famiglia;
using A4OCoreTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using A4OCore.BLCore;
using A4OCore.Utility;
using Microsoft.Extensions.DependencyInjection;
using A4ODto;
using static A4OCore.BLCore.regina.ReginaBL;


namespace A4OCoreTests.BLCore
{
    [TestClass()]
    public class BLRepositoryTests
    {
        
        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            ServiceProvider _provider = services.BuildServiceProvider();
            blRep = _provider.GetRequiredService<IBLRepository>();
        }

        IBLRepository blRep;
        [TestMethod()]
        public void GetTest()
        {
            //var blRep = k.GetService<IBLRepository>();
            ElementBLA4O ar =blRep.GetElement("APE_REGINA");
            Assert.IsNotNull(ar);
            ar.LoadByIdAsync(1).GetAwaiter().GetResult();
            //A4ODto.UtilityBL.MappingDesign(ar.GetElementsSingle(), ar.Design).First();
            A4ODto.DefinitionValueDto definitionValue = new A4ODto.DefinitionValueDto();
            var e=ar.GetElementsSingle().ToArray().MappingToElementValueA4OAdditionalInfo(ar.Design);
            e.First();
        }


        [TestMethod()]
        public void TestFilterValue()
        {
            //var blRep = k.GetService<IBLRepository>();
            
            ElementBLA4O ar = blRep.GetElement("APE_REGINA");

            var all = ar.Load(new A4OCore.Store.FilterA4O());
            all.ForEach(a => { 
                ar.CurrentElement = a;
                ar.Delete();
            });
            Assert.IsNotNull(ar);

            ar.CurrentElement= ar.NewElementByParent(ElementA4ODto.Root);
            ar[EnumReginaElement.anno.ToInt()].IntVal = 2021;
            ar.Save();


            ar.CurrentElement = ar.NewElementByParent(ElementA4ODto.Root);
            ar[EnumReginaElement.anno.ToInt()].IntVal = 2022;
            ar.Save();

            ar.CurrentElement = ar.NewElementByParent(ElementA4ODto.Root);
            ar[EnumReginaElement.anno.ToInt()].IntVal = 2023;
            ar.Save();

            ar.CurrentElement = ar.NewElementByParent(ElementA4ODto.Root);
            ar[EnumReginaElement.anno.ToInt()].IntVal = 2024;
            ar.Save();

            //A4ODto.UtilityBL.MappingDesign(ar.GetElementsSingle(), ar.Design).First();
            var filter = new A4OCore.Store.FilterA4O();
            filter.WhereAndFilterOnValue(new A4OCore.Store.SimpleFilterCondition(EnumReginaElement.anno, A4OCore.Store.Operator.Equal, 2021), ar.Design);
            var allFiltered = ar.Load(filter);
            Assert.IsTrue(allFiltered.Count==1);
            ar.CurrentElement = allFiltered[0];
            Assert.IsTrue(ar[EnumReginaElement.anno.ToInt()].IntVal==2021);
            filter = new A4OCore.Store.FilterA4O();
            filter.WhereAndFilterOnValue(new A4OCore.Store.SimpleFilterCondition(EnumReginaElement.anno, A4OCore.Store.Operator.NotEqual, 2021), ar.Design);
            allFiltered = ar.Load(filter);
            Assert.IsTrue(allFiltered.Count == 3);

            filter = new A4OCore.Store.FilterA4O();
            filter.WhereAndFilterOnValue(new A4OCore.Store.SimpleFilterCondition(EnumReginaElement.anno, A4OCore.Store.Operator.In, new int[] { 2021, 2023 }), ar.Design);
            allFiltered = ar.Load(filter);
            Assert.IsTrue(allFiltered.Count == 2);
            Assert.IsTrue(allFiltered.Any(x=> { ar.CurrentElement = x;
                return ar[EnumReginaElement.anno.ToInt()].IntVal == 2021;
            }));
            Assert.IsTrue(allFiltered.Any(x => {
                ar.CurrentElement = x;
                return ar[EnumReginaElement.anno.ToInt()].IntVal == 2023;
            }));






        }



    }
}
