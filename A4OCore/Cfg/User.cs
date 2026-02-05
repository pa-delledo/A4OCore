namespace A4OCore.Cfg
{
    public enum A4ORoles { readOnly=1, admin = 333 }
    public class User
    {
        public String Mail;
        public String Name;
        public bool Enabled;
        public A4ORoles[] Roles;


        private static readonly AsyncLocal<User> _userId = new AsyncLocal<User>();

        public static User CurrentUser
        {
            get => _userId.Value;
            set => _userId.Value = value;
        }
    }
}




