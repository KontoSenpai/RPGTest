using UnityEngine;

namespace RPGTest.Battle
{
    public class BattleModel : MonoBehaviour
    {
        public GameObject Cursor;
        public Vector3 DefaultPosition;
        public GameObject DamageSprite;

        private GameObject m_cursor;

        // Start is called before the first frame update
        void Start()
        {
            m_cursor = Instantiate(Cursor);
            m_cursor.transform.parent = this.transform;
            m_cursor.transform.localPosition = DefaultPosition;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ToggleCursor(bool visibility)
        {
            m_cursor.GetComponent<MeshRenderer>().enabled = visibility;
        }
    }
}

