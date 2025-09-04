namespace HappyHotel.Card
{
    public class CardDescriptor
    {
        public CardDescriptor(CardTypeId type, string templatePath)
        {
            Type = type;
            TemplatePath = templatePath;
        }

        public CardTypeId Type { get; }
        public string TemplatePath { get; }
    }
}