namespace HappyHotel.Card.Setting
{
    // 卡牌设置基类，包含通用的卡牌配置信息
    public abstract class CardSettingBase : ICardSetting
    {
        public virtual void ConfigureCard(CardBase card)
        {
            // 调用子类的具体配置
            ConfigureCardInternal(card);
        }

        // 子类重写此方法来实现具体的卡牌配置
        protected abstract void ConfigureCardInternal(CardBase card);
    }
}