using System.Buffers.Text;
using System.Security.Cryptography;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RandomizerTelegramBot;

public class TelegramBot
{
    private readonly Random _random = new Random();
    public async Task RunAsync(string apiKey, TimeSpan pollPeriod, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
            return;

        var offset = 0;
        var telegramBot = new TelegramBotClient(apiKey);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(pollPeriod, cancellationToken);

            Update[] updates = Array.Empty<Update>();
            try
            {
                updates = await telegramBot.GetUpdates(offset, timeout: (int)pollPeriod.TotalSeconds, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                foreach (var update in updates)
                {
                    offset = update.Id + 1;

                    if (update is null || update.Message is null || string.IsNullOrEmpty(update.Message.Text))
                        continue;

                    if(update.Message.Text.Contains(nameof(Guid), StringComparison.InvariantCultureIgnoreCase))
                        await telegramBot.SendMessage(update.Message.Chat, GetGuid());
                    else if (update.Message.Text.Contains(nameof(Dice), StringComparison.InvariantCultureIgnoreCase))
                        await telegramBot.SendMessage(update.Message.Chat, Dice());
                    else if (update.Message.Text.Contains(nameof(RandomString), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var args = int.TryParse(update.Message.Text.Replace(nameof(RandomString), string.Empty, StringComparison.InvariantCultureIgnoreCase).Trim(), out var result) ? result : 32;
                        await telegramBot.SendMessage(update.Message.Chat, RandomString(args));
                    }
                    else if (update.Message.Text.Contains(nameof(Base64), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var args = update.Message.Text.Replace(nameof(Base64), string.Empty, StringComparison.InvariantCultureIgnoreCase).Trim();
                        args = string.IsNullOrEmpty(args) ? RandomString() : args;
                        await telegramBot.SendMessage(update.Message.Chat, GetBase64(args));
                    }
                    else if (update.Message.Text.Contains(nameof(SHA256), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var args = update.Message.Text.Replace(nameof(SHA256), string.Empty, StringComparison.InvariantCultureIgnoreCase).Trim();
                        args = string.IsNullOrEmpty(args) ? RandomString() : args;
                        await telegramBot.SendMessage(update.Message.Chat, GetSha256(args));
                    }
                    else if (update.Message.Text.Contains(nameof(SHA512), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var args = update.Message.Text.Replace(nameof(SHA512), string.Empty, StringComparison.InvariantCultureIgnoreCase).Trim();
                        args = string.IsNullOrEmpty(args) ? RandomString() : args;
                        await telegramBot.SendMessage(update.Message.Chat, GetSha512(args));
                    }
                    else if (update.Message.Text.Contains("[") && update.Message.Text.Contains("-") && update.Message.Text.Contains("]"))
                    {
                        var args = update.Message.Text.Replace("[", string.Empty).Replace("]",string.Empty).Split("-");
                        
                        if(int.TryParse(args.FirstOrDefault(), out var _) && int.TryParse(args.LastOrDefault(), out var _))
                            await telegramBot.SendMessage(update.Message.Chat, GetRandomEnum(int.Parse(args.First()), int.Parse(args.Last())));
                        else
                            await telegramBot.SendMessage(update.Message.Chat, GetRandomEnum(args.First().First(), args.Last().Last()));
                    }
                    else if (update.Message.Text.Contains("(") && update.Message.Text.Contains("-") && update.Message.Text.Contains(")"))
                    {
                        var args = update.Message.Text.Replace("(", string.Empty).Replace(")", string.Empty).Split("-");

                        if (int.TryParse(args.FirstOrDefault(), out var _) && int.TryParse(args.LastOrDefault(), out var _))
                            await telegramBot.SendMessage(update.Message.Chat, GetItem(int.Parse(args.First()), int.Parse(args.Last())));
                        else
                            await telegramBot.SendMessage(update.Message.Chat, GetItem(args.First().First(), args.Last().Last()));
                    }
                    else if (update.Message.Text.Contains("{") && update.Message.Text.Contains(",") && update.Message.Text.Contains("}"))
                    {
                        var args = update.Message.Text.Replace("{", string.Empty).Replace("}", string.Empty).Split(",");

                        await telegramBot.SendMessage(update.Message.Chat, Shuffle(args));
                    }
                }
            }
        }
    }
    private string Dice() => _random.Next(1, 7).ToString();
    private string GetGuid() => Guid.NewGuid().ToString();
    private string GetBase64(string text) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
    private string GetSha256(string text) => System.Text.Encoding.UTF8.GetString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)));
    private string GetSha512(string text) => System.Text.Encoding.UTF8.GetString(SHA512.HashData(System.Text.Encoding.UTF8.GetBytes(text)));
    private string RandomString(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
    private string GetRandomEnum(char startChar, char endChar)
    {
        if (endChar <= startChar) 
            return "error";

        var charRange = Enumerable.Range(startChar, endChar - startChar + 1).Select(c => (char)c).ToArray();
        _random.Shuffle(charRange);

        return $"[{string.Join(", ", charRange)}]";
    }
    private string GetRandomEnum(int startNum, int endNum)
    {
        if (endNum <= startNum)
            return "error";

        var range = Enumerable.Range(startNum, endNum).ToArray();
        _random.Shuffle(range);

        return $"[{string.Join(", ", range)}]";
    }
    private string GetItem(char startChar, char endChar)
    {
        if (endChar <= startChar)
            return "error";

        var range = Enumerable.Range(startChar, endChar - startChar + 1).Select(c => (char)c).ToArray();
        _random.Shuffle(range);

        return $"{range.First()}";
    }
    private string GetItem(int startNum, int endNum)
    {
        if (endNum <= startNum)
            return "error";

        var range = Enumerable.Range(startNum, endNum).ToArray();
        _random.Shuffle(range);

        return $"[{range.First()}]";
    }
    private string Shuffle(string[] range)
    {
        _random.Shuffle(range);
        return $"{{{string.Join(",", range)}}}";
    }
}
