using A4OCore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace A4OCore.BLCore
{
    public interface IBLRepository
    {
        ElementBLA4O GetElement(string name);
        String[] GetAllElementsName();
        String[] GetAllActionsName();
        String[] GetAllViewsName();
        IActionBLA4O GetAction(string name);

        IViewBLA4O GetView(string name);
    }

    public class BLRepository : IBLRepository
    {
        IServiceProvider provider;
        private readonly IServiceScopeFactory _scopeFactory;
        public BLRepository(IServiceProvider provider, IServiceScopeFactory scopeFactory)
        {
            this.provider = provider;
            this._scopeFactory = scopeFactory;
        }

        private Dictionary<string, int> AllElement = null;
        private Dictionary<string, int> AllAction = null;
        private Dictionary<string, int> AllView = null;
        public String[] GetAllElementsName()
        {
            using var scope = _scopeFactory.CreateScope();
            AllElement = AllElement ?? MapElementBL(scope);
            return AllElement.Keys.ToArray();

        }
        public String[] GetAllActionsName()
        {
            using var scope = _scopeFactory.CreateScope();
            AllAction = AllAction ?? MapAction(scope);
            return AllAction.Keys.ToArray();

        }
        public String[] GetAllViewsName()
        {
            using var scope = _scopeFactory.CreateScope();
            AllView = AllView ?? MapView(scope);
            return AllView.Keys.ToArray();

        }
        public ElementBLA4O GetElement(string name)
        {
            using var scope = _scopeFactory.CreateScope();
            AllElement = AllElement ?? MapElementBL(scope);

            return scope.ServiceProvider.GetKeyedService<ElementBLA4O>(AllElement[name]);
        }
        public IActionBLA4O GetAction(string name)
        {
            using var scope = _scopeFactory.CreateScope();
            AllAction = AllAction ?? MapAction(scope);

            return scope.ServiceProvider.GetKeyedService<IActionBLA4O>(AllAction[name]);
        }
        public IViewBLA4O GetView(string name)
        {
            using var scope = _scopeFactory.CreateScope();
            AllView = AllView ?? MapView(scope);
            return scope.ServiceProvider.GetKeyedService<IViewBLA4O>(AllView[name]);
        }

        private static Dictionary<string, int> MapElementBL(IServiceScope scope)
        {
            Dictionary<string, int> allEl = new Dictionary<string, int>();
            int i = 0;
            while (true)
            {
                ElementBLA4O map = scope.ServiceProvider.GetKeyedService<ElementBLA4O>(i);
                if (map == null) break;
                allEl.Add(map.Design.ElementName, i++);
            }

            return allEl;
        }
        private static Dictionary<string, int> MapView(IServiceScope scope)
        {
            Dictionary<string, int> allEl = new Dictionary<string, int>();
            int i = 0;
            while (true)
            {
                IViewBLA4O map = scope.ServiceProvider.GetKeyedService<IViewBLA4O>(i);
                if (map == null) break;
                allEl.Add(map.ViewName, i++);
            }

            return allEl;
        }
        private static Dictionary<string, int> MapAction(IServiceScope scope)
        {
            Dictionary<string, int> allEl = new Dictionary<string, int>();
            int i = 0;
            while (true)
            {
                IActionBLA4O map = scope.ServiceProvider.GetKeyedService<IActionBLA4O>(i);
                if (map == null) break;
                allEl.Add(map.ActionName, i++);
            }

            return allEl;
        }





        //public ElementBLA4O Get(string name)
        //{
        //    return AllElementBLA4O[name];
        //}
    }
}
