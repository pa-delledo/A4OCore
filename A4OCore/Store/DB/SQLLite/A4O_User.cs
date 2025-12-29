using A4OCore.Cfg;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Dynamic;

namespace A4OCore.Store.DB.SQLLite
{
    public class A4O_User
    {
        ConfigurationA4O Cfg;
        private const string SQL_USER_ALL_COLUMNS = " name, mail, roles ";

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
       " PRIMARY KEY (Mail) " +
       " ); ";
        SqliteConnection Connection => UtilitySqlLite.GetConnection(Cfg);



        public void Insert(User user)
        {
            using var conn = Connection;
            conn.Open();

            string sql = $"INSERT INTO  {SQL_USER_TABLE_NAME} ({SQL_USER_ALL_COLUMNS})" +
                "VALUES (@name, @mail, @roles);";
            string roles = GetRolesString(user);
            conn.Execute(sql, new { name = user.Name, mail = user.Mail, roles = roles });
        }

        private const string SEP_ROLES = ",";
        private static string GetRolesString(User user)
        {
            return (user.Roles?.Length ?? 0) == 0 ? string.Empty : string.Join(SEP_ROLES, user.Roles.Select(x => x.ToString()));
        }
        private static A4ORoles[] GetRolesFromString(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles)) return Array.Empty<A4ORoles>();

            return roles.Split(SEP_ROLES).
            Select(x => Enum.TryParse<A4ORoles>(x, true, out A4ORoles r) ? (A4ORoles?)r : null)
                .Cast<A4ORoles>().ToArray();

        }
        public void Update(User user)
        {
            using var conn = Connection;

            string sql = $@"
            UPDATE {SQL_USER_TABLE_NAME}
            SET Name = @name,
                Roles=@roles
            WHERE mail= @mail;";
            conn.Execute(sql, new { name = user.Name, mail = user.Mail, roles = GetRolesString(user) });

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
            return new User { Mail = user.Mail, Name = user.Name, Roles = GetRolesFromString(user.Roles) };
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