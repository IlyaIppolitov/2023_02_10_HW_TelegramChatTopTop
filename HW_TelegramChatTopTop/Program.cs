using libToGetVacancies;
using HH_API;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// Инициализация подключения к боту по ключу

string? key = Environment.GetEnvironmentVariable("telegram_first");
var botClient = new TelegramBotClient(key);

// Обозначение кнопок
const string buttonEmployer = "/employer";
const string buttonVacancy = "/all_vacancies";

// Инициализация словаря с текущим статусом каждого чата
ConcurrentDictionary<long, UserState> _clientStates = new ConcurrentDictionary<long, UserState>();

// Токен отмены
using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

// Подключение к боту
var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Переключатель выбора ответа в завсимости от типа обновления
    switch (update.Type)
    {
        // Получено сообщение
        case UpdateType.Message:
            if (update.Message is not { } message)
                return;
            var text = message.Text;

            // проверка существования ключа с указанным в библиотеке ключей
            var state = _clientStates.ContainsKey(update.Message.Chat.Id) ? _clientStates[update.Message.Chat.Id] : null;
            if (state != null)
            {
                switch (state.State)
                {
                    // В данном чате был выбран запрос на вывод всех работодателей по фильтру
                    case State.SearchEmployer:
                        // получение работодателей по фильтру
                        var result = await HeadhunterClient.GetEmployersAsync(message.Text);
                        // Проверка наличия результата
                        if (result.Items.Length == 0) {
                            await SendTextMsg(message, "Нет таких работодателей!", botClient, cancellationToken);
                            // Удаление ключа
                            _clientStates.TryRemove(update.Message.Chat.Id, out _);
                            return;
                        }
                        // Преобразование имён в строку
                        string temp = $"Перечень работодателей с сайта НН по фильтру '{message.Text}':\n";
                        temp += string.Join("\n", result.Items.Select(it => it.Name.ToString()));
                        // Отправко результата
                        await SendTextMsg(message, temp, botClient, cancellationToken);
                        // Удаление ключа
                        _clientStates.TryRemove(update.Message.Chat.Id, out _);
                        break;
                    // Других вариантов пока нет
                    case State.None: return; 
                    default: return;
                }
                return;
            }
            else
            {
                // Отправка сообщения по умолчанию
                await MakeDefaultAnswerToTextMsg(message, botClient, cancellationToken);
            }
            break;
        
        // Обработка нажатия кнопки меню
        case UpdateType.CallbackQuery:
            if (update.CallbackQuery is not { } callback)
                return; 
            switch (callback.Data)
            {
                case buttonEmployer:
                    {
                        _clientStates[callback.Message.Chat.Id] = new UserState { State = State.SearchEmployer };
                        await SendTextMsg(callback.Message, "Введите фильтр для поиска ", botClient, cancellationToken);
                    }
                    break;
                case buttonVacancy:
                    {
                        ConcurrentBag<VacancyData> bag = await FuncToGetVacancies.GetAllVacancies();
                        string temp = "Перечень активных вакансий с сайта https://proglib.io:\n";
                        temp += string.Join("\n", bag.Select(it => it.Vacancy.ToString()));
                        await SendTextMsg(callback.Message, temp, botClient, cancellationToken);
                    }
                    break;
                default:
                    {
                        await MakeDefaultAnswerToTextMsg(callback.Message, botClient, cancellationToken);
                    }
                    break;
            }
            break;

        default: return;
    }
}

// Формирование ответа на текстовое сообщение
async Task MakeDefaultAnswerToTextMsg(Message message, ITelegramBotClient botClient , CancellationToken cancellationToken)
{
    if (message.Text is not { } messageText)
        return;

    if (message.Text.ToLower() == "/start")
    {
        await SendAnswerToStartMsg(message, botClient, cancellationToken);
    }

    await SendStandardMsg(message, botClient, cancellationToken);

    return;
}




// Отправка приветствия в ответ на сообщение начальное сообщение /start
async Task SendAnswerToStartMsg(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
{
    if (message.From is not { } sender)
        return;

    string textAnswer;

    if (sender.IsBot)
    {
        textAnswer = "Никаких имён для ботов!";
    }
    else
    {
        textAnswer = $"Привет, рад тебя видеть {message.From.FirstName}!";
    }

    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: message.Chat.Id,
        text: textAnswer,
        cancellationToken: cancellationToken);

    Console.WriteLine($"Received a /start message in chat {message.Chat.Id}.");
    return;
}

// Отправка сообщения по умолчанию
async Task SendStandardMsg(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
{


    await botClient.SendTextMessageAsync(
        message.Chat.Id,
        "Бот пока умеет принимать только эти команды выбери одну их них:",
        replyMarkup: GetButtons()
        ); ;
    return;
}

// Формирование кнопок
IReplyMarkup? GetButtons()
{
    return new InlineKeyboardMarkup(new[]
    {
        new [] {InlineKeyboardButton.WithCallbackData(buttonEmployer) },
        new [] {InlineKeyboardButton.WithCallbackData(buttonVacancy) }
    });
}

// Отправка сообщения по умолчанию
async Task SendTextMsg(Message message, string text, ITelegramBotClient botClient, CancellationToken cancellationToken)
{

    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: message.Chat.Id,
        text: text,
        cancellationToken: cancellationToken);
    return;
}



Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

