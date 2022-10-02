using Microsoft.Extensions.Options;
using SpotiBot.Library.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;
using Microsoft.AspNetCore.Http;
using SpotiBot.Library.Extensions;

namespace SpotiBot.Bot.WebApp
{
    public class WebAppValidationService : IWebAppValidationService
    {
        private readonly TelegramOptions _telegramOptions;

        public WebAppValidationService(IOptions<TelegramOptions> telegramOptions)
        {
            _telegramOptions = telegramOptions.Value;
        }

        /// <summary>
        /// Validate if a request came from telegram by comparing the hash we received with a generated one.
        /// </summary>
        /// See <a href="https://core.telegram.org/bots/webapps#validating-data-received-via-the-web-app">telegram documentation</a>.
        /// <param name="queryCollection">The queryString from the request, containing the expected hash.</param>
        /// <returns>True if the request came from telegram and we can trust its data.</returns>
        public bool IsRequestFromTelegram(IQueryCollection queryCollection)
        {
            var hash = queryCollection.GetFirstValue("hash");
            if (string.IsNullOrEmpty(hash))
                return false;

            // Convert received hash from telegram to a byte array.
            var actualHash = Convert.FromHexString(hash);

            // Generate the hash.
            var generatedHash = HMACSHA256.HashData(
                GetSecretKey(),
                GetDataCheckString(queryCollection));

            // Compare the hash from telegram with the one we generated.
            return actualHash.SequenceEqual(generatedHash);
        }

        /// <summary>
        /// Generate the DataCheckString to generate the hash with.
        /// </summary>
        private static byte[] GetDataCheckString(IQueryCollection queryCollection)
        {
            // Order the fields by alphabet.
            var sortedFields = new SortedDictionary<string, string>(
                // In case a key exists multiple times, use only the first value.
                queryCollection.Keys.ToDictionary(x => x, x => queryCollection[x].FirstOrDefault()),
                StringComparer.Ordinal);

            var dataCheckFields = sortedFields
                // Remove the hash field.
                .Where(x => x.Key != "hash")
                // Format according to telegram documentation.
                .Select(x => $"{x.Key}={x.Value}");

            const char separator = '\n';
            var dataCheckString = string.Join(separator, dataCheckFields);

            return Encoding.UTF8.GetBytes(dataCheckString);
        }

        /// <summary>
        /// Get the secret key to generate the hash with.
        /// </summary>
        private byte[] GetSecretKey()
        {
            const string constantKey = "WebAppData";

            // The secret key consists of a constant and the bot's token.
            return HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(constantKey),
                Encoding.UTF8.GetBytes(_telegramOptions.AccessToken));
        }
    }
}
