using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioILS.Model
{
    public class DeleteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DeleteStatus Status { get; set; }

        public static DeleteResult Ok(string msg = null) => new() { Success = true, Status = DeleteStatus.Deleted, Message = msg };
        public static DeleteResult Locked(string msg) => new() { Success = false, Status = DeleteStatus.HasDependencies, Message = msg };
    }

    public enum DeleteStatus { Deleted, HasDependencies, Error }
}
