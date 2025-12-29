using Microsoft.VisualStudio.TestTools.UnitTesting;
using A4OCore.Store.DB.SQLLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using A4OCoreTests;
using Microsoft.Extensions.DependencyInjection;
using A4OCore.BLCore.regina;
using A4ODto;
using A4OCore.Utility;

namespace A4OCore.Store.DB.SQLLite.Tests
{
    [TestClass()]
    public class A4O_MapIdRepositoryTests
    {

        A4O_MapIdRepository a4O_MapIdRepository;
        //private IKernel _kernel;

        [TestInitialize]
        public void Setup()
        {
            //_kernel = new StandardKernel(new NinjectTestModule());
            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            var _provider = services.BuildServiceProvider();
            a4O_MapIdRepository = _provider.GetRequiredService<A4O_MapIdRepository>();
            
        }
        
        

        [TestMethod()]
        public void InsertTest()
        {
            //A4O_MapIdRepository  a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_MapIdRepository.Insert(new A4O_MapId() { ColumnName="AAA" ,IdColumn=1312 ,TableName= DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, ElementName= "TEST"});
            var v=(A4O_MapId)a4O_MapIdRepository.GetByKey("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
            Assert.IsTrue(v.IdColumn==1312);
            a4O_MapIdRepository.Delete("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
        }

        [TestMethod()]
        public void UpdateTest()
        {
            
            //A4O_MapIdRepository a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_MapIdRepository.Delete("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
            var toInsert = new A4O_MapId() { ColumnName = "AAA", IdColumn = 1312, TableName = DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, ElementName = "TEST" };
            a4O_MapIdRepository.Insert(toInsert);


            toInsert.IdColumn = 111;
            a4O_MapIdRepository.Update(toInsert);
            var v = (A4O_MapId)a4O_MapIdRepository.GetByKey("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
            Assert.IsTrue(v.IdColumn == 111);
            a4O_MapIdRepository.Delete("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
            
        }

        [TestMethod()]
        public void DeleteTest()
        {
            //A4O_MapIdRepository a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_MapIdRepository.Delete("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
            var toInsert = new A4O_MapId() { ColumnName = "AAA", IdColumn = 1312, TableName = DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, ElementName = "TEST" };
            a4O_MapIdRepository.Insert(toInsert);
            a4O_MapIdRepository.Delete("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
            var v = (A4O_MapId)a4O_MapIdRepository.GetByKey("TEST", DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, "AAA");
            Assert.IsNull(v);


        }

        [TestMethod()]
        public void GetAllTest()
        {
            var toInsert1 = new A4O_MapId() { ColumnName = "AAA", IdColumn = 1312, TableName = DefinitionValueConst.SINGLE_VALUE_TABLE_NAME, ElementName = "TEST123" };
            var toInsert2 = new A4O_MapId() { ColumnName = "TAB_1", IdColumn = 777, TableName = "T", ElementName = "TEST123" };
            //A4O_MapIdRepository a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_MapIdRepository.Delete(toInsert1);
            a4O_MapIdRepository.Delete(toInsert2);

            var r =a4O_MapIdRepository.GetAll("ElementName='TEST123'");
            Assert.IsTrue(r.Count==0);
            a4O_MapIdRepository.Insert(toInsert1);
            a4O_MapIdRepository.Insert(toInsert2);
            r = a4O_MapIdRepository.GetAll("ElementName='TEST123'");
            Assert.IsTrue(r.Count == 2);
            a4O_MapIdRepository.Delete(toInsert1);
            a4O_MapIdRepository.Delete(toInsert2);


        }
    }
}