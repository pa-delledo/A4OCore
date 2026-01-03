
using A4OCore.Store;
using A4OCoreTests.Design.famiglia;
using A4OCoreTests;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.Intrinsics.X86;

using A4OCore.Utility;
using Microsoft.Extensions.DependencyInjection;
using A4ODto.View;
using A4OCore.Models;

namespace A4OCore.BLCore.regina

{
    public class ReginaView : IViewBLA4O
    {

        public  string ViewName => "ReginaView";
        public ReginaView(ReginaBL regina)
        {
            this.regina = regina;
        }
        public string[] ColumnNames
        {
            get
            {
                var res = new List<string>();
                var e1 = "idElement";
                res.Add(e1);
                e1 ="Name" ;
                res.Add(e1);
                e1 = "Anno" ;
                res.Add(e1);
                e1 = "Giudizio";
                res.Add(e1);
                return res.ToArray();
            }

        }

        

        //[Inject]
       public ReginaBL regina { get; set; }

        public List<ViewRowDto> GetRows(Dictionary<string, object> parameters)
        {
            
            
            FilterA4O filterA4O = new FilterA4O();
            DateTime d = DateTime.Today.AddDays(-160);
            if (parameters != null && parameters.ContainsKey("date"))
            {
                d = parameters["date"] as DateTime? ?? d;
                
            }
            filterA4O.WhereDate(d, d);
            filterA4O.SetReultValues( regina,  ReginaBL.EnumReginaElement.nome.ToInt(), ReginaBL.EnumReginaElement.anno.ToInt(), ReginaBL.EnumReginaElement.giudizio.ToInt());
            
            var loaded=regina.StoreManager.Load(filterA4O);
            var result = new List<ViewRowDto>();
            foreach (var elementA4O in loaded)
            {
                List<CellViewA4ODto> rowTmp = regina.GetRowSingle(elementA4O, ReginaBL.EnumReginaElement.nome.ToInt(), ReginaBL.EnumReginaElement.anno.ToInt(), ReginaBL.EnumReginaElement.giudizio.ToInt());
                //regina.CurrentElement = elementA4O;
                ViewRowDto row= new ViewRowDto();
                row.ElementId = elementA4O.Id;
                row.ElementName = elementA4O.ElementName;

                var cell = new CellViewA4ODto()
                {
                    TypeElement=TypeCellViewBL.id,
                    Value = regina.CurrentElement.Id.ToString()
                };
                rowTmp.Insert(0,cell);
                //cell = new CellViewA4ODto()
                //{
                //    Value = regina[ReginaBL.EnumReginaElement.nome.ToInt()].StringVal
                //};
                //row.Add(cell);
                //cell = new CellViewA4ODto()
                //{
                //    Value = regina[ ReginaBL.EnumReginaElement.anno.ToInt()].IntVal.ToString()
                //};
                //row.Add(cell);
                //cell = new CellViewA4ODto()
                //{
                //    Value = regina[ReginaBL.EnumReginaElement.giudizio.ToInt()].IntVal.ToString()
                //};
                row.Values=rowTmp;
                result.Add(row);

            }
            //regina.StroreManager.Initialize();
            return result;
        }

        

        
        
    }
    [TestClass()]
    public class ReginaViewTest
    {
        ReginaView view;
        ReginaBL reg;
        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();
            //services.AddTransient<DBBase>();
            services.AddA4ODependencies();
            services.AddSingleton<ReginaView>();
            services.AddSingleton<ReginaBL>();
            var _provider = services.BuildServiceProvider();
            view = _provider.GetRequiredService<ReginaView>();
            reg = _provider.GetRequiredService<ReginaBL>();
            reg.CheckEnumerator();
            
        }
        [TestMethod]
        public void Test()
        {
            DateTime dateTime = DateTime.Today.AddDays(1000);

            var par = new Dictionary<string, object>();
            par.Add("date",dateTime);
            List<ViewRowDto> curr = view.GetRows(par);
            var allPrev = curr.Select(x => long.Parse(x.Values[0].Value)).ToArray();
            

            var prev= reg.LoadByIds(allPrev);
            foreach( var propa in prev)
            {
                reg.CurrentElement = propa;
                reg.Delete();
            }
                
            
            ReginaBL r = reg;
            reg.CurrentElement = new ElementA4O(ElementA4O.Root);
            r.SetDate(dateTime);
            r.SetValue(ReginaBL.EnumReginaElement.nome.ToInt(), "TEST");
            r.SetValue(ReginaBL.EnumReginaElement.anno.ToInt(), 2000);
            r.SetValue(ReginaBL.EnumReginaElement.colore.ToInt(), 1);
            r.SetValue(ReginaBL.EnumReginaElement.provenienza.ToInt(), "test View");
            r.SetValue(ReginaBL.EnumReginaElement.giudizio.ToInt(), 0);
            r.SetValue(ReginaBL.EnumReginaElement.marcata.ToInt(), 1);
            r.Save();


            
            curr=view.GetRows(par);
            Assert.IsTrue(curr.Count() == 1);

            r.Delete();
            curr = view.GetRows(par);
            Assert.IsTrue(curr.Count() == 0);

        }
    }
}