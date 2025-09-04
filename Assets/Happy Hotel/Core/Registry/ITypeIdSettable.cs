namespace HappyHotel.Core.Registry
{
    // 支持TypeId自动设置的接口
    public interface ITypeIdSettable<TTypeId> where TTypeId : TypeId
    {
        void SetTypeId(TTypeId typeId);
    }
}