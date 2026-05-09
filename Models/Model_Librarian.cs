using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Models
{
    [Table("Librarians")] //Observatia 3 din pdf: Am sters NotMapped ala
    public class Model_Librarian : Model_User
    {
      
    }
}
