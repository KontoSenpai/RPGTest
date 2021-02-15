using RPGTest.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI
{
    public class UI_Category_Button : MonoBehaviour
    {
        public TextMeshProUGUI Text;
        public ItemType ItemType;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public event CategoryFilterRequestedHandler CategoryFilterRequested;
        public delegate void CategoryFilterRequestedHandler(ItemType itemType);

        public void onClickCategory()
        {
            CategoryFilterRequested(ItemType);
        }

        public void onClickExit()
        {
            Debug.Log("Clicked exit");
        }
    }
}
