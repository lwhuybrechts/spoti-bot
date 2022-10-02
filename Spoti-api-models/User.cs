namespace SpotiApiModels
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }

        public User(string id, string firstName)
        {
            Id = id;
            FirstName = firstName;
        }
    }
}
