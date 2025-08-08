using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Entidades.Entity
{
    public class AuditLog
    {
        public int AuditID { get; set; }
        public string TableName { get; set; }
        public int RecordID { get; set; }
        public string Action { get; set; }
        public int ChangedBy { get; set; }
        public DateTime ChangeDate { get; set; }
        public string PreviousData { get; set; }
        public string NewData { get; set; }
    }
}
