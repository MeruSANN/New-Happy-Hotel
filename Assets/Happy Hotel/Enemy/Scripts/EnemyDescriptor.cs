namespace HappyHotel.Enemy
{
    public class EnemyDescriptor
    {
        public EnemyDescriptor(EnemyTypeId type, string templatePath)
        {
            Type = type;
            TemplatePath = templatePath;
        }

        public EnemyTypeId Type { get; }
        public string TemplatePath { get; }
    }
}