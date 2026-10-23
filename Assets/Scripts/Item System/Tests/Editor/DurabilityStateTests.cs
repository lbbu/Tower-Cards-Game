using System;
using NUnit.Framework;

namespace TowerCards.Items.Tests
{
    public class DurabilityStateTests
    {
        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_RejectsInvalidMaximum(float maximum)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DurabilityState(maximum));
        }

        [TestCase(0.5f)]
        [TestCase(100f)]
        public void Constructor_StartsAtFinitePositiveMaximum(float maximum)
        {
            Assert.That(new DurabilityState(maximum).CurrentDurability, Is.EqualTo(maximum));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void TakeDamage_RejectsInvalidAmountWithoutMutation(float amount)
        {
            var state = new DurabilityState(100f);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.TakeDamage(amount));
            Assert.That(state.CurrentDurability, Is.EqualTo(100f));
        }

        [Test]
        public void TakeDamage_PreservesFiniteGameplayClamping()
        {
            var state = new DurabilityState(100f);
            state.TakeDamage(0f);
            Assert.That(state.CurrentDurability, Is.EqualTo(100f));
            state.TakeDamage(30f);
            Assert.That(state.CurrentDurability, Is.EqualTo(70f));
            state.TakeDamage(float.MaxValue);
            Assert.That(state.CurrentDurability, Is.Zero);
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void SetCurrent_RejectsNonFiniteWithoutMutation(float value)
        {
            var state = new DurabilityState(100f);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetCurrentDurability(value));
            Assert.That(state.CurrentDurability, Is.EqualTo(100f));
        }

        [TestCase(-50f, 0f)]
        [TestCase(55f, 55f)]
        [TestCase(150f, 100f)]
        public void SetCurrent_ClampsFiniteValues(float value, float expected)
        {
            var state = new DurabilityState(100f);
            state.SetCurrentDurability(value);
            Assert.That(state.CurrentDurability, Is.EqualTo(expected));
        }

        [TestCase(-1f)]
        [TestCase(101f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Restore_RejectsInvalidValueWithoutMutation(float value)
        {
            var state = new DurabilityState(100f);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.RestoreState(
                new DurabilityStateData { currentDurability = value }));
            Assert.That(state.CurrentDurability, Is.EqualTo(100f));
        }

        [TestCase(0f)]
        [TestCase(100f)]
        public void Restore_AcceptsBoundaryValues(float value)
        {
            var state = new DurabilityState(100f);
            state.RestoreState(new DurabilityStateData { currentDurability = value });
            Assert.That(state.CurrentDurability, Is.EqualTo(value));
        }

        [Test]
        public void Restore_RejectsNullAndWrongDtoType()
        {
            var state = new DurabilityState(100f);
            Assert.Throws<ArgumentException>(() => state.RestoreState(null));
            Assert.Throws<ArgumentException>(() => state.RestoreState(new OtherTestStateData()));
            Assert.That(state.CurrentDurability, Is.EqualTo(100f));
        }

        [Test]
        public void CapturedDto_IsIndependentFromRuntimeState()
        {
            var state = new DurabilityState(100f);
            var captured = (DurabilityStateData)state.CaptureState();
            state.TakeDamage(30f);
            Assert.That(captured.currentDurability, Is.EqualTo(100f));
            captured.currentDurability = 1f;
            Assert.That(state.CurrentDurability, Is.EqualTo(70f));
        }
    }
}
