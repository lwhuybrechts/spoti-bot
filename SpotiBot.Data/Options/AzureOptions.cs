using System.ComponentModel.DataAnnotations;

namespace SpotiBot.Data.Options
{
    public class AzureOptions
    {
        [Required]
        public string FunctionAppUrl { get; set; } = string.Empty;
        
        [Required]
        public string StorageAccountConnectionString { get; set; } = string.Empty;
    }
}
