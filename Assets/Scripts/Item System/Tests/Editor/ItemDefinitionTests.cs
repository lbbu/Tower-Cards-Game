using System;
using NUnit.Framework;

namespace TowerCards.Items.Tests
{
    public class ItemDefinitionTests : ItemSystemTestFixture
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Construction_RejectsInvalidItemID(string id)
        {
            ItemDataSO item = Item();
            SetField(item, "itemID", id);
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(item));
        }

        [TestCase("basePrice", -1f)]
        [TestCase("basePrice", float.NaN)]
        [TestCase("basePrice", float.PositiveInfinity)]
        [TestCase("basePrice", float.NegativeInfinity)]
        [TestCase("itemWeight", -1f)]
        [TestCase("itemWeight", float.NaN)]
        [TestCase("itemWeight", float.PositiveInfinity)]
        [TestCase("itemWeight", float.NegativeInfinity)]
        public void Definition_RejectsInvalidNumericProperties(string field, float value)
        {
            ItemDataSO item = Item();
            SetField(item, field, value);
            Assert.Throws<InvalidOperationException>(() => item.ValidateDefinition());
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Definition_RejectsInvalidStackSize(int value)
        {
            ItemDataSO item = Item();
            SetField(item, "maxStackSize", value);
            Assert.Throws<InvalidOperationException>(() => item.ValidateDefinition());
        }

        [Test]
        public void Definition_RejectsNullListAndMissingModule()
        {
            ItemDataSO item = Item((ItemModuleSO)null);
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(item));
            SetField(item, "itemModules", null);
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(item));
        }

        [Test]
        public void Definition_RejectsAmbiguousDuplicateModuleTypes()
        {
            ItemDataSO item = Item(Create<SwordWeaponModuleSO>(), Create<SwordWeaponModuleSO>());
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(item));
        }

        [TestCase("baseDamage", -1f)]
        [TestCase("baseDamage", float.NaN)]
        [TestCase("baseDamage", float.PositiveInfinity)]
        [TestCase("hitsPerSecond", -1f)]
        [TestCase("hitsPerSecond", float.NaN)]
        [TestCase("hitsPerSecond", float.PositiveInfinity)]
        public void SwordModule_RejectsInvalidConfiguration(string field, float value)
        {
            var module = Create<SwordWeaponModuleSO>();
            SetField(module, field, value);
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(Item(module)));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void DurabilityModule_RejectsInvalidMaximum(float value)
        {
            var module = Create<DurabilityModuleSO>();
            SetField(module, "maxDurability", value);
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(Item(module)));
        }

        [Test]
        public void StatelessModuleAndOptionalVisuals_AreSupported()
        {
            var module = Create<SwordWeaponModuleSO>();
            ItemDataSO definition = Item(module);
            Assert.That(definition.TryGetModule(out SwordWeaponModuleSO found), Is.True);
            Assert.That(found, Is.SameAs(module));
            Assert.That(definition.TryGetModule(out DurabilityModuleSO _), Is.False);
            Assert.That(new ItemInstance(definition).CaptureSaveData().states, Is.Empty);
        }

        [Test]
        public void Construction_RejectsDuplicateStateTypesFromDifferentModules()
        {
            var first = Create<FirstTestModule>();
            var second = Create<SecondTestModule>();
            first.StateFactory = () => new TestState { ID = "first" };
            second.StateFactory = () => new TestState { ID = "second" };
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(Item(first, second)));
        }

        [Test]
        public void Construction_RejectsDuplicateStateIDsAcrossDifferentTypes()
        {
            var first = Create<FirstTestModule>();
            var second = Create<SecondTestModule>();
            first.StateFactory = () => new TestState();
            second.StateFactory = () => new OtherTestState();
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(Item(first, second)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Construction_RejectsInvalidStateID(string id)
        {
            var module = Create<FirstTestModule>();
            module.StateFactory = () => new TestState { ID = id };
            Assert.Throws<InvalidOperationException>(() => new ItemInstance(Item(module)));
        }

        [Test]
        public void Construction_RejectsInvalidStateDataTypes()
        {
            var module = Create<FirstTestModule>();
            ItemDataSO item = Item(module);
            foreach (Type type in new[] { null, typeof(string), typeof(ItemModuleStateData), typeof(GenericTestStateData<>) })
            {
                module.StateFactory = () => new TestState { DataType = type };
                Assert.Throws<InvalidOperationException>(() => new ItemInstance(item), $"Type: {type}");
            }
        }

        [Test]
        public void Capture_RejectsNullAndIncompatibleDto()
        {
            var module = Create<FirstTestModule>();
            foreach (ItemModuleStateData data in new ItemModuleStateData[] { null, new OtherTestStateData() })
            {
                module.StateFactory = () => new TestState { Capture = () => data };
                var instance = new ItemInstance(Item(module));
                Assert.Throws<InvalidOperationException>(() => instance.CaptureSaveData());
            }
        }
    }
}
