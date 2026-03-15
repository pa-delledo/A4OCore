using A4OCore.Cfg;
using A4OCore.Models;
using A4OCore.Store;
using A4OCore.Store.DB.SQLLite;
using A4OCore.Utility;
using A4ODto;
using A4ODto.Action;
using A4ODto.View;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace A4OCore.BLCore
{
    public abstract class ElementBaseA4O: ElementBLA4O


    {




        private bool? _isAdmin= null;
        public bool IsAdmin
        {
            get
            {
                _isAdmin = IsInRole(_isAdmin, A4ORoles.admin, true);
                
                return _isAdmin.Value;
            }
        }
        private bool? _isReadOnly = null;
        public bool IsReadOnly
        {
            get
            {

                _isReadOnly = IsInRole(_isReadOnly, A4ORoles.readOnly,false); 
                return _isReadOnly.Value;
            }
        }

        private static bool? IsInRole(bool? a, A4ORoles role, bool defalutValue)
        {
#if DEBUG
            try
            {
#endif
                a = a.HasValue ? a.Value : UserA4O.CurrentUser.IsInRole(role);
#if DEBUG
            }
            catch
            {
                a = defalutValue;
            }
#endif
            return a;
        }

        public sealed override void CustomizeElementView(ViewValueDto designValueDto)
        {
            if(IsReadOnly)
            {
                if (designValueDto.Hidden==true)
                {
                    designValueDto.ReadOnly = true;
                }
                return;
            }
            if (IsAdmin)
            {
                
                designValueDto.ReadOnly = false;
                designValueDto.Hidden = false;
                
                return;
            }

            this.CustomizeElement(designValueDto);
        }
        public virtual void CustomizeElement(ViewValueDto designValueDto)
        {
            base.CustomizeElementView(designValueDto);
        }

        public override sealed List<MessageA4O> OnCheck()
        {
            if (IsReadOnly)
            {
                return new List<MessageA4O>() { new MessageA4O() { messageType = MessageType.error, Description = "Hai solo il diritto di Lettura" } };
            }
            var result = OnCheckElement();
            if (IsAdmin)
            {
                foreach (var item in result)
                {
                    if (item.messageType >= MessageType.error)
                    {
                        item.messageType = MessageType.warning;
                    }
                }
            }
            return result;
        }
        public virtual List<MessageA4O> OnCheckElement()
        {
            return new List<MessageA4O>();
        }
        
        

        

        public ElementBaseA4O(IStoreA4O storeManager, IA4O_CheckEnumRepository checkEnum):base(storeManager, checkEnum) {

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



    }
}
