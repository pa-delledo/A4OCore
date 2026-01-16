
using A4OCore.Models;
using A4ODto;
using A4ODto.View;
using System.Runtime.CompilerServices;
using System.Text.Json;


namespace A4OCore.Utility
{
    public static class ExtensionsA4O
    {
        public static bool Equals(this ElementA4ODto first, ElementA4ODto? other, List<DefinitionValueDto> definitionValues = null)
        {
            if (first == other) return true;
            bool res =
                first.Id == other.Id &&
                first.ElementNameParent == other.ElementNameParent &&
                first.IdParent == other.IdParent &&
                first.Deleted == other.Deleted;
            if (!res) return false;

            if (first.Values == other.Values) return true;
            if (first.Values == null || other.Values == null) return false;

            var thisOrd = first.Values.
                Where(x => definitionValues == null || (x != null && x.OnlystoredElement())).
                OrderBy(x => x.InfoData).ThenBy(x => x.Idx).ToList();
            var otherOrd = first.Values.
                Where(x => definitionValues == null || (x != null && x.OnlystoredElement())).
                OrderBy(x => x.InfoData).ThenBy(x => x.Idx).ToList();

            int count = thisOrd.Count();
            if (count != otherOrd.Count()) return false;

            var idx = count;
            while (idx-- > 0)
            {
                if (!thisOrd[idx].Equals(otherOrd[idx])) return false;
            }
            return true;

        }
        public static int ToInt<T>(this T e) where T : Enum
        {
            return Convert.ToInt32(e);
        }
        public static IEnumerable<ElementValueA4OAdditionalInfo> MappingToElementValueA4OAdditionalInfo(this IEnumerable<ElementValueA4ODto> list, DesignElement design)
        {
            return list.Select(x => x.ToElementValueA4OAdditionalInfo(design));
        }
        public static bool OnlystoredElement(this ElementValueA4ODto x)
        {
            var typeId = (ValueDesignType)UtilityDesign.GetTypeFromInfoData(x.InfoData);
            return UtilityBL.VALUES_TYPE_DESIGN_TO_SAVE.Contains(typeId);

        }
        public static ElementValueA4OAdditionalInfo ToElementValueA4OAdditionalInfo(this ElementValueA4ODto el, DesignElement design)
        {
            return new ElementValueA4OAdditionalInfo()
            {
                tableName = design.EnumTabletRev[UtilityDesign.GetTableFromInfoData(el.InfoData)],
                elementName = design.EnumElementRev[UtilityDesign.GetIdElementFromInfoData(el.InfoData)],
                typeElement = UtilityDesign.GetTypeFromInfoData(el.InfoData),
                val = el
            };
        }
        public static string ToStringA4O(this ElementValueA4ODto el)
        {
            var typeElement = UtilityDesign.GetTypeFromInfoData(el.InfoData);
            string res = null;
            switch (typeElement)
            {
                case ValueDesignType.BASE:
                    break;
                case ValueDesignType.FLOAT:
                    res = el.FloatVal.ToString();
                    break;
                case ValueDesignType.INT:
                    res = el.IntVal.ToString();
                    break;
                case ValueDesignType.DATE:
                    res = el.DateVal.ToString();
                    break;
                case ValueDesignType.VIEW:
                case ValueDesignType.CALCULATE:
                case ValueDesignType.CRIPTED:
                case ValueDesignType.COMMENT:
                case ValueDesignType.STRING:
                case ValueDesignType.ACTION:
                case ValueDesignType.BUTTON:
                case ValueDesignType.OPTIONSET:
                case ValueDesignType.LINK:
                case ValueDesignType.ATTACHMENT:
                    res = el.StringVal;
                    break;

                default:
                    throw new Exception("NOT DEFINED TYPE!!!");
            }
            return res;
        }
        public static LinkValueDto GetValueLink(this ElementValueA4ODto val)
        {
            if (string.IsNullOrEmpty(val.StringVal)) return null;
            try
            {
                return JsonSerializer.Deserialize<LinkValueDto>(val.StringVal);
            }
            catch
            {
                return null;
            }

        }
        public static void SetValueLink(this ElementValueA4ODto val, LinkValueDto  likVal)
        {
            
            val.StringVal= JsonSerializer.Serialize<LinkValueDto>(likVal);
        }


        public static ViewRowDto GetEmptyViewRowDto(this ElementA4ODto el)
        {
            return new ViewRowDto()
            {
                Values = new List<CellViewA4ODto>(),
                ElementId = el.Id,
                ElementName = el.ElementName

            };
        }
        public static string Menu2String(this MenuBLA4ODto menu, int i = 0)
        {
            var baseSpaces = new string('\t', i);
            if (!menu.IsVisible) return null;
            var res = baseSpaces + menu.Label;

            if (menu.Childrens?.Count() > 0)
            {
                res += Environment.NewLine;
                res += string.Join(Environment.NewLine, menu.Childrens.Select(x =>

                x.Menu2String(i + 1)).Where(x => x != null));
            }

            return res;
        }


    }
}
