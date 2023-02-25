using RPGTest.Models;
using RPGTest.Modules.Battle;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Interactibles
{
    public class InteractibleEnemy : MonoBehaviour, IInteractible
    {
        [SerializeField] private List<EnemyReference> Enemies = new List<EnemyReference>();
        [SerializeField] private Enums.EncounterType EncounterType;
        [SerializeField] private string SpecialText;
        [SerializeField] private AudioClip Bgm;

        public void Initialize(List<EnemyReference> enemies, Enums.EncounterType encounterType, string specialText, AudioClip bgm)
        {
            Enemies = enemies;
            EncounterType = encounterType;
            SpecialText = specialText;
            Bgm = bgm;
        }

        // Triger battle initiation in the BattleManager
        public void Interact()
        {
            FindObjectOfType<BattleManager>().Initialize(Enemies, this.transform.parent.transform.position, EncounterType, SpecialText, Bgm);
            Destroy();
        }


        private void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}
