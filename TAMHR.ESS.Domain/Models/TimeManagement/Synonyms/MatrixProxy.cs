using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("MATRIX_PROXY")]
    public class MatrixProxy
    {
        public string Seq_no { get; set; }
        public string Tr_Date { get; set; }
        public string Tr_Time { get; set; }
        public string Staff_Number { get; set; }
    }
}
