# 添加新Registry类型指南

本文档说明如何在Happy Hotel项目中添加新的被Registry系统托管的类，如Enemy、Action等。

## 概述

Registry系统是项目中用于管理各种类型实例的核心系统。它通过工厂模式和注册属性来实现类型的自动注册和创建。主要包含以下组件：

- **基类**：定义核心功能和接口
- **具体实现类**：继承基类的具体类型
- **工厂类**：负责创建实例的工厂
- **注册属性**：用于标记和注册类型
- **Manager**：管理类型实例的单例管理器

## 添加新Enemy类型

### 1. 创建Enemy实现类

在 `Assets/Happy Hotel/Enemy/Scripts/Enemies/` 目录下创建新的敌人类：

```csharp
using UnityEngine;
using HappyHotel.Core.BehaviorComponent.Components;
using HappyHotel.Action;
using System.Collections.Generic;
using HappyHotel.Core.BehaviorComponent;

namespace HappyHotel.Enemy
{
    // 新敌人实现，描述其特性
    [AutoInitComponent(typeof(需要的组件类型))]
    public class 新敌人名称 : EnemyBase
    {
        // 组件引用
        private 组件类型 组件引用;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 获取组件
            组件引用 = GetBehaviorComponent<组件类型>();
            
            // 初始化逻辑
            初始化方法();
        }
        
        // 具体的初始化逻辑
        private void 初始化方法()
        {
            // 实现具体功能
        }
    }
}
```

**要点：**
- 继承自 `EnemyBase`
- 使用 `[AutoInitComponent(typeof(组件类型))]` 自动初始化需要的组件
- 在 `Awake()` 中调用 `base.Awake()` 并进行初始化
- 不需要在类上添加注册属性

### 2. 创建Enemy工厂类

在 `Assets/Happy Hotel/Enemy/Scripts/Factories/` 目录下创建工厂类：

```csharp
using HappyHotel.Enemy.Setting;
using UnityEngine;
using HappyHotel.Enemy.Templates;

namespace HappyHotel.Enemy.Factory
{
    [EnemyRegistration(
        "类型ID",
        "Templates/模板文件名")]
    public class 新敌人名称Factory : EnemyFactoryBase<新敌人名称>
    {
        protected override 新敌人名称 CreateEnemyComponent(GameObject gameObject)
        {
            return gameObject.AddComponent<新敌人名称>();
        }
    }
}
```

**要点：**
- 继承自 `EnemyFactoryBase<T>`，其中T是具体的敌人类型
- 使用 `[EnemyRegistration("类型ID", "模板路径")]` 注册
- 实现 `CreateEnemyComponent` 方法，返回添加到GameObject上的组件

## 添加新Action类型

### 1. 创建Action实现类

在 `Assets/Happy Hotel/Action/Scripts/Actions/` 目录下创建新的行动类：

```csharp
using UnityEngine;

namespace HappyHotel.Action
{
    // 新行动实现，描述其功能
    public class 新行动名称 : ActionBase
    {
        // 行动参数
        private int 参数名称;
        
        public 新行动名称()
        {
            // 初始化默认值
            参数名称 = 默认值;
        }
        
        // 参数设置方法
        public void Set参数名称(int value)
        {
            参数名称 = value;
        }
        
        public override void Execute()
        {
            // 实现具体的行动逻辑
            Debug.Log($"执行{新行动名称}");
            
            // 获取行动发起者
            var actionQueue = GetActionQueue();
            if (actionQueue == null || actionQueue.GetHost() == null)
            {
                Debug.LogError("无法获取行动发起者");
                return;
            }
            
            // 具体的执行逻辑
        }
    }
}
```

**要点：**
- 继承自 `ActionBase`
- 实现 `Execute()` 方法定义行动逻辑
- 可以添加参数设置方法
- 不需要在类上添加注册属性

### 2. 创建Action工厂类

在 `Assets/Happy Hotel/Action/Scripts/Factories/` 目录下创建工厂类：

```csharp
using HappyHotel.Action.Setting;
using HappyHotel.Core.Registry;

namespace HappyHotel.Action.Factories
{
    [ActionRegistration("类型ID")]
    public class 新行动名称Factory : ActionFactoryBase<新行动名称>
    {
        protected override 新行动名称 CreateActionInstance()
        {
            return new 新行动名称();
        }
    }
}
```

**要点：**
- 继承自 `ActionFactoryBase<T>`，其中T是具体的行动类型
- 使用 `[ActionRegistration("类型ID")]` 注册
- 实现 `CreateActionInstance` 方法，返回新的实例

## 在代码中使用Registry创建实例

### 创建Action实例

```csharp
// 方法1：通过TypeId创建
var typeId = Core.Registry.TypeId.Create<ActionTypeId>("类型ID");
var action = ActionManager.Instance.Create(typeId);

// 方法2：强制转换为具体类型
var typeId = Core.Registry.TypeId.Create<ActionTypeId>("Attack");
var attackAction = (AttackAction)ActionManager.Instance.Create(typeId);
attackAction.SetDamage(1);
```

### 创建Enemy实例

```csharp
// 通过EnemyManager创建
var typeId = Core.Registry.TypeId.Create<EnemyTypeId>("类型ID");
var enemy = EnemyManager.Instance.Create(typeId, template, setting);
```

## 重要注意事项

### 1. 文件命名规范
- Enemy类：`新敌人名称.cs`
- Enemy工厂：`新敌人名称Factory.cs`
- Action类：`新行动名称Action.cs`
- Action工厂：`新行动名称ActionFactory.cs`

### 2. 注册属性位置
- **Enemy类本身**：不添加注册属性
- **Enemy工厂**：添加 `[EnemyRegistration]`
- **Action类本身**：不添加注册属性
- **Action工厂**：添加 `[ActionRegistration]`

### 3. 继承关系
- Enemy类继承 `EnemyBase`
- Enemy工厂继承 `EnemyFactoryBase<T>`
- Action类继承 `ActionBase`
- Action工厂继承 `ActionFactoryBase<T>`

### 4. 组件系统集成
- 使用 `[AutoInitComponent(typeof(组件类型))]` 自动初始化组件
- 在 `Awake()` 中获取组件引用
- 使用 `GetBehaviorComponent<T>()` 获取组件

### 5. 模板文件
- Enemy需要对应的Template文件（.asset）
- 在工厂注册时指定模板路径
- 模板文件通常放在 `Resources/Templates/` 目录下

## 示例：完整的Slime敌人实现

参考项目中的Slime敌人实现，它展示了：
- 如何使用ActionCycleComponent
- 如何创建和配置行动循环
- 如何设置攻击目标
- 如何启动自动循环

这个实现可以作为创建新敌人类型的参考模板。

## 总结

添加新的Registry类型需要：
1. 创建具体实现类（继承对应基类）
2. 创建对应的工厂类（继承对应工厂基类）
3. 在工厂类上添加注册属性
4. 通过Manager的Create方法创建实例

遵循这个流程可以确保新类型正确集成到Registry系统中。 