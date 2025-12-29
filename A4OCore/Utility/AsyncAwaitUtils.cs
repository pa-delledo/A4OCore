namespace A4OCore.Utility
{
    internal class AsyncAwaitUtils
    {
        internal static void Wait(Func<Task> asyncFunc)
        {
            Task.Run(() => asyncFunc()).GetAwaiter().GetResult();
        }
    }
}
