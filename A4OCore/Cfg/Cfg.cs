using Microsoft.Extensions.Configuration;


namespace A4OCore.Cfg
{
    public enum EnviromentEnum { PROD, TEST, INT, UnitTest }
    public enum StoreEnum { SqlLite, NotImplemented }
    public class ConfigurationA4O
    {
        public EnviromentEnum Enviroment = EnviromentEnum.TEST;
        public string SQLLiteFile;
        public StoreEnum StoreType = StoreEnum.SqlLite;
        public string[] BLProject;

        public string? PathBase { get; private set; }




        //private static ConfigurationA4O _cfg;
        //public static ConfigurationA4O CurrentConfiguration
        //{
        //    get { return _cfg; }
        //}
        //static Cfg()
        //{
        //    if (_cfg == null)
        //    {
        //        _cfg = Get("appsettings.json");
        //    }
        //}
        public static ConfigurationA4O Get(string fileName)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(fileName, optional: false, reloadOnChange: true)
            .Build();

            ConfigurationA4O res = new ConfigurationA4O();
            var appConfig = configuration.GetSection("O4A");

            if (!Enum.TryParse(typeof(EnviromentEnum), appConfig.GetSection("Enviroment").Value, true, out var enviroment))
            {
                enviroment = EnviromentEnum.TEST;
            }
            res.Enviroment = (EnviromentEnum)enviroment;



            if (!Enum.TryParse(typeof(StoreEnum), appConfig.GetSection("Enviroment").Value, true, out var storeEnum))
            {
                storeEnum = StoreEnum.SqlLite;
            }
            res.StoreType = (StoreEnum)storeEnum;

            res.SQLLiteFile = appConfig.GetSection("SQLLiteFile").Value;
            res.BLProject = appConfig.GetSection("BLProject").Value.Split(',', ';');
            res.PathBase = appConfig.GetSection("PathBase").Value;
            return res;


            //return appConfig;

        }



    }
}
