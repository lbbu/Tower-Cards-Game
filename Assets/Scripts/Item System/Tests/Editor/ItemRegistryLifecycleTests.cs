using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace TowerCards.Items.Tests
{
    public class ItemRegistryLifecycleTests
    {
        private const string RegistryPath = "Assets/Items Data/Item Regestry/ItemsRegistry.asset";
        private const string BackupKey = "TowerCards.ItemTests.ReloadSettingsBackedUp";
        private const string EnabledKey = "TowerCards.ItemTests.OriginalReloadEnabled";
        private const string OptionsKey = "TowerCards.ItemTests.OriginalReloadOptions";
        private const string StaleID = "__item_test_stale_cache__";

        [SetUp]
        public void BackUpEditorSettings()
        {
            if (SessionState.GetBool(BackupKey, false))
                return;
            SessionState.SetBool(EnabledKey, EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(OptionsKey, (int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(BackupKey, true);
        }

        [UnityTest]
        public IEnumerator CacheResets_WithDomainAndSceneReload()
        {
            return VerifyCacheReset(EnterPlayModeOptions.None);
        }

        [UnityTest]
        public IEnumerator CacheResets_WithDomainReloadDisabled()
        {
            return VerifyCacheReset(EnterPlayModeOptions.DisableDomainReload);
        }

        [UnityTest]
        public IEnumerator CacheResets_WithSceneReloadDisabled()
        {
            return VerifyCacheReset(EnterPlayModeOptions.DisableSceneReload);
        }

        [UnityTest]
        public IEnumerator CacheResets_WithBothReloadsDisabled()
        {
            return VerifyCacheReset(EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload);
        }

        private static IEnumerator VerifyCacheReset(EnterPlayModeOptions options)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = options;
            var registry = AssetDatabase.LoadAssetAtPath<ItemRegistrySO>(RegistryPath);
            Assert.That(registry, Is.Not.Null);
            var items = (List<ItemDataSO>)typeof(ItemRegistrySO).GetField(
                "items", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(registry);
            Assert.That(items, Is.Not.Empty);

            typeof(ItemRegistrySO).GetField("itemLookup", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(registry, new Dictionary<string, ItemDataSO> { [StaleID] = items[0] });
            Assert.That(registry.TryGetItem(StaleID, out _), Is.True, "The stale-cache precondition was not established.");

            bool expectDomainReload = (options & EnterPlayModeOptions.DisableDomainReload) == 0;
            yield return new EnterPlayMode(expectDomainReload);

            registry = AssetDatabase.LoadAssetAtPath<ItemRegistrySO>(RegistryPath);
            Assert.That(registry.TryGetItem(StaleID, out _), Is.False, "A stale lookup survived entering Play Mode.");
            items = (List<ItemDataSO>)typeof(ItemRegistrySO).GetField(
                "items", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(registry);
            Assert.That(registry.TryGetItem(items[0].ItemID, out ItemDataSO found), Is.True);
            Assert.That(found, Is.SameAs(items[0]));
            yield return new ExitPlayMode();
        }

        [UnityTearDown]
        public IEnumerator RestoreEditorSettings()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                yield return new ExitPlayMode();
            EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(EnabledKey, true);
            EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(OptionsKey, 0);
            SessionState.EraseBool(BackupKey);
            var registry = AssetDatabase.LoadAssetAtPath<ItemRegistrySO>(RegistryPath);
            if (registry != null)
                registry.InvalidateLookup();
        }
    }
}
