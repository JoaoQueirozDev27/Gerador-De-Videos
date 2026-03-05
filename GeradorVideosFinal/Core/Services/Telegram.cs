using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Services
{
    public class TelegramBot
    {
        private readonly TelegramBotClient _botClient;
        private readonly CancellationTokenSource _CancellationTokenSource;
        private readonly int _ImageLimit;
        private readonly long chatId = 7210957104;

        public List<int> Images = new List<int>();
        int CountImages = 0;
        public bool IsConfirmed = false;

        public TelegramBot(int limit)
        {
            string botToken = "8353488012:AAEnLA3LhmTCVwWtXM5oT08TuZQhj1Q1MzE";

            _ImageLimit = limit;

            _botClient = new TelegramBotClient(botToken);

            _CancellationTokenSource = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: _CancellationTokenSource.Token
            );

        }

        async Task HandleUpdateAsync(ITelegramBotClient bot, Telegram.Bot.Types.Update update, CancellationToken token)
        {

            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                if (update.Message.Text.ToLower() == "sim")
                    IsConfirmed = true;
            }

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;

                if (CountImages < _ImageLimit)
                    Images.Add(int.Parse(callbackQuery.Data));

                if (CountImages >= _ImageLimit)
                    await bot.SendMessage(chatId, "Limite de imagens atingido, você confirma?");

                CountImages++;
            }

        }

        public async Task SendImage(string imageUrl, int count)
        {
            try
            {

                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                    {
                    new[]
                    {
                        Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(
                            "Escolher",
                            count.ToString()
                        )
                    }
                }
                );

                await _botClient.SendDocument(
                    chatId: 7210957104,
                    document: InputFile.FromUri(imageUrl),
                    replyMarkup: keyboard
                );
            }
            catch { }
        }

        public async Task SendMessage(string message)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: message
            );
        }

        public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Erro: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
