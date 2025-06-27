using Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqInsertarUsuario
    {
        public Usuario usuario {  get; set; }
        public string numeroVerificacion {  get; set; }
    }
}
