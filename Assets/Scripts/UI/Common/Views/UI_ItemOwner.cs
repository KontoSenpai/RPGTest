using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public enum OwnerDisplayType
    {
        Icon,
        Name,
        Detailled,
        None
    }

    public class UI_ItemOwner : MonoBehaviour
    {
        public OwnerDisplayType OwnerDisplayType = OwnerDisplayType.None;

        [SerializeField] private GameObject IconsList;
        [SerializeField] private GameObject IconGo;
        private List<GameObject> m_instantiatedIcons; // TODO

        [SerializeField] private TextMeshProUGUI Text;

        public void SetOwnerDisplay(List <PlayableCharacter> owners, PresetSlot preset, Slot slot)
        {
            if (owners.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            switch (OwnerDisplayType)
            {
                case OwnerDisplayType.None:
                    IconsList.SetActive(false);
                    Text.gameObject.SetActive(false);
                    return;
                case OwnerDisplayType.Icon:
                    IconsList.SetActive(true);
                    Text.gameObject.SetActive(false);

                    SetOwnerIcons(owners);
                    return;
                case OwnerDisplayType.Name:
                    IconsList.SetActive(false);
                    Text.gameObject.SetActive(true);

                    Text.text = string.Join(" - ", owners.Select((o) => o.Name));

                    return;

                case OwnerDisplayType.Detailled:
                    IconsList.SetActive(false);
                    Text.gameObject.SetActive(true);

                    SetDetailledOwners(owners, preset, slot);

                    return;
            }
        }

        private void SetOwnerIcons(List<PlayableCharacter> owners)
        {
            CleanIcons();
            foreach (var owner in owners)
            {
                var iconGo = Instantiate(IconGo);
                iconGo.transform.SetParent(IconsList.transform);
                iconGo.transform.localScale = new Vector3(1, 1, 1);
                iconGo.GetComponent<RawImage>().texture = (Texture2D)Resources.Load($"Portraits/{owner.Id}");
                iconGo.name = owner.Id;
            }
        }

        private void SetDetailledOwners(List<PlayableCharacter> owners, PresetSlot preset, Slot slot)
        {
            if (owners.Count > 1)
            {
                Debug.LogWarning("Detailled Display mode should not be used with stacked owners");
                return;
            }

            var owner = owners[0];

            Text.text = $"{owner.Name} : {preset} - {slot}";
        }

        private void CleanIcons()
        {
            m_instantiatedIcons.ForEach((g) => Destroy(g));
            m_instantiatedIcons.Clear();
        }
    }
}
