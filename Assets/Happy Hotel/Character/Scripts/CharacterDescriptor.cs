namespace HappyHotel.Character
{
    public class CharacterDescriptor
    {
        public CharacterDescriptor(CharacterTypeId type, string templatePath)
        {
            Type = type;
            TemplatePath = templatePath;
        }

        public CharacterTypeId Type { get; }
        public string TemplatePath { get; }
    }
}