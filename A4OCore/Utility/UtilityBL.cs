using A4ODto;

namespace A4OCore.Utility
{
    public static class UtilityBL
    {
        public static bool OnlystoredElement(this BaseDesignDto val)
        {
#if DEBUG
            if (!VALUES_TYPE_DESIGN_TO_SAVE.Union(VALUES_TYPE_DESIGN_TO_UNSAVE).Contains(val.DesignType))
            {
                throw new Exception("NOT DEFINED TYPE!!!");
            }
#endif
            return VALUES_TYPE_DESIGN_TO_SAVE.Contains(val.DesignType);
        }
        public static readonly ValueDesignType[] VALUES_TYPE_DESIGN_TO_UNSAVE = {
            ValueDesignType.VIEW
        };

        public static readonly ValueDesignType[] VALUES_TYPE_DESIGN_TO_SAVE = {
            ValueDesignType.INT,
            ValueDesignType.CRIPTED,
            ValueDesignType.DATE  ,
            ValueDesignType.FLOAT,
            ValueDesignType.COMMENT,
            ValueDesignType.OPTIONSET,
            ValueDesignType.STRING,
            ValueDesignType.LINK,
            ValueDesignType.ATTACHMENT,
        };

        //public static bool OnlystoredElement(ValueDesignBase val)
        //{

        //    return VALUES_TYPE_DESIGN_TO_SAVE.Contains(val.DesignType);
        //}



    }
}
