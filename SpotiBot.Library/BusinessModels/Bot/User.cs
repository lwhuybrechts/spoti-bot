namespace SpotiBot.Library.BusinessModels.Bot
{
    public class User
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public User(long id, string firstName, string? lastName, string? userName)
        {
            Id = id;
            FirstName = firstName;
            
            if (lastName != null)
                LastName = lastName;

            if (userName != null)
                UserName = userName;
        }
    }
}
