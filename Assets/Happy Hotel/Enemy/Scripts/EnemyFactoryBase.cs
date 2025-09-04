using System.Reflection;
using HappyHotel.Action.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Enemy.Settings;
using HappyHotel.Enemy.Templates;
using HappyHotel.UI;
using HappyHotel.UI.HoverDisplay.EnemyHover;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.Enemy.Factories
{
    // Enemy工厂基类，提供自动TypeId设置功能
    public abstract class EnemyFactoryBase<TEnemy> : IEnemyFactory
        where TEnemy : EnemyBase
    {
        public EnemyBase Create(EnemyTemplate template, IEnemySetting setting = null)
        {
            var enemyObject = new GameObject(GetEnemyName());
            enemyObject.AddComponent<SpriteRenderer>();

            // 添加Image组件用于UI事件检测
            var enemyImage = enemyObject.AddComponent<Image>();
            enemyImage.raycastTarget = true; // 确保可以接收UI事件
            enemyImage.color = new Color(1f, 1f, 1f, 0f); // 透明，不显示

            var enemy = enemyObject.AddComponent<TEnemy>();

            // 自动设置TypeId
            AutoSetTypeId(enemy);

            if (template) enemy.SetTemplate(template);

            AddCanvas(enemyObject);

            // 获取敌人血量显示预制体
            var healthBarPrefab = Resources.Load<GameObject>("Prefabs/Enemy Health");

            // 如果有血量显示预制体，则创建血量UI
            if (healthBarPrefab != null)
                CreateHealthBar(enemyObject, healthBarPrefab, enemy);
            else
                Debug.LogWarning("未找到敌人血量显示预制体，请确保Resources文件夹中存在名为Enemy Health的预制体");

            // 创建意图显示器
            CreateIntentDisplayer(enemyObject, enemy);

            setting?.ConfigureEnemy(enemy);

            // 为敌人添加悬停接收器
            AddHoverReceiverToEnemy(enemyObject, enemy);

            return enemy;
        }

        private void AddCanvas(GameObject enemyObject)
        {
            // 在敌人身上添加Canvas组件
            var canvas = enemyObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 10; // 确保血条显示在敌人上方

            // 添加CanvasScaler组件以适应不同分辨率
            var canvasScaler = enemyObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            // 添加GraphicRaycaster组件
            enemyObject.AddComponent<GraphicRaycaster>();
        }

        private void CreateHealthBar(GameObject enemyObject, GameObject healthBarPrefab, EnemyBase enemy)
        {
            // 实例化敌人血量显示预制体
            var healthBarInstance = Object.Instantiate(healthBarPrefab, enemyObject.transform);

            // 设置位置为(0, 0.3)
            var rect = healthBarInstance.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 0.3f);

            // 获取EnemyHealthDisplayUI组件并设置绑定的BehaviorComponentContainer
            var healthDisplayUI = healthBarInstance.GetComponent<EnemyHealthDisplayUI>();
            if (healthDisplayUI != null)
                healthDisplayUI.SetRelatedContainer(enemy);
            else
                Debug.LogWarning($"敌人血量显示预制体 {healthBarPrefab.name} 上没有找到EnemyHealthDisplayUI组件");
        }

        // 创建意图显示器
        private void CreateIntentDisplayer(GameObject enemyObject, EnemyBase enemy)
        {
            // 获取意图显示器预制体
            var intentDisplayerPrefab = Resources.Load<GameObject>("Prefabs/Intent Displayer");

            if (intentDisplayerPrefab != null)
            {
                // 实例化意图显示器
                var intentDisplayerInstance = Object.Instantiate(intentDisplayerPrefab, enemyObject.transform);

                // 设置位置为(0, -0.3)，在敌人下方显示
                var rect = intentDisplayerInstance.GetComponent<RectTransform>();

                // 获取EnemyIntentDisplay组件并设置目标敌人
                var intentDisplay = intentDisplayerInstance.GetComponent<EnemyIntentDisplay>();
                if (intentDisplay != null)
                {
                    intentDisplay.SetTargetEnemy(enemy);
                    Debug.Log($"为敌人 {enemyObject.name} 创建了意图显示器");
                }
                else
                {
                    Debug.LogWarning($"意图显示器预制体 {intentDisplayerPrefab.name} 上没有找到EnemyIntentDisplay组件");
                }
            }
            else
            {
                Debug.LogWarning("未找到意图显示器预制体，请确保Resources/Prefabs文件夹中存在名为Intent Displayer的预制体");
            }
        }

        private void AutoSetTypeId(EnemyBase enemy)
        {
            var attr = GetType().GetCustomAttribute<EnemyRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<EnemyTypeId>(attr.TypeId);
                ((ITypeIdSettable<EnemyTypeId>)enemy).SetTypeId(typeId);
            }
        }

        protected virtual string GetEnemyName()
        {
            return typeof(TEnemy).Name;
        }

        // 为敌人添加悬停接收器
        private void AddHoverReceiverToEnemy(GameObject enemyObject, EnemyBase enemy)
        {
            // 检查是否已经有悬停接收器
            var existingReceiver = enemyObject.GetComponent<EnemyHoverReceiver>();
            if (existingReceiver != null) return; // 如果已经有接收器，直接返回

            // 添加悬停接收器组件
            var receiver = enemyObject.AddComponent<EnemyHoverReceiver>();

            Debug.Log($"为敌人 {enemyObject.name} 添加了悬停接收器");
        }
    }
}