using RPGTest.Models.Entity;
using RPGTest.Modules.Battle.UI;
using UnityEngine;

namespace RPGTest.Modules.Battle
{
    public class BattleEntity : MonoBehaviour
    {
        public GameObject DamageSprite;



        [SerializeField]
        private GameObject Cursor;
        [SerializeField]
        private Vector3 DefaultCursorPosition;

        [SerializeField]
        private Vector3 DefaultOverheadPosition;
        [SerializeField]
        private GameObject InformationPanel;

        // Start is called before the first frame update
        void Start()
        {
            Cursor.transform.localPosition += DefaultCursorPosition;
            InformationPanel.transform.localPosition += DefaultOverheadPosition;
        }

        // Update is called once per frame
        void Update()
        {
            var camera = FindObjectOfType<Camera>();
            if (InformationPanel != null && camera != null)
            {
                InformationPanel.gameObject.transform.LookAt( -1 * camera.gameObject.transform.position);
                //InformationPanel.gameObject.transform.Rotate(-camera.gameObject.transform.forward);
                //InformationPanel.gameObject.transform.Rotate(new Vector3(0, 180, 0));
            }
        }

        public GameObject Initialize(Entity entity, GameObject meshIntantiation, Vector3 position, float offset, int count = -2, bool party = false)
        {
            this.transform.position = new Vector3(position.x + (count > -2 ? 1 * count : 0), position.y - 0.5f, position.z + offset - (count * (party ? -0.4f : 0.4f)));
            this.transform.name = entity.Name;
            GameObject mesh = Instantiate(meshIntantiation, transform);

            mesh.transform.localEulerAngles = new Vector3(0, offset > 0 ? 195 : -15, 0);

            InformationPanel.GetComponent<UI_Overhead_Information>().Initialize(entity);
            return mesh;
        }

        public void ToggleCursor(bool visibility)
        {
            Cursor.GetComponent<MeshRenderer>().enabled = visibility;
            InformationPanel.GetComponent<Canvas>().enabled = visibility;
        }
    }
}
