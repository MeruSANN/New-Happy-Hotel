using System.Reflection;
using HappyHotel.Character.Settings;
using HappyHotel.Character.Templates;
using HappyHotel.Core.Registry;
using HappyHotel.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.Character.Factories
{
    // Character工厂基类，提供自动TypeId设置功能
    public abstract class CharacterFactoryBase<TCharacter> : ICharacterFactory
        where TCharacter : CharacterBase
    {
        public CharacterBase Create(CharacterTemplate template, ICharacterSetting setting = null)
        {
            var characterObject = new GameObject(GetCharacterName());
            var spriteRenderer = characterObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "Hero"; // 设置角色的SortingLayer为Hero层

            var character = characterObject.AddComponent<TCharacter>();

            // 自动设置TypeId
            AutoSetTypeId(character);

            if (template) character.SetTemplate(template);

            AddCanvas(characterObject);

            // 获取方向显示预制体并创建
            var directionDisplayerPrefab = Resources.Load<GameObject>("Prefabs/DirectionDisplayer");
            if (directionDisplayerPrefab != null)
                CreateDirectionDisplayer(characterObject, directionDisplayerPrefab, character);
            else
                Debug.LogWarning("未找到DirectionDisplayer预制体，请确保Resources文件夹中存在名为DirectionDisplayer的预制体");

            // 获取Buff列表显示预制体并创建
            var buffListDisplayerPrefab = Resources.Load<GameObject>("Prefabs/BuffListDisplayer");
            if (buffListDisplayerPrefab != null)
                CreateBuffListDisplayer(characterObject, buffListDisplayerPrefab, character);
            else
                Debug.LogWarning("未找到BuffListDisplayer预制体，请确保Resources文件夹中存在名为BuffListDisplayer的预制体");

            setting?.ConfigureCharacter(character);

            return character;
        }

        private void AutoSetTypeId(CharacterBase character)
        {
            var attr = GetType().GetCustomAttribute<CharacterRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<CharacterTypeId>(attr.TypeId);
                ((ITypeIdSettable<CharacterTypeId>)character).SetTypeId(typeId);
            }
        }

        protected virtual string GetCharacterName()
        {
            return typeof(TCharacter).Name;
        }

        private void AddCanvas(GameObject characterObject)
        {
            // 在角色身上添加Canvas组件
            var canvas = characterObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 10; // 确保UI显示在角色上方

            // 添加CanvasScaler组件以适应不同分辨率
            var canvasScaler = characterObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            // 添加GraphicRaycaster组件
            characterObject.AddComponent<GraphicRaycaster>();
        }

        private void CreateDirectionDisplayer(GameObject characterObject, GameObject directionDisplayerPrefab,
            CharacterBase character)
        {
            // 实例化方向显示预制体
            var directionDisplayerInstance = Object.Instantiate(directionDisplayerPrefab, characterObject.transform);

            // 获取DirectionDisplayer组件并设置绑定的BehaviorComponentContainer
            var directionDisplayer = directionDisplayerInstance.GetComponent<DirectionDisplayer>();
            if (directionDisplayer != null)
                directionDisplayer.SetRelatedContainer(character);
            else
                Debug.LogWarning($"方向显示预制体 {directionDisplayerPrefab.name} 上没有找到DirectionDisplayer组件");
        }

        private void CreateBuffListDisplayer(GameObject characterObject, GameObject buffListDisplayerPrefab,
            CharacterBase character)
        {
            // 实例化Buff列表显示预制体
            var buffListDisplayerInstance = Object.Instantiate(buffListDisplayerPrefab, characterObject.transform);

            var rect = buffListDisplayerInstance.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 0.2f); // 位置在角色下方

            // 获取BuffListDisplayer组件并设置绑定的BehaviorComponentContainer
            var buffListDisplayer = buffListDisplayerInstance.GetComponent<BuffListDisplayer>();
            if (buffListDisplayer != null)
                buffListDisplayer.SetRelatedContainer(character);
            else
                Debug.LogWarning($"Buff列表显示预制体 {buffListDisplayerPrefab.name} 上没有找到BuffListDisplayer组件");
        }
    }
}