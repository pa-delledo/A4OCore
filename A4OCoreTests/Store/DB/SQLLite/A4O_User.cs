using Microsoft.VisualStudio.TestTools.UnitTesting;
using A4OCore.Store.DB.SQLLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using A4OCoreTests;
using A4OCore.Cfg;
using Microsoft.Extensions.DependencyInjection;
using A4OCore.Utility;

namespace A4OCore.Store.DB.SQLLite.Tests
{
    [TestClass()]
    public class A4O_UserTests
    {
        private const string MIA_MAIL = "aaa@gmail.com";
        A4O_User a4O_User;
        //private IKernel _kernel;
        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            var _provider = services.BuildServiceProvider();
            a4O_User = _provider.GetRequiredService<A4O_User>();
            //_kernel = new StandardKernel(new NinjectTestModule());
         
        }
        
        

        [TestMethod()]
        public void InsertTest()
        {

            //A4O_MapIdRepository  a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_User.Delete(MIA_MAIL);
            this.a4O_User.Insert(MioUser());
            var v = a4O_User.GetByMail(MIA_MAIL);
            Assert.IsTrue(v.Name == "io");

        }

        private static User MioUser()
        {
            return new Cfg.User() { Mail = MIA_MAIL, Name = "io", Roles = new Cfg.A4ORoles[] { Cfg.A4ORoles.admin } };
        }
        private static User MioUser2()
        {
            return new Cfg.User() { Mail = "aaa@a4o.a4o", Name = "user2", Roles = Array.Empty<Cfg.A4ORoles>() };
        }

        [TestMethod()]
        public void UpdateTest()
        {
            
            //A4O_MapIdRepository a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_User.Delete(MIA_MAIL);
            var toInsert = MioUser();
            a4O_User.Insert(toInsert);


            toInsert.Name = "io!!!!";
            a4O_User.Update(toInsert);
            var v = a4O_User.GetByMail(MIA_MAIL);
            Assert.IsTrue(v.Name == toInsert.Name);
            a4O_User.Delete(MIA_MAIL);
            
        }

        [TestMethod()]
        public void DeleteTest()
        {
            //A4O_MapIdRepository a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_User.Delete(MIA_MAIL);
            var toInsert = MioUser();
            a4O_User.Insert(toInsert);
            var v = a4O_User.GetByMail(MIA_MAIL);
            Assert.IsNotNull(v);
            a4O_User.Delete(MIA_MAIL);
            v = a4O_User.GetByMail(MIA_MAIL);
            Assert.IsNull(v);
        }

        [TestMethod()]
        public void GetAllTest()
        {
            var toInsert1 = MioUser();
            var toInsert2 = MioUser2();
            
            //A4O_MapIdRepository a4O_MapIdRepository = new A4O_MapIdRepository();
            a4O_User.Delete(toInsert1);
            a4O_User.Delete(toInsert2);

            var r =a4O_User.GetAll($"mail='{toInsert1.Mail}' or mail='{toInsert2.Mail}' ");
            Assert.IsTrue(r.Count==0);
            a4O_User.Insert(toInsert1);
            a4O_User.Insert(toInsert2);
            r = a4O_User.GetAll($"mail='{toInsert1.Mail}' or mail='{toInsert2.Mail}' ");
            Assert.IsTrue(r.Count == 2);
            a4O_User.Delete(toInsert1);
            a4O_User.Delete(toInsert2);


        }
    }
}