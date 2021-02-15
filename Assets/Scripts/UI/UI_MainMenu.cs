using RPGTest.Managers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI
{
    public class UI_MainMenu : UI_Base
    {
        public Button DefaultButton;
        public GameObject MainPanel;
        public GameObject SettingsPanel;

        public GameManager GameManager;
        public bool QuitRequiresConfirmation = false;

        public string Scene;
        public string Exit;

        public void Start()
        {
            InitMenu(DefaultButton);
        }


        public void InitMenu(Button button)
        {
            button.Select();
        }


        public void NewGame()
        {
            StartCoroutine(PerformNewGame());
        }

        public void LoadGame()
        {
            GameManager.LoadSaveFile("File01");
        }

        public void InitSettings()
        {

        }

        public void ExitGame()
        {
            if (QuitRequiresConfirmation)
            {
                Debug.Log("TODO : ADD POPUP");
            }
            Application.Quit();
        }

        private IEnumerator PerformNewGame()
        {
            yield return StartCoroutine(Fade(false));
            GameManager.NewGame(Scene, Exit);
        }
    }
}
