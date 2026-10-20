using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TowerCards.Items.Tests
{
    public abstract class ItemSystemTestFixture
    {
        private readonly List<ScriptableObject> createdObjects = new();

        protected T Create<T>() where T : ScriptableObject
        {
            T value = ScriptableObject.CreateInstance<T>();
            createdObjects.Add(value);
            return value;
        }

        protected ItemDataSO Item(params ItemModuleSO[] modules)
        {
            ItemDataSO item = Create<ItemDataSO>();
            item.name = "Test Item";
            SetField(item, "itemID", "test-item");
            SetField(item, "itemModules", new List<ItemModuleSO>(modules));
            return item;
        }

        protected ItemRegistrySO Registry(params ItemDataSO[] items)
        {
            ItemRegistrySO registry = Create<ItemRegistrySO>();
            SetField(registry, "items", new List<ItemDataSO>(items));
            return registry;
        }

        protected static void SetField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing test setup field: {name}");
            field.SetValue(target, value);
        }

        [TearDown]
        public void DestroyTestObjects()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                if (createdObjects[index] != null)
                    UnityEngine.Object.DestroyImmediate(createdObjects[index]);
            }
            createdObjects.Clear();
        }
    }

    // In-memory test doubles only. These are never authored as game content.
    public class FirstTestModule : ItemModuleSO
    {
        public Func<ItemModuleState> StateFactory;
        public override ItemModuleState CreateState() => StateFactory?.Invoke();
    }

    public class SecondTestModule : ItemModuleSO
    {
        public Func<ItemModuleState> StateFactory;
        public override ItemModuleState CreateState() => StateFactory?.Invoke();
    }

    public class TestState : ItemModuleState
    {
        public string ID = "test-state";
        public Type DataType = typeof(DurabilityStateData);
        public Func<ItemModuleStateData> Capture = () => new DurabilityStateData { currentDurability = 25f };
        public override string StateID => ID;
        public override Type StateDataType => DataType;
        public override ItemModuleStateData CaptureState() => Capture();
        public override void RestoreState(ItemModuleStateData data) { }
    }

    public class OtherTestState : TestState { }
    public class OtherTestStateData : ItemModuleStateData { }
    public class GenericTestStateData<T> : ItemModuleStateData { }
}
