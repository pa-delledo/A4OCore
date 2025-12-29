using Microsoft.VisualStudio.TestTools.UnitTesting;
using A4OCore.Store.DB.SQLLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace A4OCore.Store.DB.SQLLite.Tests
{
    [TestClass()]
    public class FilterConditionSqlLiteManagerTests
    {
        

        [TestMethod()]
        public void ToSqlTestEqual()
        {
            List<SqliteParameter> parameters = new List<SqliteParameter>();
            string par1 = "A";
            string val1 = "B";
            var filter = FilterBase.Equal(par1, val1);

            string sql = FilterConditionSqlLiteManager.GenerateSqlFilter(filter, parameters);
            Assert.IsNotNull(sql);
            Assert.IsTrue(parameters[0].Value == val1 && parameters[0].ParameterName == "@" + par1);
            Assert.IsTrue(sql == $"{par1} = @{par1} ");
        }
        [TestMethod()]
        public void ToSqlTestNot()
        {
            List<SqliteParameter> parameters = new List<SqliteParameter>();
            string par1 = "A";
            
            var filter = FilterBase.Not(par1);

            string sql = FilterConditionSqlLiteManager.GenerateSqlFilter(filter, parameters);
            Assert.IsNotNull(sql);
            Assert.IsTrue(parameters.Count()==0);
            Assert.IsTrue(sql == $"not {par1}");
        }
        [TestMethod()]
        public void ToSqlTest()
        {
            List<SqliteParameter> parameters = new List<SqliteParameter>();

            var filter = FilterBase.And(
                FilterBase.Equal("A", "A"),
                FilterBase.Equal("B", "B"),
                FilterBase.Or(
                    FilterBase.Like("A", "A"),
                    FilterBase.LessThan("B", "B")
                    )

                );
            var sql = FilterConditionSqlLiteManager.GenerateSqlFilter(filter, parameters);
            Assert.IsNotNull(sql);

        }

        
    }
}