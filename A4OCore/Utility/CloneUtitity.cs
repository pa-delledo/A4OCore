namespace A4OCore.Utility
{
    public static class CloneUtility
    {

        public static void FillFromChild<TChild, TParent>(TChild source, ref TParent dest)
            where TChild : TParent
        {

            var fields = typeof(TParent).GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            foreach (var prop in fields)
            {

                var value = prop.GetValue(source);
                prop.SetValue(dest, value);

            }


            var properties = typeof(TParent).GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            foreach (var prop in properties)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(source);
                    prop.SetValue(dest, value);
                }
            }

        }

        public static void FillFromParent<TParent, TChild>(TParent source, ref TChild dest)
            where TChild : TParent
        {

            var fields = typeof(TParent).GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            foreach (var prop in fields)
            {

                var value = prop.GetValue(source);
                prop.SetValue(dest, value);

            }


            var properties = typeof(TParent).GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            foreach (var prop in properties)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(source);
                    prop.SetValue(dest, value);
                }
            }

        }


        //public static T Clone<T>(T t)  where T : class
        //{
        //    return (T)t.MemberwiseClone();
        //}
    }
}
