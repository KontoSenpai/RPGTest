using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace RPGTest.Models
{
    public class SaveFileModel
    {
        private static string DefaultWorld = "Gym_Complete";
  
        private static string SaveFilePath = @"D:\{0}.save";

        public string FileName { get; set; }
            
        public List<PlayableCharacter> Characters { get; set; }

        public Dictionary<string, int> Inventory { get; set; }

        public int Money { get; set; }

        public string CurrentScene { get; set; }
        public List<float> CurrentPosition { get; set; }
        public List<float> CurrentRotation { get; set; }

        public List<SceneState> SceneStates { get; set; } = new List<SceneState>();

        [YamlIgnore]
        public SceneState CurrentSceneState => SceneStates.SingleOrDefault(x => x.SceneName == CurrentScene);

        public int PlayTime { get; set; }

        public static SaveFileModel Instance { get; private set; }

        #region Static methods
        public static void CreateNew(string fileName, List<PlayableCharacter> party, Dictionary<string, int> inventory)
        {
            Instance = new SaveFileModel();
            Instance.FileName = fileName;

            Instance.Characters = party;
            Instance.Inventory = inventory;
            Instance.Money = 150;

            Instance.CurrentScene = DefaultWorld;
            Instance.CurrentPosition = new List<float>() { 0.0f, 0.0f, 0.0f };
            Instance.CurrentRotation = new List<float>() { 0.0f, 0.0f, 0.0f };
        }

        public static SaveFileModel LoadSaveFile(string fileName)
        {
            if(File.Exists(string.Format(SaveFilePath, fileName)))
            {
                Deserializer deserializer = new Deserializer();
                string content = File.ReadAllText(string.Format(SaveFilePath, fileName));
                Instance = deserializer.Deserialize<SaveFileModel>(content);
            }
            else
            {
                Instance = new SaveFileModel();
            }

            return Instance;
        }

        public static Vector3 GetFloatArrayAsVector3(List<float> list)
        {
            if (list != null && list.Count > 0)
                return new Vector3(list[0], list[1], list[2]);
            else
                return new Vector3(0, 0, 0);
        }

        public static SaveFileModel GetExistingSaves()
        {
            return null;
        }
        #endregion

        #region Update Information Methods
        public void UpdatePlayerPosition(Vector3 position, Vector3 rotation)
        {
            Instance.CurrentPosition = new List<float> { position.x, position.y, position.z };
            Instance.CurrentRotation = new List<float> { rotation.x, rotation.y, rotation.z };
        }

        public void UpdateInventory(Dictionary<string, int> items)
        {
            Instance.Inventory = items;
        }

        public void UpdateChestStates(List<ChestState> chestStates)
        {
            if(CurrentSceneState == null)
            {
                SceneStates.Add(new SceneState(CurrentScene));
            }

            CurrentSceneState.ChestStates = chestStates;
        }
        #endregion


        public void SaveToFile()
        {
            Serializer serializer = new Serializer();
            using (TextWriter file = File.CreateText(string.Format(SaveFilePath, Instance.FileName)))
            {
                serializer.Serialize(file, Instance);
            }
        }
    }

    public class SceneState
    {
        public string SceneName { get; set; }

        public List<ChestState> ChestStates { get; set; }

        public SceneState() { }

        public SceneState(string scene)
        {
            SceneName = scene;
        }
    }

    public class ChestState
    {
        public string ChestId { get; set; }

        public Dictionary<string, int> RemainingItems { get; set; }

        public ChestState() { }
        public ChestState(string id, Dictionary<string, int> items)
        {
            ChestId = id;
            RemainingItems = items;
        }
    }
}
