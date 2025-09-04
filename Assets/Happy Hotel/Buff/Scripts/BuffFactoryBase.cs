using System.Reflection;
using HappyHotel.Buff.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Buff.Factories
{
    // Buff工厂基类，提供自动TypeId设置功能
    public abstract class BuffFactoryBase<TBuff> : IBuffFactory
        where TBuff : BuffBase, new()
    {
        public BuffBase Create(BuffTemplate template, IBuffSetting setting = null)
        {
            var buff = new TBuff();

            // 自动设置TypeId
            AutoSetTypeId(buff);

            setting?.ConfigureBuff(buff);

            return buff;
        }

        private void AutoSetTypeId(BuffBase buff)
        {
            var attr = GetType().GetCustomAttribute<BuffRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<BuffTypeId>(attr.TypeId);
                ((ITypeIdSettable<BuffTypeId>)buff).SetTypeId(typeId);
            }
        }
    }
}