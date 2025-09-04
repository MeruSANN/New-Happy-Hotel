using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 方向显示UI组件，用于显示实体的朝向
    public class DirectionDisplayer : MonoBehaviour
    {
        [Header("方向图标")] [SerializeField] private Image upDirectionImage; // 上方向图标

        [SerializeField] private Image downDirectionImage; // 下方向图标
        [SerializeField] private Image leftDirectionImage; // 左方向图标
        [SerializeField] private Image rightDirectionImage; // 右方向图标

        [Header("设置")] [SerializeField] private BehaviorComponentContainer container; // 绑定的容器

        [SerializeField] private bool autoFindContainer; // 是否自动查找容器
        [SerializeField] private string targetTag = "MainCharacter"; // 自动查找对象的tag

        private DirectionComponent directionComponent;

        private void Start()
        {
            InitializeDirectionComponent();
        }

        private void Update()
        {
            // 如果开启自动查找且container为空，则根据tag查找
            if (autoFindContainer && !container)
            {
                AutoFindContainer();
                InitializeDirectionComponent();
            }
        }

        private void OnDestroy()
        {
            // 取消事件监听
            if (directionComponent != null) directionComponent.onDirectionChanged -= OnDirectionChanged;
        }

        // 设置绑定的BehaviorComponentContainer
        public void SetRelatedContainer(BehaviorComponentContainer componentContainer)
        {
            container = componentContainer;
            InitializeDirectionComponent();
        }

        // 自动查找BehaviorComponentContainer
        private void AutoFindContainer()
        {
            var targetObject = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObject != null)
            {
                container = targetObject.GetComponent<BehaviorComponentContainer>();
                if (container == null)
                    Debug.LogError($"DirectionDisplayer: 在标签为'{targetTag}'的对象上未找到BehaviorComponentContainer组件");
            }
        }

        private void InitializeDirectionComponent()
        {
            if (!container) return;

            // 获取DirectionComponent
            directionComponent = container.GetBehaviorComponent<DirectionComponent>();
            if (directionComponent != null)
            {
                // 注册方向改变事件
                directionComponent.onDirectionChanged += OnDirectionChanged;
                // 初始化显示
                UpdateDirectionDisplay(directionComponent.GetDirection());
            }
            else
            {
                Debug.LogWarning("DirectionDisplayer: 无法获取DirectionComponent");
            }
        }

        // 方向改变事件处理
        private void OnDirectionChanged(Direction newDirection)
        {
            UpdateDirectionDisplay(newDirection);
        }

        // 更新方向显示
        private void UpdateDirectionDisplay(Direction direction)
        {
            // 隐藏所有方向图标
            SetAllDirectionImagesActive(false);

            // 根据当前方向显示对应的图标
            switch (direction)
            {
                case Direction.Up:
                    if (upDirectionImage != null)
                        upDirectionImage.gameObject.SetActive(true);
                    break;
                case Direction.Down:
                    if (downDirectionImage != null)
                        downDirectionImage.gameObject.SetActive(true);
                    break;
                case Direction.Left:
                    if (leftDirectionImage != null)
                        leftDirectionImage.gameObject.SetActive(true);
                    break;
                case Direction.Right:
                    if (rightDirectionImage != null)
                        rightDirectionImage.gameObject.SetActive(true);
                    break;
            }
        }

        // 设置所有方向图标的激活状态
        private void SetAllDirectionImagesActive(bool active)
        {
            if (upDirectionImage != null)
                upDirectionImage.gameObject.SetActive(active);
            if (downDirectionImage != null)
                downDirectionImage.gameObject.SetActive(active);
            if (leftDirectionImage != null)
                leftDirectionImage.gameObject.SetActive(active);
            if (rightDirectionImage != null)
                rightDirectionImage.gameObject.SetActive(active);
        }
    }
}