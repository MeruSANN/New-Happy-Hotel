using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    // 卡牌模板基类，所有卡牌类型的父类
    [CreateAssetMenu(fileName = "New Card Template", menuName = "Happy Hotel/Item/Card Template")]
    public class CardTemplate : ItemTemplate
    {
        [Header("卡牌设置")] [Tooltip("如果为true，该卡牌使用后会进入消耗区，不会再被抽取")]
        public bool isConsumable;

        [Tooltip("卡牌消耗，例如法力值、能量等")] public int cardCost;

        [Tooltip("卡牌图画")] [PreviewField] public Sprite cardImage;
    }
}