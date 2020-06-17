using System;
namespace ExchangeRatesWebApp.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public long ChatId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public int TelegramId { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastForumMessageDate { get; set; }
    }
}
