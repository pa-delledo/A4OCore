namespace A4OCore.Cfg
{
    public enum A4ORoles { admin }
    public class User
    {
        public String Mail;
        public String Name;
        public A4ORoles[] Roles;

        private static readonly AsyncLocal<User> _userId = new AsyncLocal<User>();

        public static User CurrentUser
        {
            get => _userId.Value;
            set => _userId.Value = value;
        }
    }
}




