using System;
using System.Text.Json.Serialization;

namespace Todo.Core
{
    public class TodoItem
    {
        [JsonInclude]
        public Guid Id { get; private set; } = Guid.NewGuid();

        [JsonInclude]
        public string Title { get; private set; }

        [JsonInclude]
        public bool IsDone { get; private set; }

        public TodoItem(string title)
        {
            Title = title?.Trim() ?? throw new ArgumentNullException(nameof(title));
        }

        [JsonConstructor]
        public TodoItem(Guid id, string title, bool isDone)
        {
            Id = id;
            Title = title?.Trim() ?? throw new ArgumentNullException(nameof(title));
            IsDone = isDone;
        }

        public void MarkDone() => IsDone = true;
        public void MarkUndone() => IsDone = false;
        public void Rename(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle)) throw new ArgumentException("Title required", nameof(newTitle));
            Title = newTitle.Trim();
        }
    }
}
