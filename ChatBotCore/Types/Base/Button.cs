namespace BotCore
{
    public class Button : IButton
    {
        public Button()
        {

        }

        public Button(string text)
        {
            Text = text;
        }

        public Button(string text, string key) : this(text)
        {
            Key = key;
        }

        public string Text { get; set; }

        public string Key { get; set; }

        public override string ToString()
        {
            return $"Button :{{Text :\"{Text}\", Key :\"{Key}\"}}";
        }
    }
}
