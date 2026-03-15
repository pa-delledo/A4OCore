using A4OCore.Cfg;

namespace A4OCore.Store
{
    public interface IA4O_User
    {
        void Delete(string mail);
        void Delete(UserA4O u);
        List<UserA4O> GetAll(string filter, Dictionary<string, object> par = null);
        UserA4O? GetByMail(string mail);
        void Insert(UserA4O user);
        void Update(UserA4O user);
    }
}