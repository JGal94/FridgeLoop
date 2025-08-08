using Frontend_Proyecto_Fridgeloop.Entidades.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Entidades.Entity
{
    public class Error
    {
        public EnumErrores ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
