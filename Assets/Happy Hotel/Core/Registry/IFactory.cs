namespace HappyHotel.Core.Registry
{
    // 通用工厂接口
    public interface IFactory<TObject, TTemplate, TSettings>
        where TTemplate : class
        where TSettings : class
    {
        public TObject Create(TTemplate template, TSettings setting = null);
    }
}