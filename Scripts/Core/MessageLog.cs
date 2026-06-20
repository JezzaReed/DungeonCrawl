using System.Collections.Generic;

namespace NewGameProject;

public class MessageLog
{
    public record Message(string Text, string Color = "#dddddd");

    private readonly List<Message> _messages = new();
    private readonly int _max;

    public IReadOnlyList<Message> Messages => _messages;

    public MessageLog(int max = 60) => _max = max;

    public void Add(string text, string color = "#dddddd")
    {
        _messages.Add(new Message(text, color));
        if (_messages.Count > _max)
            _messages.RemoveAt(0);
    }
}
