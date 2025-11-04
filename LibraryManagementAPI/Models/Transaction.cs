namespace LibraryManagementAPI.Models
{

    public enum TransactionType{
        CheckOut = 0,
        Return = 1,
        Renewal = 2
    }
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public DateTime? DueDate { get; set; } // nullable for "Return" type
        public User? User { get; set; }
        public Book? Book { get; set; } 
        
    }
}
