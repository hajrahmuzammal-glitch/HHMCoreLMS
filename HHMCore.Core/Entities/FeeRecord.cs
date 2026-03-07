using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class FeeRecord : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string FeeType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal LateFine { get; set; } = 0;
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; } = false;
        public DateTime? PaidAt { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Remarks { get; set; }

        public Guid SemesterId { get; set; }
        public Semester Semester { get; set; } = null!;
    }
}
