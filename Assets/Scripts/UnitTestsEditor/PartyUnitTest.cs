using NUnit.Framework;
using RPGTest.Collectors;
using RPGTest.Managers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGTest.UnitTests.Party
{
    [TestFixture]
    public class PartyUnitTest
    {
        private PartyManager m_partyManager => Object.FindObjectOfType<PartyManager>();
        private void InitializeScene()
        {
            //GameManager
            if(Object.FindObjectOfType<GameManager>() == null)
            {
                var gameManager = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/GameManager.prefab", typeof(GameObject));
                Object.Instantiate((GameObject)gameManager);
            }
        }

        [Test]
        public void AddPartyMemberPass()
        {
            InitializeScene();

            Assert.True(m_partyManager.TryAddPartyMember("PC0001"));
        }

        [Test]
        public void GetPartyMemberPass()
        {
           Assert.AreEqual(m_partyManager.TryGetPartyMember("PC0001"), PlayableCharactersCollector.TryGetPlayableCharacter("PC0001"));
        }
    }
}

