using A4OCore.Cfg;
using A4OCore.Utility;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Dynamic;

namespace A4OCore.Store.DB.SQLLite
{
    public class A4O_User : IA4O_User
    {
        ConfigurationA4O Cfg;
        private const string SQL_USER_ALL_COLUMNS = " name, mail, roles ,Enabled";

        public A4O_User(ConfigurationA4O cfg)
        {
            this.Cfg = cfg;
        }

        public const string SQL_USER_TABLE_NAME = "A4O_User";
        public const string SQL_CREATE_USER = " CREATE TABLE " + SQL_USER_TABLE_NAME +
       " ( " +
       " Name TEXT, " +
       " Mail TEXT, " +
       //" Root TEXT, " +
       //" RootId INTEGER, " +
       " Roles TEXT, " +
       " Enabled INTEGER, " +
       " PRIMARY KEY (Mail) " +
       " ); ";
        SqliteConnection Connection => UtilitySqlLite.GetConnection(Cfg);



        public void Insert(User user)
        {
            using var conn = Connection;
            conn.Open();

            string sql = $"INSERT INTO  {SQL_USER_TABLE_NAME} ({SQL_USER_ALL_COLUMNS})" +
                "VALUES (@name, @mail, @roles,@enabled);";
            string roles = user.SerializeRoles();
            conn.Execute(sql, new { name = user.Name, mail = user.Mail, roles = roles ,enabled=user.Enabled?1:0 });
        }

        
        public void Update(User user)
        {
            using var conn = Connection;

            string sql = $@"
            UPDATE {SQL_USER_TABLE_NAME}
            SET Name = @name,
                Roles=@roles,
                Enabled=@enabled
            WHERE mail= @mail;";
            conn.Execute(sql, new { name = user.Name, mail = user.Mail, roles = user.SerializeRoles(), enabled=user.Enabled?1:0 });

        }

        public void Delete(User u)
        {
            if (u == null) return;

            Delete(u.Mail);
        }

        public void Delete(string mail)
        {
            using var conn = Connection;

            string sql = $@"
            DELETE FROM {SQL_USER_TABLE_NAME}
            WHERE mail = @mail;";

            conn.Execute(sql, new
            {
                mail = mail
            });


        }

        public User? GetByMail(string mail)
        {
            using var conn = Connection;

            string sql = $@"
            SELECT {SQL_USER_ALL_COLUMNS}
            FROM {SQL_USER_TABLE_NAME}
            WHERE mail = @mail";

            var user = conn.QueryFirstOrDefault(sql, new
            {
                mail = mail
            });
            if (user == null)
            {
                return null;
            }

            return GetUserFromDb(user);
        }

        private static User GetUserFromDb(dynamic user)
        {
            var result=new User { Mail = user.Mail, Name = user.Name, Enabled=user.Enabled>0 };
            result.SetRolesSerialized(user.Roles as string);
            return result;
        }


        public List<User> GetAll(string filter, Dictionary<string, object> par = null)
        {


            using var conn = Connection;

            string sql = $"SELECT {SQL_USER_ALL_COLUMNS} FROM {SQL_USER_TABLE_NAME} ";
            if (!string.IsNullOrEmpty(filter))
            {
                sql += " where " + filter;
            }
            var expando = new ExpandoObject() as IDictionary<string, object>;
            if (par != null)
            {
                foreach (var kv in par)
                    expando.Add(kv.Key, kv.Value);
            }


            List<dynamic> list = conn.Query(sql, expando).ToList();


            return list.Select(y => (User)GetUserFromDb(y)).ToList();
        }

    }
}