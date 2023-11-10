namespace SpotiBot.Library.BusinessModels.Bot
{
    public class User
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? LanguageCode { get; set; }

        public User(long id, string firstName, string? lastName, string? userName, string? languageCode)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            LanguageCode = languageCode;
        }
    }
}
