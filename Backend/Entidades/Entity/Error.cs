using System;
using Entidades.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Entity
{
    public class Error
    {

        public EnumErrores ErrorCode { get; set; }
        public string Message { get; set; }

    }
}
