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



    }
}
