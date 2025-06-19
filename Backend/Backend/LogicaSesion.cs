using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using Entidades.Entity;
using AcccesoDatos;

namespace Backend
{
    class LogicaSesion
    {
        public bool abrir(Sesion sesion)
        {
            try
            {
                int? idBD = 0;
                int? errorId = 0;
                string errorBD = "";

                using (linqDataContext linq = new linqDataContext())
                {
                    linq.CreateUserSession(sesion.Token, sesion.Usuario.id, sesion.Origen, ref idBD, ref errorId, ref errorBD);
                }

                if (idBD == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
