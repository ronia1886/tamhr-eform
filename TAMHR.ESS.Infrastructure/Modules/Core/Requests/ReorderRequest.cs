using System;

namespace TAMHR.ESS.Infrastructure.Requests
{
    public class ReorderRequest
    {
        public Guid Id { get; set; }
        public Guid? ReferenceId { get; set; }
        public int OrderIndex { get; set; }
    }
}
