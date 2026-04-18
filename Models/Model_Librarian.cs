using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Models
{
    public class Model_Librarian : Model_User
    {
        //adaug o proprietate specifica sa se stie cand ii la munca
        private string _shift;
        public string Shift
        {
            get { return _shift; }
            set { _shift = value; }
        }
        public Model_Librarian()
        {
            Role = "Librarian";
            IsActive = true;
        }
        //corpul pt metode de printf
        public void issueBook(int bookID, int memberID)
        {
            Console.WriteLine($"Librarian {Username} (ID: {Id}) issued book {bookId} to member {memberId}");
        }
        public void returnBook(int bookID, int memberID)
        {
            Console.WriteLine($"Librarian {Username} (ID: {Id}) processed return of book {bookId} from member {memberId}");
        }
        //daca face verificari in timpul zilei-poate ca nu e necesara metoda
        public void manageInventory()
        {
            Console.WriteLine($"Librarian {Username} (ID: {Id}) is managing inventory");
        }
    }
}
