namespace SpotiApiModels
{
    public class Track
    {
        public string Id { get; set; }
        public long AddedByTelegramUserId { get; set; }
        public DateTimeOffset AddedAt { get; set; }

        public Track(string id, long addedByTelegramUserId, DateTimeOffset addedAt)
        {
            Id = id;
            AddedByTelegramUserId = addedByTelegramUserId;
            AddedAt = addedAt;
        }
    }
}
