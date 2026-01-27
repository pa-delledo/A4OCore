using A4OCore.Models;
using A4OCore.Store;
using A4OCore.Store.DB.SQLLite;
using A4OCore.Utility;
using A4ODto;
using A4ODto.Action;
using A4ODto.View;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace A4OCore.BLCore
{
    public abstract class ElementBLA4O : IElementBLA4O


    {

        public static T GetEnumOptional<T>(string valueName) where T : struct, Enum
        {
            if (!Enum.TryParse<T>(valueName, true, out T result))
            {
                // 3. Eccezione specifica con dettagli utili
                throw new ArgumentException($"Il valore '{valueName}' non è un nome valido per l'enum {typeof(T).Name}.");
            }

            return result; // Nessun cast necessario
            //return Enum.TryParse<T>(valueName, true, out T result) ? result : null;
        }
        public string GetOptionSetDescription(int idElement, string code)
        {
            BaseDesignDto baseDesignDto = null;
            return GetOptionSetDescription(idElement, code, ref baseDesignDto);
        }
        public string GetOptionSetDescription(int idElement, string code, ref BaseDesignDto baseDesignDto)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            baseDesignDto = baseDesignDto ?? this.Design.ItemsDesignBase.FirstOrDefault(x => x.IdElement == idElement);
            return baseDesignDto?.OptionsSetValues[code];
        }

        public virtual void OnOpen(bool isNew)
        {
        }

        public virtual List<ActionDto> ActionAfterSave()
        {
            List<ActionDto> result = new();
            result.Add(
                ActionConverter.CreateNavigateBackAction()
            //ActionConverter.CreateNavigateAction("/element/" + this.CurrentElement.ElementName)
            );
            return result;

        }
        //public ElementBLA4O(IA4O_CheckEnumRepository checkEnum)
        //{
        //    this.CheckEnum = checkEnum;

        //}

        public const int SingleValueTable = DefinitionValueConst.SINGLE_VALUE_TABLE_VALUE;
        public const string SingleNameTable = DefinitionValueConst.SINGLE_VALUE_TABLE_NAME;

        //public static  bool Initialized {  get; set; }
        public abstract DesignElement Design { get; }


        public ElementA4ODto NewElementByParent( long idParent, long forceId = -1)
        {
            
            ElementA4O element = new ElementA4O()
            {
                ElementName = this.Design.ElementName,
                ElementNameParent = this.Design.ElementNameParent,
                IdParent = idParent,
                Date = DateTime.Today,
                DateChange = DateTime.Now,
                Deleted = false,
                Id = forceId

            };
            return element;
        }


        public List<ViewValueDto> GetElementsViewModel(List<(int idElemet, IEnumerable<int>? idx)>? filterElements = null)
        {
            List<ViewValueDto> designValueDtos = new List<ViewValueDto>();
            if (this.CurrentElement is null) return designValueDtos;
            DesignElement design = this.Design;
            this.AlignCurrentElement();

            var values = this.CurrentElement.Values.OrderBy(x => x.InfoData);
            BaseDesignDto itemDesign = design.ItemsDesignBase[0];
            foreach (ElementValueA4ODto? val in values)
            {
                if (itemDesign?.InfoData != val.InfoData)
                {
                    itemDesign = design.ItemsDesignBase.FirstOrDefault(x => x.InfoData == val.InfoData);
                }
                if (itemDesign is null) continue;
                bool skip = false;
                skip = SkipCurrentItem(filterElements, itemDesign.IdElement, val.Idx);

                ViewValueDto designValueDto = new ViewValueDto();
                CloneUtility.FillFromParent(itemDesign, ref designValueDto);
                designValueDto.Idx = val.Idx;
                designValueDto.Value = val.ToStringA4O();
                SetDesignValue(val!, ref designValueDto);
                CustomizeElementView(designValueDto);
                if (designValueDto.DesignType == ValueDesignType.LINK || designValueDto.DesignType == ValueDesignType.VIEW)
                {
                    var par= SetFilterParams(designValueDto);
                    
                    designValueDto.LinkJsonFilter = par?.ToJsonString();
                    
                }
                designValueDtos.Add(designValueDto);
            }
            return designValueDtos;
        }

        
        /// <summary>
        /// ex:
        /// return new JsonObject
    ///{
    ///    ["Nome"] = "Mario",
    ///    ["Eta"] = 30
    ///};
    /// </summary>
    /// <param name="designValueDto"></param>
    /// <returns></returns>
    public virtual JsonObject SetFilterParams(ViewValueDto designValueDto) 
        {
            return null;
        }
        public virtual void CustomizeElementView(ViewValueDto designValueDto)
        {

        }
        public List<ElementValueA4ODto> GetElementsValue(JsonElement json)
        {

            List<ElementValueA4ODto> result = new List<ElementValueA4ODto>();
            // prendo la proprietà "values"
            if (!json.TryGetProperty("values", out JsonElement valuesElement))
            {
                return result;
            }

            if (!json.TryGetProperty("id", out JsonElement idProp))
            {
                return result;
            }

            var id = idProp.GetInt64();
            // verifico che sia un array
            if (valuesElement.ValueKind != JsonValueKind.Array)
                return result;

            DesignElement design = this.Design;
            //this.AlignCurrentElement();
            (int idElement, BaseDesignDto design) cache = (-1, null);
            foreach (JsonElement element in valuesElement.EnumerateArray())
            {
                if (!element.TryGetProperty("idElement", out JsonElement jIdElement)) continue;

                if (!jIdElement.TryGetInt32(out int idElement)) continue;

                if (idElement != cache.idElement)
                {
                    cache.idElement = idElement;
                    cache.design = design.ItemsDesignBase.FirstOrDefault(x => x.IdElement == idElement);
                }
                if (cache.design == null) continue;

                ElementValueA4ODto obj = new ElementValueA4ODto();
                obj.InfoData = cache.design.InfoData;
                obj.Id = id;
                int idx = 0;
                if (element.TryGetProperty("idx", out JsonElement jIdx))
                {
                    jIdx.TryGetInt32(out idx);
                }
                obj.Idx = idx;


                if (element.TryGetProperty("value", out JsonElement jVal))
                {



                    switch (cache.design.DesignType)
                    {
                        case ValueDesignType.FLOAT:
                            if (jVal.ValueKind == JsonValueKind.Number)
                            {
                                obj.FloatVal = jVal.GetDouble();
                            }
                            else
                            {
                                var valStr = jVal.GetString();
                                if (double.TryParse(valStr, out double intVal))
                                {
                                    obj.FloatVal = intVal;
                                }
                            }


                            break;
                        case ValueDesignType.INT:
                            if (jVal.ValueKind == JsonValueKind.Number)
                            {
                                obj.IntVal = jVal.GetInt32();
                            }
                            else
                            {
                                var valStr = jVal.GetString();
                                if (int.TryParse(valStr, out int intVal))
                                {
                                    obj.IntVal = intVal;
                                }
                            }

                            break;
                        case ValueDesignType.DATE:
                            {
                                {
                                    string val = jVal.GetString();
                                    if (DateTime.TryParse(val, out DateTime dt))
                                    {
                                        obj.DateVal = dt;
                                    }
                                    break;
                                }
                            }
                        case ValueDesignType.COMMENT:
                        case ValueDesignType.STRING:
                        case ValueDesignType.OPTIONSET:
                        case ValueDesignType.ATTACHMENT:
                        
                            obj.StringVal = jVal.GetString();
                            break;
                        case ValueDesignType.LINK:
                            {
                                string val = jVal.GetString();
                                obj.StringVal = val;
                                obj.IntVal = -1;
                                if (!string.IsNullOrEmpty(val))
                                {
                                    obj.IntVal = JsonSerializer.Deserialize<JsonElement>(val).GetProperty("id").GetInt64();
                                }
                            break;
                            }
                        default:
                            throw new Exception("NOT DEFINED TYPE!!!");
                            

                    }

                }
                result.Add(obj);
            }


            //BaseDesignDto itemDesign = design.ItemsDesignBase[0];

            return result;
        }



        private static bool SkipCurrentItem(List<(int idElemet, IEnumerable<int>? idx)>? filterElements, int idElement, int idxValue)
        {

            if (filterElements != null)
            {
                var indexFilter = filterElements.FindIndex(x => x.idElemet == idElement);
                if (indexFilter < 0) return true;
                if (filterElements[indexFilter].idx != null)
                {
                    if (!filterElements[indexFilter].idx!.Contains(idxValue))
                    {
                        return true;

                    }
                }
            }

            return false;
        }

        public ElementA4ODto NewElementByParent( ElementA4ODto parent, long forceId=-1)
        {
            return NewElementByParent(parent.Id, forceId);
        }



        public virtual List<MessageA4O> OnCheck()
        {
            return new List<MessageA4O>();
        }
        public virtual List<ActionDto>  OnChange(string elementName, int idx)
        {
            return new();
        }
        public abstract void OnAction(string actionName);
        public abstract void OnButton(string buttonName, int idx);
        public abstract void OnSave();
        public abstract void OnAfterSave();

        public virtual bool CanRead(ElementA4ODto element) => true;
        public virtual bool CanWrite(ElementA4ODto element) => true;

        //public virtual Dictionary<string , Func<Dictionary<string,object>, List<ElementA4O>>> Views { get
        //    {
        //        Dictionary<string, Func<Dictionary<string, object>, List<ElementA4O>>> res = new Dictionary<string, Func<Dictionary<string, object>, List<ElementA4O>>>();
        //        res.Add("DEF", (par) => this.StroreManager.Load(null) );
        //        return res;

        //    }
        //}

        public IA4O_CheckEnumRepository CheckEnum { get; set; }


        public void Initialize()
        {

        }
        
        private ElementA4ODto _currentElement = null;
        public ElementA4ODto CurrentElement
        {
            get
            {
                return _currentElement;
            }
            set
            {
                if (value is null)
                {
                    _currentElement = null;
                    return;
                }
                if (string.IsNullOrEmpty(value.ElementNameParent) || (value.ElementNameParent != ElementA4ODto.ROOT_NAME && value.IdParent == 0))
                    throw new Exception("please select the parent!!!");
                if (value.ElementName == null)
                {
                    value.ElementName = this.Design.ElementName;
                }
                if (value.ElementName != this.Design.ElementName)
                    throw new Exception("Wrong elementName");
                this._currentElement = value;
            }
        }

        public void CheckEnumerator()
        {
#if DEBUG
            //IA4O_CheckEnumRepository checkEnum = new A4O_CheckEnumRepository();
            CheckEnum.CheckEnumChanged(this.Design.EnumItems, this.Design.ElementName, false);
            CheckEnum.CheckEnumChanged(this.Design.EnumTable, this.Design.ElementName, true);
#endif

        }
        private void CommontCreation()
        {
            if (!Cache.CacheInstance.CurrentA4OCache.DesignCacheContainsKey(Design.ElementName))
            {
                Cache.CacheInstance.CurrentA4OCache.DesignCacheSet(Design.ElementName, () => this.Design);

            }


        }
        public IStoreA4O StoreManager { get; set; }
        public bool CurrentIsVoid
        {
            get
            {
                return this.CurrentElement == null;
            }
        }

        public ElementBLA4O(IStoreA4O storeManager, IA4O_CheckEnumRepository checkEnum)
        {
            this.CheckEnum = checkEnum;
            this.StoreManager = storeManager;
            CommontCreation();
            if (StoreManager is DBBase bBase)
            {
                var colDes = this.Design.ItemsDesignBase.Where(x => x.OnlystoredElement()).Select(x => new DefinitionValueDto() { ColumnName = this.Design.GetStringElementFromId(x.IdElement), TableName = this.Design.GetStringTableFromId(x.Table), IdColumn = x.InfoData }).ToList();
                //var res = A4OCore.Store.ManagerA4O.CurrentStoreA4O();
                bBase.SetTableInfo(Design.ElementName, colDes);
            }
            //SetCurrent(new ElementA4O());

            //AlignCurrentElement();

        }

        //public ElementValueA4O this[EnumElement enumElement]
        //{
        //    get
        //    {
        //        int intEnumElement = enumElement;
        //        var valueDesing = Design.ItemsDesignBase.First(x => x.IdElement == intEnumElement);
        //        return CurrentElement?.Values.FirstOrDefault(x => x.InfoData == valueDesing.InfoData && x.Idx == 0);
        //    }
        //}
        public ElementValueA4ODto this[string enumElement, int idx = 0]
        {
            get
            {
                int intEnumElement = this.Design.EnumItems[enumElement];
                var valueDesing = Design.ItemsDesignBase.First(x => x.IdElement == intEnumElement);
                return CurrentElement?.Values.FirstOrDefault(x => x.InfoData == valueDesing.InfoData && x.Idx == idx);
            }
        }

        
        public ElementValueA4ODto this[int intEnumElement, int idx = 0]
        {
            get
            {
                var valueDesing = Design.ItemsDesignBase.First(x => x.IdElement == intEnumElement);
                var result= CurrentElement?.Values.FirstOrDefault(x => x.InfoData == valueDesing.InfoData && x.Idx == idx);
                if(CurrentElement is not null && result is null)
                {
                    result = new ElementValueA4ODto() { InfoData = valueDesing.InfoData, Idx = idx, Id = this.CurrentElement.Id };
                    CurrentElement?.Values.Add(result);
                }
                return result;
            }
        }
        public IEnumerable<ElementValueA4ODto> GetElementsSingle()
        {
            return GetElementsTable(SingleNameTable);
        }
        public IEnumerable<ElementValueA4ODto> GetElementsTable(string enumTable)
        {
            int intEnumTable = this.Design.EnumTable[enumTable];
            return GetElementsTable(intEnumTable);

        }

        public IEnumerable<ElementValueA4ODto> GetElementsTable(int intEnumTable)
        {
            IEnumerable<int> InfoDataCurrentTable = Design.ItemsDesignBase.Where(x => x.Table == intEnumTable).Select(x => x.InfoData);
            var found = CurrentElement?.Values.Where(x => InfoDataCurrentTable.Contains(x.InfoData));

            return found;
        }

        public IEnumerable<ElementValueA4ODto> GetElementsTable(int enumTable, int idx)
        {

            int intEnumTable = enumTable;
            IEnumerable<int> InfoDataCurrentTable = Design.ItemsDesignBase.Where(x => x.Table == intEnumTable).Select(x => x.InfoData);
            IEnumerable<ElementValueA4ODto>? res = CurrentElement?.Values.Where(x => x.Idx == idx && InfoDataCurrentTable.Contains(x.InfoData));
            if ((res?.Count() ?? 0) == 0) return null;


            if (res.Count() == InfoDataCurrentTable.Count()) return res;
            var InfoDataFound = res.Select(x => x.InfoData).ToList();
            List<ElementValueA4ODto> additional = new List<ElementValueA4ODto>();
            foreach (var missingElement in InfoDataCurrentTable.Except(InfoDataFound))
            {
                var toAdd = new ElementValueA4ODto { InfoData = missingElement, Idx = idx, Id = CurrentElement.Id };
                additional.Add(toAdd);
                this.CurrentElement.Values.Add(toAdd);
            }

            return res.Union(additional);

        }


        public void AlignCurrentElement()
        {
            if (CurrentElement is null) return;
            List<ElementValueA4ODto> allValues = CurrentElement.Values;
            List<ElementValueA4ODto> toAdd = new List<ElementValueA4ODto>();
            foreach (var groupDesign in Design.ItemsDesignBase.GroupBy(x => x.Table))
            {

                int maxNumValues = 0;
                int currentTable = groupDesign.Key;
                var allValuesGroup = allValues.Where(x => UtilityDesign.GetTableFromInfoData(x.InfoData) == currentTable).ToList();
                if (currentTable != SingleValueTable && allValuesGroup.Count > 0)
                {
                    maxNumValues = allValuesGroup?.Max(x => x.Idx) ?? -1;
                    maxNumValues++;
                }
                if(currentTable == SingleValueTable)
                {
                    maxNumValues = 1;
                }
                var rowsDesignToAdd = groupDesign.ToList();
                toAdd.AddRange(ValuesToAddForAlignTable(allValuesGroup, rowsDesignToAdd, maxNumValues));

            }
            CurrentElement.Values.AddRange(toAdd);
        }

        private List<ElementValueA4ODto> ValuesToAddForAlignTable(List<ElementValueA4ODto> allValues, List<BaseDesignDto> rowsDesignToAdd, int maxNumValues)
        {
            List<ElementValueA4ODto> toAdd = new List<ElementValueA4ODto>();
            foreach (var design in rowsDesignToAdd)
            {
                int InfoData = design.InfoData;
                var allIndexPresent = allValues.Where(x => x.InfoData == InfoData).Select(x => x.Idx);
                List<int> allPossibleIndex = Enumerable.Range(0, maxNumValues).ToList();
                foreach (var idx in allPossibleIndex.Except(allIndexPresent))
                {
                    toAdd.Add(new ElementValueA4ODto { InfoData = InfoData, Idx = idx, Id = CurrentElement.Id });
                }

            }
            return toAdd;
        }

        //public ElementBLA4O()
        //{
        //    //this.CheckEnum=checkEnum;
        //    CommontCreation();

        //    CurrentElement = new ElementA4O()
        //    {
        //        Date = DateTime.Today,
        //        Deleted = false,
        //        ElementNameParent = Design.ElementNameParent,
        //        IdParent = 1
        //    };

        //}

        public async Task LoadByIdAsync(long id)
        {
            if (!this.Design.IsStorable) throw new Exception("Element " + this.Design.ElementName + " is not storable");
            var r = await StoreManager.LoadAsync(new FilterA4O().WhereId(id));
            if (r == null || r.Count() == 0)
            {
                CurrentElement = null;
                return;
            }
            CurrentElement = r[0];
        }
        public List<ElementA4ODto> Load(FilterA4O filter)
        {
            return this.LoadAsync(filter).GetAwaiter().GetResult();
        }
        public List<ElementA4ODto> LoadByIds(params long[] ids)
        {
            return LoadByIdsAsync(ids).GetAwaiter().GetResult();
        }
        public ElementA4ODto LoadById(long id)
        {
            return LoadByIdsAsync(new[] { id}).GetAwaiter().GetResult().FirstOrDefault();
        }
        public async Task<List<ElementA4ODto>> LoadByIdsAsync(params long[] ids)
        {
            var filter = new FilterA4O();
            filter.WhereId(ids);
            return await this.LoadAsync(filter);
        }

        public async Task<List<ElementA4ODto>> LoadAsync(FilterA4O filter)
        {
            if (!this.Design.IsStorable) throw new Exception("Element " + this.Design.ElementName + " is not storable");
            List<ElementA4ODto> r = await StoreManager.LoadAsync(filter);
            return r;
        }

        public void Save()
        {
            Task.Run(() => SaveAsync()).GetAwaiter().GetResult();
        }
        public async Task SaveAsync()
        {
            if (!this.Design.IsStorable) throw new Exception("Element " + this.Design.ElementName + " is not storable");
            await StoreManager.SaveAsync(CurrentElement);
        }

        public void Delete()
        {
            Task.Run(() => DeleteAsync()).GetAwaiter().GetResult();
        }
        public async Task DeleteAsync()
        {
            if (!this.Design.IsStorable) throw new Exception("Element " + this.Design.ElementName + " is not storable");
            await StoreManager.DeleteAsync(CurrentElement);
        }

        public void SetDate(DateTime dat)
        {
            CurrentElement.Date = dat;
        }
        public void SetIsDeleted(bool del)
        {
            CurrentElement.Deleted = del;
        }

        public void SetValue(int e, string val)
        {
            ISetValue(SingleValueTable, e, 0, val);
        }



        public void SetLinkValue(int e, string elementName, long elementId)
        {
            ISetValue(SingleValueTable, e, 0, (elementName, elementId));
        }


        public List<CellViewA4ODto> GetRowSingle(ElementA4ODto elementA4O, params int[] elm)
        {
            return GetRowSingle(elementA4O, null, elm);
        }


        public List<CellViewA4ODto> GetCellViewA4O(ElementA4ODto elementA4O, params int[] elm)
        {
            var res = GetCellViewA4O(elementA4O, SingleValueTable, null, elm);
            return res.Count() > 0 ? res[0] : new List<CellViewA4ODto>();
        }

        public List<CellViewA4ODto> GetCellViewA4O(ElementA4ODto elementA4O, Action<ElementA4ODto, int, CellViewA4ODto> updateCell, params int[] elm)
        {
            Action<ElementA4ODto, int, int, CellViewA4ODto> up = (a, b, c, d) => updateCell(a, b, d);
            var res = GetCellViewA4O(elementA4O, SingleValueTable, up, elm);
            return res.Count() > 0 ? res[0] : new List<CellViewA4ODto>();
        }

        public List<List<CellViewA4ODto>> GetCellViewA4O(ElementA4ODto elementA4O, int table, params int[] elm)
        {
            return GetCellViewA4O(elementA4O, table, null, elm);
        }
        public List<List<CellViewA4ODto>> GetCellViewA4O(ElementA4ODto elementA4O, int table, Action<ElementA4ODto, int, int, CellViewA4ODto> updateCell, params int[] elm)
        {

            var res = new List<List<CellViewA4ODto>>();
            if (elm.Count() == 0) return res;

            this.CurrentElement = elementA4O;
#if DEBUG

            var filtered = elm.Where(x => this.Design.ItemsDesignBase.Any(y => y.Table == table));
            if (filtered.Count() != elm.Length) throw new Exception("all elm must be of table " + table.ToString());
#endif
            int rowsNumber = this.GetElementsTable(table).Max(x => x.Idx);
            List<int> allPossibleIndex = Enumerable.Range(0, rowsNumber).ToList();

            foreach (var idx in allPossibleIndex)
            {
                var toAdd = new List<CellViewA4ODto>();
                foreach (var ele in elm)
                {
                    this.AddRow(table);
                    var currVal = this[ele, idx];

                    string v = currVal?.StringVal ??
                        currVal?.IntVal?.ToString() ??
                        currVal?.DateVal?.ToString() ??
                        currVal?.FloatVal?.ToString() ??
                        string.Empty;

                    var toAddCell = new CellViewA4ODto()
                    {
                        Value = v,
                    };
                    if (updateCell != null)
                    {
                        updateCell(elementA4O, ele, idx, toAddCell);
                    }
                    toAdd.Add(toAddCell);
                }
                res.Add(toAdd);
            }
            return res;
        }

        private void AddRow(int table)
        {
            var allValues = this.CurrentElement.Values;
            var maxIdx = allValues.Where(x => UtilityDesign.GetTableFromInfoData(x.InfoData) == table).Max(x => x.Idx);
            var allDesignTable = this.Design.ItemsDesignBase.Where(x => x.Table == table).ToList();
            allValues.AddRange(ValuesToAddForAlignTable(allValues, allDesignTable, maxIdx + 1));

        }

        public List<CellViewA4ODto> GetRowSingle(ElementA4ODto elementA4O, Action<ElementA4ODto, int, CellViewA4ODto> updateCell, params int[] elm)
        {
            var res = new List<CellViewA4ODto>();
            this.CurrentElement = elementA4O;
            foreach (var ele in elm)
            {


                var toAdd = new CellViewA4ODto();

                if (updateCell != null)
                {
                    updateCell(elementA4O, ele, toAdd);
                }
                else
                {

                    ElementValueA4ODto current = this[ele];

                    string v = current?.StringVal +
                    current?.IntVal?.ToString() +
                    current?.DateVal?.ToString() +
                    current?.FloatVal?.ToString() +
                    string.Empty;
                    toAdd.Value = v;
                }
                res.Add(toAdd);
            }
            return res;
        }



        // Overload per intero (int)
        public void SetValue(int e, long? val)
        {
            ISetValue(SingleValueTable, e, 0, val);
        }

        // Overload per data (DateTime)
        public void SetValue(int e, DateTime? val)
        {
            ISetValue(SingleValueTable, e, 0, val);
        }



        // Overload per double (se necessario)
        public void SetValue(int e, double? val)
        {
            ISetValue(SingleValueTable, e, 0, val);
        }

        // Overload per int nullable
        public void SetValue(int t, int e, int idx, long? val)
        {
            ISetValue(t, e, idx, val);
        }

        public void SetLinkValue(int t, int e, int idx, string elementName, long elementId)
        {
            ISetValue(t, e, idx, (elementName, elementId));
        }


        // Overload per DateTime nullable
        public void SetValue(int table, int element, int idx, DateTime? val)
        {
            ISetValue(table, element, idx, val);
        }

        // Overload per float nullable

        // Overload per double nullable
        public void SetValue(int table, int element, int idx, double? val)
        {
            ISetValue(table, element, idx, val);
        }

        // Overload per bool nullable
        public void SetValue(int t, int e, int idx, string val)
        {
            ISetValue(t, e, idx, val);
        }

        private void ISetValue(int table, int element, int idx, object val)
        {
            if (table == SingleValueTable && idx != 0) throw new Exception("Single Value table !!! only idx=0");

            var des = Design.ItemsDesignBase.First(x => x.IdElement == element && x.Table == table);
            var toAdd = new ElementValueA4ODto()
            {
                Idx = idx,
                InfoData = des.InfoData,
                Id = this.CurrentElement.Id
            };
            switch (des.DesignType)
            {
                case ValueDesignType.DATE:
                    toAdd.DateVal = val as DateTime?;
                    break;
                case ValueDesignType.FLOAT:
                    toAdd.FloatVal = val as Double?;
                    break;
                case ValueDesignType.COMMENT:
                    toAdd.StringVal = val as string;
                    break;
                case ValueDesignType.STRING:
                    toAdd.StringVal = val as string;
                    break;
                case ValueDesignType.INT:
                    toAdd.IntVal = val as long?;
                    break;
                case ValueDesignType.LINK:
                    (string, long)? currVal = val as (string, long)?;
                    if (currVal.HasValue)
                    {
                        toAdd.IntVal = currVal.Value.Item2;
                        toAdd.StringVal = currVal.Value.Item1;
                    }
                    break;
            }
            this.CurrentElement.Values.Add(toAdd);
        }
        /***
         * 
         * 
         */
        public void SetDesignValue(ElementValueA4ODto val, ref ViewValueDto designValueDto)
        {

        }

        public virtual List<ActionDto> OnPreSave()
        {
            return [];
        }



        //public IStoreA4O StroreManager
        //{
        //    get
        //    {
        //        var colDes = this.Design.ItemsDesignBase.Where(x => UtilityBL.OnlystoredElement(x)).Select(x => new DefinitionValue() { ColumnName = x.IdElementName, TableName = x.TableName, IdColumn = x.InfoData }).ToList();
        //        var res = A4OCore.Store.ManagerA4O.CurrentStoreA4O(Design.ElementName, colDes);
        //        return res;
        //    }
        //}



    }
}
