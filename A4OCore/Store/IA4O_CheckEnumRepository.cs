using A4OCore.Store.DB;

namespace A4OCore.Store
{
    public interface IA4O_CheckEnumRepository
    {
        void Delete(A4O_CheckEnum entry);
        void Delete(string elementName, string enumString, bool isTable);
        Task DeleteAsync(A4O_CheckEnum e);
        Task DeleteAsync(string elementName, string enumString, bool isTable);
        Task<List<A4O_CheckEnum>> GetAllAsync(string filter, Dictionary<string, object> par = null);
        Task<A4O_CheckEnum?> GetByKey(string elementName, string enumString, bool isTable);
        void Insert(A4O_CheckEnum entry);
        Task InsertAsync(A4O_CheckEnum entry);
        void Update(A4O_CheckEnum entry);
        Task UpdateAsync(A4O_CheckEnum entry);
        internal void CheckEnumChanged(Dictionary<string, int> desingDictionary, string elementName, bool isTable);
    }
}