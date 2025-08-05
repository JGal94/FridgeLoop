using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccesoDatos;
using Entidades.Entity;
using Entidades.Enum;
using Entidades.Response;

namespace Backend
{
    public class LogicaNotificacion
    {
        public ResObtenerNotificaciones ObtenerNotificaciones(int userId)
        {
            var res = new ResObtenerNotificaciones();
            res.notificaciones = new List<Notification>();
            res.listaDeErrores = new List<Error>();

            try
            {
                using (var linq = new linqDataContext())
                {
                    var notis = linq.GetUserNotifications(userId).ToList();

                    foreach (var n in notis)
                    {
                        res.notificaciones.Add(new Notification
                        {
                            NotificationID = n.NotificationID,
                            UserID = n.UserID ?? 0,
                            Message = n.Message,
                            Type = n.Type,
                            IsRead = n.IsRead ?? false,
                            SentAt = n.SentAt ?? DateTime.MinValue
                        });
                    }

                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
            }

            return res;
        }

        public ResBase MarcarComoLeida(int notificationID)
        {
            var res = new ResBase();
            res.listaDeErrores = new List<Error>();

            try
            {
                using (var linq = new linqDataContext())
                {
                    linq.MarkNotificationAsRead(notificationID);
                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
            }

            return res;
        }

        public ResInsertarNotificacion InsertarNotificacion(int userId, string message, string type)
        {
            var res = new ResInsertarNotificacion();
            res.listaDeErrores = new List<Error>();

            try
            {
                using (var linq = new linqDataContext())
                {
                    linq.InsertNotification(userId, message, type);
                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.listaDeErrores.Add(new Error
                {
                    ErrorCode = EnumErrores.ErrorNoControlado,
                    Message = ex.Message
                });
            }

            return res;
        }
    }

}
