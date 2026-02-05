using A4OCore.Cfg;

namespace A4OCore.Store
{
    public interface IA4O_User
    {
        void Delete(string mail);
        void Delete(User u);
        List<User> GetAll(string filter, Dictionary<string, object> par = null);
        User? GetByMail(string mail);
        void Insert(User user);
        void Update(User user);
    }
}