using Entidades.Entity;
using System.Collections.Generic;
namespace Entidades.Response
{
    public class ResBase
    {
        public bool resultado { get; set; }
        public List<Error> listaDeErrores { get; set; }
    }
}
