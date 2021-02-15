using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace RPGTest.Initialization
{
    [TestFixture]
    public class _LoadSceneUnitTest
    {
        [Test]
        public void LoadScene()
        {
            SceneManager.LoadScene("Scenes/Gym", LoadSceneMode.Single);
        }
    }
}
