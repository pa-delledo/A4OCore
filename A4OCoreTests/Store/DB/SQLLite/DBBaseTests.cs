using A4ODto;
using Microsoft.Extensions.DependencyInjection;
using A4OCore.Utility;

namespace A4OCore.Store.DB.SQLLite.Tests
{
    [TestClass()]
    public class DBBaseTests
    {
        private static readonly object _lockObject = new object();

        
        DBBase dbBase;
        [TestInitialize]
        public void Initialize()
        {
            // Puoi fare qualsiasi setup che richieda l'esecuzione sequenziale
            Monitor.Enter(_lockObject);  // Blocca il thread fino a quando non è rilasciato
            var services = new ServiceCollection();
            services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            var _provider = services.BuildServiceProvider();
            dbBase=_provider.GetRequiredService<DBBase>();
        }


        [TestMethod()]
        public void ExistsTableTest()
        {

            Console.WriteLine("E1");
            DropTables("TEST");
            Console.WriteLine("E2");
            Assert.IsFalse(dbBase.ExistsTable("TEST"));
            Console.WriteLine("E3");

            Console.WriteLine("E4");
            dbBase.SetTableInfo("TEST", null); 
            Console.WriteLine("E5");
            Assert.IsTrue(dbBase.ExistsTable("TEST"));
            Console.WriteLine("E6");



        }

        [TestMethod()]
        public void ExistsTableTest2()
        {


            Console.WriteLine("T1");
            DropTables("TEST");
            Console.WriteLine("T2");
            Assert.IsFalse(dbBase.ExistsTable("TEST"));
            Console.WriteLine("T3");
            //dBBase = new DBBase("TEST", null);
            dbBase.SetTableInfo("TEST", null);
            Console.WriteLine("T4");
            //dBBase.Initialize();
            Console.WriteLine("T5");
            Assert.IsTrue(dbBase.ExistsTable("TEST"));
            Console.WriteLine("T6");



        }

        [TestMethod()]
        public void LoadTestFilterTop()
        {
            Task.Run(() => LoadTestFilterTopAsync()).GetAwaiter().GetResult();

        }

        
        public async void LoadTestFilterTopAsync()
        {

            {
                DropTables("TEST");
                var el1 = CreateElement();
                //DBBase dBBase = new DBBase("TEST", el1.Item2);
                dbBase.SetTableInfo("TEST", el1.Item2);
                //dBBase.Initialize();

                await dbBase.SaveAsync(el1.Item1);
                el1.Item1.Date = DateTime.Today.AddDays(-100);
                await dbBase.SaveAsync(el1.Item1);

                var el2 = CreateElement();
                await dbBase.SaveAsync(el2.Item1);
                var f = new FilterA4O();
                f.ResultTop(1);
                var r = await dbBase.LoadAsync(f);
                Assert.IsNotNull(r);
                Assert.IsTrue(r.Count() == 1);
                Assert.IsTrue(r[0].Equals(el1.Item1, el1.Item2));


                f = new FilterA4O();
                f.WhereDate(null, DateTime.Today.AddDays(-10));
                r = await dbBase.LoadAsync(f);
                Assert.IsTrue(r.Count() == 1);

                f = new FilterA4O();
                f.WhereId(10, 11, 12);
                r = await dbBase.LoadAsync(f);
                Assert.IsTrue(r.Count() == 0);

                f.WhereId(1, 11, 2);
                r = await dbBase.LoadAsync(f);
                Assert.IsTrue(r.Count() == 2);

                f.WhereId(true, 1, 11);
                r = await dbBase.LoadAsync(f);
                Assert.IsTrue(r.Count() == 1);
                Assert.IsTrue(r[0].Id != 1);
            }

        }

        [TestMethod()]
        public void LoadTest()
        {
            Task.Run(() => LoadTestAsync()).GetAwaiter().GetResult();

        }

        
        public async void LoadTestAsync()
        {

            {
                DropTables("TEST");
                var el1 = CreateElement();
                dbBase.SetTableInfo("TEST", el1.Item2);
                //DBBase dBBase = new DBBase("TEST", el1.Item2);
                //dBBase.Initialize();

                await dbBase.SaveAsync(el1.Item1);
                el1.Item1.Date = DateTime.Today.AddDays(-100);
                await dbBase.SaveAsync(el1.Item1);

                var el2 = CreateElement();
                await dbBase.SaveAsync(el2.Item1);

                var r = await dbBase.LoadAsync(null);
                Assert.IsNotNull(r);
                Assert.IsTrue(r.Count() == 2);
                Assert.IsTrue(r[0].Equals(el1.Item1, el1.Item2));
                Assert.IsTrue(r[1].Equals(el2.Item1, el1.Item2));

            }

        }

        [TestMethod()]
        public void DeleteTest()
        {
            Task.Run(() => DeleteTestAsync()).GetAwaiter().GetResult();
        }

