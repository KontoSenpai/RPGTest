using RPGTest.Managers;
using UnityEngine;

namespace RPGTest.Interactibles
{
    public class InteractibleExit : MonoBehaviour, IInteractible
    {
        public LayerMask CollisionLayer;

        public string SceneName;
        public string ExitName;

        public Vector3 GetSpawnPosition()
        {
            Physics.Raycast(transform.position + (transform.forward * 2), Vector3.down, out RaycastHit hitInfo, 5.0f, CollisionLayer);
            var pos = hitInfo.point;
            pos.y += 0.3f;
            return pos;
        }

        public void Update()
        {

        }

        public void Interact()
        {
            if(!string.IsNullOrEmpty(SceneName) && !string.IsNullOrEmpty(ExitName))
             {
                StartCoroutine(FindObjectOfType<GameManager>().ChangeScene(SceneName, ExitName));
                this.enabled = false;
            }
        }

        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(GetSpawnPosition(), 0.2f);
        }
    }
}
