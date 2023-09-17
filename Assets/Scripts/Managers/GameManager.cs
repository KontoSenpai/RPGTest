using RPGTest.Collectors;
using RPGTest.Controllers;
using RPGTest.Interactibles;
using RPGTest.Models;
using RPGTest.Models.Items;
using RPGTest.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGTest.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Properties
        public bool MainMenu = false;

        public GameObject PlayerController;
        public Camera MainCamera;

        public UIManager UIManager;
        public PartyManager PartyManager;
        public InventoryManager InventoryManager;
        public AbilityManager AbilityManager;

        private static string CurrentScene = "Gym_Climbable";
        private static string CurrentExit = "Exit-EntryPoint";
        private static Vector3 PlayerPosition = new Vector3();
        private static Vector3 PlayerRotation = new Vector3();

        private SaveFileModel SaveFile => SaveFileModel.Instance;

        #endregion

        public void Start()
        {
            Application.targetFrameRate = 60;
            if(UIManager.GetUIComponent<UI_MainLayer>() != null)
            {
                UIManager.GetUIComponent<UI_MainLayer>().gameObject.SetActive(true);
            }         
            if (!MainMenu)
            {
                LoadSaveFileInformation();
                StartCoroutine(InitializePlayerPosition());
            }
        }

        public void NewGame(string sceneName, string sceneExit)
        {
            InitializeSaveFile();
            CreateSaveFile("FILE01");
            InitManagerProperties();
            StartCoroutine(ChangeScene(sceneName, sceneExit));
        }

        public void CreateSaveFile(string fileName)
        {
            SaveFileModel.CreateNew(fileName, PartyManager.GetAllPartyMembers(), InventoryManager.GetItems());
        }

        public void LoadSaveFile(string fileName)
        {
            SaveFileModel.LoadSaveFile(fileName);
            InitManagerProperties();
            StartCoroutine(ChangeScene(CurrentScene, PlayerPosition, PlayerRotation));
        }


        /// <summary>
        /// Load a new scene with informations of the player location on the new scene
        /// </summary>
        /// <param name="sceneName">New Scene to be loaded</param>
        /// <param name="position">Player position on the new scene</param>
        /// <param name="rotation">Player rotation on the new scene</param>
        /// <example>
        /// <code>
        /// ChangeScene("Gym_Complete", transform.position, transform.eulerAngles);
        /// </code>
        /// </example>
        public IEnumerator ChangeScene(string sceneName, Vector3 position, Vector3 rotation)
        {
            UpdateCurrentSceneState(position, rotation);
            CurrentScene = sceneName;
            CurrentExit = null;
            if (SaveFile != null)
            {
                SaveFile.CurrentScene = sceneName;
            }
            yield return StartCoroutine(UIManager.GetUIComponent<UI_MainLayer>().FadeLoading(true));
            SceneManager.LoadScene(CurrentScene);
        }
        
        public IEnumerator ChangeScene(string sceneName, string exitName)
        {
            CurrentScene = sceneName;
            CurrentExit = exitName;
            yield return StartCoroutine(UIManager.GetUIComponent<UI_MainLayer>().FadeLoading(true));
            SceneManager.LoadScene(CurrentScene);
        }

        #region SaveState methods
        /// <summary>
        /// Refresh the Current Scene informations and update the save file.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void UpdateCurrentSceneState(Vector3 position, Vector3 rotation)
        {
            UpdateCurrentSceneChestState();
            UpdatePlayerInformation(position, rotation);
        }

        /// <summary>
        /// Send party an inventory models to the Save File to keep it up-to-date.
        /// </summary>
        /// <param name="position"> Player position to be saved</param>
        /// <param name="rotation"> Player rotation to be saved</param>
        private void UpdatePlayerInformation(Vector3 position, Vector3 rotation)
        {
            PlayerPosition = position;
            PlayerRotation = rotation;

            if (SaveFile != null)
            {
                SaveFile.UpdatePlayerPosition(position, rotation);
                SaveFile.UpdateInventory(InventoryManager.GetItems());
            }
        }

        /// <summary>
        /// Updates the SaveFile with the current scene chests information
        /// </summary>
        private void UpdateCurrentSceneChestState()
        {
            if (SaveFile != null)
            {
                var interactibles = GameObject.FindGameObjectsWithTag("Interactible");
                var chests = interactibles.Where(c => c.name.StartsWith("Chest_"));
                SaveFile.UpdateChestStates(chests.Select(x => new ChestState(x.name, x.GetComponent<InteractibleChest>().GetChestInventory())).ToList());
            }
        }

        /// <summary>
        /// Initiate CurrentScene with the save file information, if it exists. Otherwise create temp party infos
        /// </summary>
        private void LoadSaveFileInformation()
        {
            if(SaveFile == null)
            {
                InitializeSaveFile();
            }
            else
            {
                if (SaveFile.Characters != null && SaveFile.Characters.Count > 0)
                {
                    foreach (var character in SaveFile.Characters)
                    {
                        PartyManager.TryAddPartyMember(character.Id);
                    }
                }
                if (SaveFile.CurrentSceneState != null)
                {
                    var interactibles = FindObjectsOfType<GameObject>().Where(x => x.CompareTag("Interactible"));
                    foreach (var chestState in SaveFile.CurrentSceneState.ChestStates)
                    {
                        interactibles.SingleOrDefault(x => x.name == chestState.ChestId).GetComponent<InteractibleChest>().UpdateInventory(chestState.RemainingItems);
                    }
                }
            }
        }
        #endregion

        #region Initializers
        private IEnumerator InitializePlayerPosition()
        {
            PlayerController.GetComponent<PlayerController>().enabled = false;
            if (PlayerController != null)
            {
                Vector3 position = Vector3.zero;
                Vector3 rotation = Vector3.zero;
                if (!string.IsNullOrEmpty(CurrentExit))
                {
                    var exit = FindObjectsOfType<InteractibleExit>().SingleOrDefault(x => x.name == CurrentExit);
                    if (exit != null)
                    {
                        position = exit.GetSpawnPosition();
                        rotation = exit.transform.eulerAngles;
                    }
                    CurrentExit = null;
                }
                else
                {
                    position = PlayerPosition;
                    rotation = PlayerRotation;
                }
                MainCamera.transform.position = position;
                PlayerController.transform.position = position;
                PlayerController.transform.eulerAngles = rotation;

                yield return new WaitForSeconds(0.1f);
                StartCoroutine(UIManager.GetUIComponent<UI_MainLayer>().FadeLoading(false));
                PlayerController.GetComponent<PlayerController>().enabled = true;
                MainCamera.GetComponent<NoClippingCameraController>().ResetCameraPosition();
                MainCamera.enabled = true;
            }
        }

        /// <summary>
        /// Create the basic party and inventory informations
        /// </summary>
        private void InitializeSaveFile()
        {
            // Add consummables
            InventoryManager.TryAddItem("C0001", 5);
            InventoryManager.TryAddItem("C0002", 4);

            // Add Equipments
            InventoryManager.TryAddItem("E0001", 4);
            InventoryManager.TryAddItem("E0002", 2);
            InventoryManager.TryAddItem("E0003", 2);
            InventoryManager.TryAddItem("E0007", 1);
            InventoryManager.TryAddItem("E0008", 1);

            // Add Party members
            PartyManager.TryAddPartyMember("PC0001");
            PartyManager.TryAddPartyMember("PC0002");
            
            PartyManager.TryAddPartyMember("PC0003");
            PartyManager.TryAddPartyMember("PC0004");
            PartyManager.TryAddPartyMember("PC0005");
            
            // Equip equipments
            List<Item> removedEquipments;
            var partyMember = PartyManager.TryGetPartyMemberById("PC0001");
            partyMember.TryEquip(PresetSlot.First, Enums.Slot.LeftHand, ItemCollector.TryGetEquipment("E0001"), out removedEquipments);
            partyMember.TryEquip(PresetSlot.First, Enums.Slot.RightHand, ItemCollector.TryGetEquipment("E0001"), out removedEquipments);
            partyMember.TryEquip(PresetSlot.Second, Enums.Slot.LeftHand, ItemCollector.TryGetEquipment("E0003"), out removedEquipments);            
        }

        private void InitManagerProperties()
        {
            if(SaveFile != null)
            {
                PlayerPosition = SaveFileModel.GetFloatArrayAsVector3(SaveFile.CurrentPosition);
                PlayerRotation = SaveFileModel.GetFloatArrayAsVector3(SaveFile.CurrentRotation);
                CurrentScene = SaveFile.CurrentScene;
            }
        }
        #endregion
    }
}
