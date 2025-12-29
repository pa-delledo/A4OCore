using System.Runtime.Loader;

namespace A4OCore.Utility
{
    internal class ReflectionUtility
    {

        public static List<Type> GetAllChildrenClass<T>(string projectName)
        {
            var cl = typeof(T);

            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies();
            var d1 = derivedTypes.Where(x => string.Equals(x.GetName().Name, projectName, StringComparison.CurrentCultureIgnoreCase));
            if (!d1.Any())
            {

                var assemblyPath = Path.Combine(AppContext.BaseDirectory, projectName + ".dll");
                if (File.Exists(assemblyPath))
                {
                    System.Reflection.Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    return assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && cl.IsAssignableFrom(t)).ToList();
                }
            }
            //    d1 = derivedTypes.Where(x => x.GetName().Name.ToLowerInvariant().Contains("test"));
            var d2 = d1.SelectMany(a => a.GetTypes());

            //var d3 =d2     .Where(t => t.IsClass && !t.IsAbstract && cl.IsInstanceOfType(t) ).ToList();
            var d3 = d2.Where(t => t.IsClass && !t.IsAbstract && cl.IsAssignableFrom(t)).ToList();
            return d3;
        }
    }
}
