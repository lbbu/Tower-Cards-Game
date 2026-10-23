using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;

namespace TowerCards.Items.Tests
{
    public class ItemRegistryTests : ItemSystemTestFixture
    {
        [Test]
        public void Lookup_IsOrdinalAndRejectsUnknownOrBlankIDs()
        {
            ItemDataSO definition = Item();
            ItemRegistrySO registry = Registry(definition);
            Assert.That(registry.TryGetItem("test-item", out ItemDataSO found), Is.True);
            Assert.That(found, Is.SameAs(definition));
            foreach (string id in new[] { null, "", " ", "unknown", "TEST-ITEM" })
            {
                Assert.That(registry.TryGetItem(id, out ItemDataSO missing), Is.False);
                Assert.That(missing, Is.Null);
            }
        }

        [Test]
        public void Lookup_RejectsNullListAndNullItem()
        {
            ItemRegistrySO registry = Registry((ItemDataSO)null);
            Assert.Throws<InvalidOperationException>(() => registry.TryGetItem("test-item", out _));
            SetField(registry, "items", null);
            Assert.Throws<InvalidOperationException>(() => registry.TryGetItem("test-item", out _));
        }

        [Test]
        public void Lookup_RejectsDuplicateAndInvalidDefinitions()
        {
            ItemDataSO first = Item();
            ItemDataSO second = Item();
            ItemRegistrySO registry = Registry(first, second);
            Assert.Throws<InvalidOperationException>(() => registry.TryGetItem("test-item", out _));
            SetField(second, "itemID", " ");
            Assert.Throws<InvalidOperationException>(() => registry.TryGetItem("test-item", out _));
            SetField(second, "itemID", "second");
            SetField(second, "maxStackSize", 0);
            Assert.Throws<InvalidOperationException>(() => registry.TryGetItem("test-item", out _));
        }

        [Test]
        public void FailedBuild_PublishesNoPartialCache_AndCanRetryAfterCorrection()
        {
            ItemDataSO first = Item();
            ItemDataSO second = Item();
            ItemRegistrySO registry = Registry(first, second);
            Assert.Throws<InvalidOperationException>(() => registry.TryGetItem("test-item", out _));
            FieldInfo lookup = typeof(ItemRegistrySO).GetField("itemLookup", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(lookup.GetValue(registry), Is.Null);
            SetField(second, "itemID", "second");
            Assert.That(registry.TryGetItem("second", out ItemDataSO found), Is.True);
            Assert.That(found, Is.SameAs(second));
        }

        [Test]
        public void InvalidateLookup_RebuildsCacheAfterExplicitContentChange()
        {
            ItemDataSO definition = Item();
            ItemRegistrySO registry = Registry(definition);
            Assert.That(registry.TryGetItem("test-item", out _), Is.True);
            SetField(definition, "itemID", "renamed");
            registry.InvalidateLookup();
            Assert.That(registry.TryGetItem("test-item", out _), Is.False);
            Assert.That(registry.TryGetItem("renamed", out ItemDataSO found), Is.True);
            Assert.That(found, Is.SameAs(definition));
        }

        [Test]
        public void RuntimeInitializer_InvalidatesAlreadyLoadedRegistries()
        {
            ItemRegistrySO registry = Registry(Item());
            registry.TryGetItem("test-item", out _);
            MethodInfo initialize = typeof(ItemRegistryRuntimeInitializer).GetMethod(
                "Initialize", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(initialize, Is.Not.Null);
            initialize.Invoke(null, null);
            FieldInfo lookup = typeof(ItemRegistrySO).GetField("itemLookup", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(lookup.GetValue(registry), Is.Null);
            Assert.That(registry.TryGetItem("test-item", out _), Is.True);
        }

        [Test]
        public void RegisteredProjectContent_IsValidAndCanRoundTrip()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemRegistrySO", new[] { "Assets" });
            Assert.That(guids, Is.Not.Empty, "No registry assets were found in the project.");
            foreach (string guid in guids)
            {
                var registry = AssetDatabase.LoadAssetAtPath<ItemRegistrySO>(AssetDatabase.GUIDToAssetPath(guid));
                registry.InvalidateLookup();
                FieldInfo itemsField = typeof(ItemRegistrySO).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
                var items = (List<ItemDataSO>)itemsField.GetValue(registry);
                Assert.That(items, Is.Not.Null);
                foreach (ItemDataSO item in items)
                {
                    Assert.That(item, Is.Not.Null);
                    Assert.That(registry.TryGetItem(item.ItemID, out ItemDataSO found), Is.True);
                    Assert.That(found, Is.SameAs(item));
                    ItemInstance restored = ItemInstance.RestoreFromSaveData(new ItemInstance(item).CaptureSaveData(), registry);
                    Assert.That(restored.ItemData, Is.SameAs(item));
                }
            }
        }
    }
}
