using HappyHotel.Buff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 单个Buff图标显示器，用于显示单个Buff的图标和数值
    public class BuffIconDisplayer : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI valueText; // 显示Buff数值的文本组件

        private BuffBase currentBuff; // 当前显示的Buff
        private BuffTypeId currentTypeId; // 当前显示的Buff类型ID（在没有实例时）

        private void Awake()
        {
            // 如果没有指定Image组件，尝试获取当前GameObject上的Image组件
            if (iconImage == null) iconImage = GetComponent<Image>();

            // 如果没有指定Text组件，尝试在子对象中查找
            if (valueText == null) valueText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnDestroy()
        {
            // 清理引用
            currentBuff = null;
            currentTypeId = null;
        }

        // 设置要显示的Buff实例
        public void SetBuff(BuffBase buff)
        {
            currentBuff = buff;
            currentTypeId = buff?.TypeId;

            if (currentBuff != null)
            {
                // 设置图标
                SetBuffIcon(currentBuff.TypeId);

                // 更新数值显示
                UpdateValueDisplay();

                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // 设置要显示的Buff类型ID（保持向后兼容）
        public void SetBuffTypeId(BuffTypeId typeId)
        {
            SetBuffIcon(typeId);
            currentTypeId = typeId;

            // 如果没有具体的Buff实例，隐藏数值显示
            if (valueText != null) valueText.gameObject.SetActive(false);
        }

        // 设置Buff图标
        private void SetBuffIcon(BuffTypeId typeId)
        {
            if (iconImage == null)
            {
                Debug.LogError("BuffIconDisplayer: iconImage为空");
                return;
            }

            // 获取Buff的Template
            var template = BuffManager.Instance.GetResourceManager().GetTemplate(typeId);
            if (template != null)
            {
                // 使用Template中的icon字段
                if (template.icon != null)
                {
                    iconImage.sprite = template.icon;
                    gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"BuffIconDisplayer: TypeId为{typeId}的Buff模板没有设置图标");
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning($"BuffIconDisplayer: 无法找到TypeId为{typeId}的Buff模板");
                gameObject.SetActive(false);
            }
        }

        // 更新数值显示
        private void UpdateValueDisplay()
        {
            if (currentBuff == null)
                return;

            // 获取Buff的数值
            var buffValue = currentBuff.GetValue();

            // 更新数值显示
            if (valueText != null)
            {
                // 当数值为0时隐藏数字显示
                if (buffValue > 0)
                {
                    valueText.text = buffValue.ToString();
                    valueText.gameObject.SetActive(true);
                }
                else
                {
                    valueText.gameObject.SetActive(false);
                }
            }
        }

        // 隐藏图标
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // 显示图标
        public void Show()
        {
            gameObject.SetActive(true);
        }

        // 提供给悬停接收器的数据访问
        public BuffBase GetCurrentBuff()
        {
            return currentBuff;
        }

        public bool TryGetTypeId(out BuffTypeId typeId)
        {
            if (currentBuff != null)
            {
                typeId = currentBuff.TypeId;
                return true;
            }

            if (currentTypeId != null)
            {
                typeId = currentTypeId;
                return true;
            }

            typeId = default;
            return false;
        }
    }
}