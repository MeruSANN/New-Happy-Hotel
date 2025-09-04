using System.Collections.Generic;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Character
{
    // 角色控制器，负责角色的创建和位置管理业务逻辑
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu")]
    [SingletonInitializationDependency(typeof(GridObjectManager))]
    public class CharacterController : SingletonBase<CharacterController>
    {
        protected override void OnSingletonAwake()
        {
            // 确保GridObjectManager已初始化
            if (GridObjectManager.Instance == null) Debug.LogError("GridObjectManager未初始化，角色控制可能无法正常工作");
        }

        // 在指定位置创建角色
        public CharacterBase CreateCharacter(CharacterTypeId typeId, Vector2Int position)
        {
            var character = CharacterManager.Instance.Create(typeId);
            if (character)
            {
                // 设置角色位置
                character.transform.parent = transform;
                character.GetBehaviorComponent<GridObjectComponent>().MoveTo(position);
            }

            return character;
        }

        // 便捷方法：使用字符串创建角色
        public CharacterBase CreateCharacter(string typeId, Vector2Int position)
        {
            var characterTypeId = TypeId.Create<CharacterTypeId>(typeId);
            return CreateCharacter(characterTypeId, position);
        }

        // 获取指定位置的所有角色
        public List<CharacterBase> GetAllCharactersAt(Vector2Int position)
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取角色");
                return new List<CharacterBase>();
            }

            // 查找所有角色
            var characters = new List<CharacterBase>();
            var containers = GridObjectManager.Instance.GetObjectsOfTypeAt<CharacterBase>(position);
            foreach (var character in containers) characters.Add(character);

            return characters;
        }

        // 移除指定位置的角色
        public void RemoveCharacter(Vector2Int position)
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogError("GridObjectManager未初始化，无法移除角色");
                return;
            }

            var characters = GridObjectManager.Instance.GetObjectsOfTypeAt<CharacterBase>(position);
            foreach (var character in characters)
            {
                CharacterManager.Instance.Remove(character);
                Destroy(character.gameObject);
                Debug.Log($"已移除位置 {position} 的角色");
            }
        }

        // 获取所有角色
        public List<CharacterBase> GetAllCharacters()
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取角色");
                return new List<CharacterBase>();
            }

            return GridObjectManager.Instance.GetObjectsOfType<CharacterBase>();
        }

        // 清除所有角色
        public void ClearAllCharacters()
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogError("GridObjectManager未初始化，无法清除角色");
                return;
            }

            var characters = GridObjectManager.Instance.GetObjectsOfType<CharacterBase>();
            foreach (var character in characters)
            {
                CharacterManager.Instance.Remove(character);
                Destroy(character.gameObject);
            }

            Debug.Log("已清除所有角色");
        }
    }
}