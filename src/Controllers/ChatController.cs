using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ZavaStorefront.Services;

namespace ZavaStorefront.Controllers
{
    public class ChatController : Controller
    {
        private const string ChatSessionKey = "ChatHistory";
        private readonly ILogger<ChatController> _logger;
        private readonly ChatService _chatService;

        public ChatController(ILogger<ChatController> logger, ChatService chatService)
        {
            _logger = logger;
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Loading chat page");
            var history = GetHistoryFromSession();
            ViewBag.ConversationDisplay = FormatHistoryForDisplay(history);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return RedirectToAction("Index");
            }

            _logger.LogInformation("Processing chat message");

            var history = GetHistoryFromSession();
            history.Add(new ChatMessage { Role = "user", Content = userMessage });

            var (reply, error) = await _chatService.SendMessageAsync(history);

            if (reply != null)
            {
                history.Add(new ChatMessage { Role = "assistant", Content = reply });
                SaveHistoryToSession(history);
            }
            else
            {
                // Remove the user message we just added — the turn failed
                history.RemoveAt(history.Count - 1);
                SaveHistoryToSession(history);
                ViewBag.ErrorMessage = error;
            }

            ViewBag.ConversationDisplay = FormatHistoryForDisplay(history);
            return View("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _logger.LogInformation("Clearing chat history");
            HttpContext.Session.Remove(ChatSessionKey);
            return RedirectToAction("Index");
        }

        private List<ChatMessage> GetHistoryFromSession()
        {
            var json = HttpContext.Session.GetString(ChatSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<ChatMessage>();
            }

            return JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new List<ChatMessage>();
        }

        private void SaveHistoryToSession(List<ChatMessage> history)
        {
            HttpContext.Session.SetString(ChatSessionKey, JsonSerializer.Serialize(history));
        }

        private static string FormatHistoryForDisplay(List<ChatMessage> history)
        {
            if (history.Count == 0)
            {
                return string.Empty;
            }

            var lines = history.Select(m =>
                m.Role == "user" ? $"You: {m.Content}" : $"Phi-4: {m.Content}");
            return string.Join("\n\n", lines);
        }
    }
}