        public async void DeleteTestAsync()
        {


            DropTables("TEST");
            var el1 = CreateElement();

            dbBase.SetTableInfo("TEST", el1.Item2);
            //DBBase dBBase = new DBBase("TEST", null);
            //dBBase.Initialize();
            await dbBase.SaveAsync(el1.Item1);

            var el2 = CreateElement();
            await dbBase.SaveAsync(el2.Item1);
            var r = await dbBase.LoadAsync(null);
            Assert.IsTrue(r.Count() == 2);
            await dbBase.DeleteAsync(el1.Item1.Id);
            r = await dbBase.LoadAsync(null);
            Assert.IsTrue(r.Count() == 1);
            await dbBase.DeleteAsync(el2.Item1.Id);
            r = await dbBase.LoadAsync(null);
            Assert.IsTrue(r.Count() == 0);


        }

        [TestMethod()]
        public void TestMultiResult()
        {


            string sql = "SELECT distinct id FROM TEST order by id ;" +
                "SELECT distinct id FROM TEST order by id desc ;";

            dbBase.ExecuteQuery(sql, (p) =>
            {
                do
                {
                    while (p.Read())
                    {
                        var a = p[0];
                    }
                } while (p.NextResult());

            });



        }
        [TestMethod()]
        public void InsertTest()
        {
            Task.Run(() => InsertTestAsync()).GetAwaiter().GetResult();

        }
        public async void InsertTestAsync()
        {


            DropTables("TEST");
            var el1 = CreateElement();
            dbBase.SetTableInfo("TEST", el1.Item2);
            //DBBase dBBase = new DBBase("TEST", el1.Item2);
            //dBBase.Initialize();


            await dbBase.SaveAsync(el1.Item1);
            var r = await dbBase.LoadAsync(null);
            Assert.IsTrue(r[0].Equals(el1.Item1));




        }






        private static (ElementA4O, List<DefinitionValueDto>) CreateElement()
        {

            {
                ElementA4O elementDB = CreateBaseElement();
                elementDB.Values = new List<ElementValueA4ODto>();

                ElementValueA4ODto val1 = new ElementValueA4ODto()
                {
                    DateVal = DateTime.Today,
                    FloatVal = 1.1,
                    Idx = 0,
                    InfoData = 1,
                    IntVal = 1,
                    StringVal = "1"

                };
                elementDB.Values.Add(val1);
                ElementValueA4ODto val2 = new ElementValueA4ODto()
                {
                    DateVal = DateTime.Today.AddDays(1),
                    FloatVal = 2.2,
                    Idx = 1,
                    InfoData = 2,
                    IntVal = 2,
                    StringVal = "2"

                };
                elementDB.Values.Add(val2);
                ElementValueA4ODto val3 = new ElementValueA4ODto()
                {
                    Idx = 3,
                    InfoData = 2,
                    DateVal = null,
                    FloatVal = null,
                    IntVal = null,
                    StringVal = "null"

                };
                elementDB.Values.Add(val3);
                List<DefinitionValueDto> def = new List<DefinitionValueDto>();
                def.AddDefinitionValue(new DefinitionValueDto() { ColumnName = "QQQ", IdColumn = 1, TableName = DefinitionValueConst.SINGLE_VALUE_TABLE_NAME },
                    new DefinitionValueDto() { ColumnName = "TTT", IdColumn = 2, TableName = "TABLE" });

                return (elementDB, def);
            }
        }

        private static ElementA4O CreateBaseElement()
        {
            return new ElementA4O()
            {
                Date = DateTime.Today,
                Deleted = false,
                IdParent = 1,
                ElementNameParent = "$ROOT"
            };
        }

        private void DropTables(string tableName)
        {
            try
            {
                dbBase.ExecuteNoQuery($"drop table {tableName}", null);
            }
            catch { }
            try
            {
                dbBase.ExecuteNoQuery($"drop table {tableName}_VAL", null);
            }
            catch { }
        }

        [TestMethod()]
        public void CreateBaseTableTest()
        {

            List<DefinitionValueDto> definitionValues = new List<DefinitionValueDto>();
            definitionValues.Add(new DefinitionValueDto()
            {
                ColumnName = "A1",
                IdColumn = 123,
                TableName = DefinitionValueConst.SINGLE_VALUE_TABLE_NAME,
            });
            definitionValues.Add(new DefinitionValueDto()
            {
                ColumnName = "A2",
                IdColumn = 1233,
                TableName = DefinitionValueConst.SINGLE_VALUE_TABLE_NAME,
            });
            definitionValues.Add(new DefinitionValueDto()
            {
                ColumnName = "T1",
                IdColumn = 1234,
                TableName = "T",
            });
            definitionValues.Add(new DefinitionValueDto()
            {
                ColumnName = "T2",
                IdColumn = 1235,
                TableName = "T",

            });
            dbBase.SetTableInfo("TEST", definitionValues);
            //dBBase.Initialize();
            Assert.IsTrue(dbBase.ExistsTable());
            Assert.IsTrue(dbBase.ExistsTable("TEST_VALS"));
            Assert.IsTrue(dbBase.ExistsView("TEST_VALSV"));

            dbBase.ExecuteNoQuery("drop table TEST", null);
            dbBase.ExecuteNoQuery("drop table TEST_VALS", null);
            dbBase.ExecuteNoQuery("drop view TEST_VALSV", null);




        }
    }
}