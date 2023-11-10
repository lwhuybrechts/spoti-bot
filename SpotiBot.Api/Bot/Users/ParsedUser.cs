namespace SpotiBot.Api.Bot.Users
{
    public class ParsedUser
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;

        public ParsedUser(long id, string firstName, string lastName, string userName, string languageCode)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            LanguageCode = languageCode;
        }
    }
}
