using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace TowerCards.Items.Tests
{
    public class ItemSerializationTests : ItemSystemTestFixture
    {
        private ItemDataSO definition;
        private ItemRegistrySO registry;

        [SetUp]
        public void SetUpItem()
        {
            definition = Item(Create<DurabilityModuleSO>());
            registry = Registry(definition);
        }

        private ItemSaveData SaveWithJson(string json)
        {
            ItemSaveData saved = new ItemInstance(definition).CaptureSaveData();
            saved.states.Find(state => state.stateID == "durability").jsonData = json;
            return saved;
        }

        [TestCase("{}")]
        [TestCase("{\"wrongField\":55}")]
        [TestCase("{\"currentDurability\":null}")]
        [TestCase("{\"currentDurability\":55,\"wrongField\":1}")]
        [TestCase("{\"currentDurability\":{}}")]
        [TestCase("[]")]
        [TestCase("{not-json}")]
        [TestCase("{\"currentDurability\":55} {}")]
        public void Restore_RejectsMalformedOrIncompleteDurabilityJson(string json)
        {
            ItemSaveData saved = SaveWithJson(json);
            Assert.Catch<JsonException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
        }

        [TestCase("null")]
        [TestCase("")]
        [TestCase("   ")]
        public void Restore_RejectsMissingStatePayload(string json)
        {
            ItemSaveData saved = SaveWithJson(json);
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
        }

        [TestCase("{\"currentDurability\":-1}")]
        [TestCase("{\"currentDurability\":101}")]
        [TestCase("{\"currentDurability\":\"NaN\"}")]
        [TestCase("{\"currentDurability\":\"Infinity\"}")]
        [TestCase("{\"currentDurability\":\"-Infinity\"}")]
        public void Restore_RejectsInvalidDurabilityValue(string json)
        {
            ItemSaveData saved = SaveWithJson(json);
            Assert.Throws<ArgumentOutOfRangeException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
        }

        [TestCase(0f)]
        [TestCase(55f)]
        [TestCase(100f)]
        public void JsonRoundTrip_PreservesIdentityDefinitionAndIndependentState(float durability)
        {
            var original = new ItemInstance(definition);
            Assert.That(original.TryGetState(out DurabilityState originalState), Is.True);
            originalState.SetCurrentDurability(durability);
            string json = JsonConvert.SerializeObject(original.CaptureSaveData());
            ItemSaveData saved = JsonConvert.DeserializeObject<ItemSaveData>(json);
            ItemInstance restored = ItemInstance.RestoreFromSaveData(saved, registry);

            Assert.That(restored.InstanceID, Is.EqualTo(original.InstanceID));
            Assert.That(restored.ItemData, Is.SameAs(definition));
            Assert.That(restored.TryGetState(out DurabilityState restoredState), Is.True);
            Assert.That(restoredState.CurrentDurability, Is.EqualTo(durability));
            Assert.That(restoredState, Is.Not.SameAs(originalState));
            restoredState.SetCurrentDurability(durability == 0f ? 1f : 0f);
            Assert.That(originalState.CurrentDurability, Is.EqualTo(durability));
        }

        [Test]
        public void NewInstances_HaveUniqueIDsAndIndependentDurability()
        {
            var first = new ItemInstance(definition);
            var second = new ItemInstance(definition);
            Assert.That(first.InstanceID, Is.Not.EqualTo(second.InstanceID));
            Assert.That(Guid.TryParseExact(first.InstanceID, "N", out _), Is.True);
            first.TryGetState(out DurabilityState firstState);
            second.TryGetState(out DurabilityState secondState);
            firstState.TakeDamage(30f);
            Assert.That(firstState.CurrentDurability, Is.EqualTo(70f));
            Assert.That(secondState.CurrentDurability, Is.EqualTo(100f));
        }

        [Test]
        public void StatelessItems_RoundTripWithEmptyStates()
        {
            foreach (ItemDataSO item in new[] { Item(), Item(Create<SwordWeaponModuleSO>()) })
            {
                var original = new ItemInstance(item);
                ItemSaveData saved = JsonConvert.DeserializeObject<ItemSaveData>(
                    JsonConvert.SerializeObject(original.CaptureSaveData()));
                ItemInstance restored = ItemInstance.RestoreFromSaveData(saved, Registry(item));
                Assert.That(saved.states, Is.Empty);
                Assert.That(restored.InstanceID, Is.EqualTo(original.InstanceID));
                Assert.That(restored.TryGetState(out DurabilityState _), Is.False);
            }
        }

        [Test]
        public void SaveSchema_RetainsExistingFieldNamesAndStateID()
        {
            ItemSaveData saved = new ItemInstance(definition).CaptureSaveData();
            var outer = Newtonsoft.Json.Linq.JObject.Parse(JsonConvert.SerializeObject(saved));
            Assert.That(outer.Count, Is.EqualTo(3));
            Assert.That(outer["itemID"].Value<string>(), Is.EqualTo("test-item"));
            Assert.That(outer["instanceID"].Value<string>(), Is.EqualTo(saved.instanceID));
            Assert.That(outer["states"][0]["stateID"].Value<string>(), Is.EqualTo("durability"));
            Assert.That(Newtonsoft.Json.Linq.JObject.Parse(saved.states[0].jsonData)["currentDurability"].Value<float>(), Is.EqualTo(100f));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("unknown")]
        public void Restore_RejectsInvalidItemID(string id)
        {
            ItemSaveData saved = new ItemInstance(definition).CaptureSaveData();
            saved.itemID = id;
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Restore_RejectsInvalidInstanceID(string id)
        {
            ItemSaveData saved = new ItemInstance(definition).CaptureSaveData();
            saved.instanceID = id;
            Assert.Throws<ArgumentException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
        }

        [Test]
        public void Restore_RejectsMissingDuplicateUnknownAndNullStates()
        {
            ItemSaveData saved = new ItemInstance(definition).CaptureSaveData();
            saved.states.Add(saved.states[0]);
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
            saved.states.RemoveAt(1);
            saved.states[0].stateID = "unknown";
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
            saved.states[0].stateID = " ";
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
            saved.states[0] = null;
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
            saved.states.Clear();
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
            saved.states = null;
            Assert.Throws<InvalidOperationException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
        }

        [Test]
        public void PublicBoundaries_RejectNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ItemInstance(null));
            Assert.Throws<ArgumentNullException>(() => ItemInstance.RestoreFromSaveData(null, registry));
            Assert.Throws<ArgumentNullException>(() => ItemInstance.RestoreFromSaveData(new ItemSaveData(), null));
        }

        [Test]
        public void StateSerializer_IsIndependentOfGlobalDefaults()
        {
            Func<JsonSerializerSettings> previous = JsonConvert.DefaultSettings;
            try
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Error = (_, args) => args.ErrorContext.Handled = true
                };
                var original = new ItemInstance(definition);
                original.TryGetState(out DurabilityState state);
                state.SetCurrentDurability(0f);
                ItemSaveData saved = original.CaptureSaveData();
                Assert.That(saved.states[0].jsonData, Does.Not.Contain("$type"));
                Assert.That(saved.states[0].jsonData, Does.Contain("currentDurability"));
                ItemInstance restored = ItemInstance.RestoreFromSaveData(saved, registry);
                restored.TryGetState(out DurabilityState restoredState);
                Assert.That(restoredState.CurrentDurability, Is.Zero);
                saved.states[0].jsonData = "{}";
                Assert.Catch<JsonException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
                saved.states[0].jsonData = "{\"currentDurability\":null}";
                Assert.Catch<JsonException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
            }
            finally
            {
                JsonConvert.DefaultSettings = previous;
            }
        }

        [Test]
        public void Restore_DoesNotHonorTypeMetadata()
        {
            ItemSaveData saved = SaveWithJson("{\"$type\":\"System.Version, mscorlib\",\"currentDurability\":55}");
            Assert.Catch<JsonException>(() => ItemInstance.RestoreFromSaveData(saved, registry));
        }
    }
}
