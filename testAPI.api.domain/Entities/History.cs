using System.ComponentModel.DataAnnotations.Schema;

namespace testAPI.api.domain.Entities
{
    public class History
    {
        public int Id { get; set; }
        public string Operation { get; set; } = string.Empty;
        public int OperationKeyId { get; set; }

        [ForeignKey("OperationKeyId")]
        public OperationKey OperationKey { get; set; } = null!;
        public DateTime ActionDate { get; set; }
        public int? UserId { get; set; }
    }
}
