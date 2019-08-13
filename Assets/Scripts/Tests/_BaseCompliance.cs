using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class _BaseCompliance
    {
        // A Test behaves as an ordinary method
        [Test]
        public void _BaseComplianceSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        [Test]
        public void _BaseCompliancePrefabTest()
        {
            var tileRef = Resources.Load<GameObject>("MagicCube/MagicCubModel");
            Assert.NotNull(tileRef);
            GameObject obj = MonoBehaviour.Instantiate(tileRef);
            Assert.NotNull(obj);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator _BaseComplianceWithEnumeratorPasses()
        {
            GameObject obj = Resources.Load<GameObject>("MagicCube/MagicCubModel");
            Assert.NotNull(obj);
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
