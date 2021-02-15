using UnityEngine;
using RPGTest.Inputs;
using RPGTest.Managers;

[AddComponentMenu("Camera-Control")]
public class NoClippingCameraController : MonoBehaviour
{
    public Transform target;

    public float targetHeight = 1.7f;
    public float distance = 10.0f;
    public float offsetFromWall = 0.1f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public int zoomRate = 40;
    public float zoomDampening = 5.0f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    public LayerMask collisionLayers = 2;

    private float currentDistance;
    private float desiredDistance = 4;
    private float correctedDistance;

    public Controls playerInput;
    private Vector2 m_lookDirection;

    private float m_currentX;
    private float m_currentY;

    public void Awake()
    {
        playerInput = new Controls();

    }
    public void OnEnable() => playerInput.Enable();
    public void OnDisable() => playerInput.Disable();

    // Use this for initialization
    void Start()
    {
        FindObjectOfType<UIManager>().MenuChanged += OnMenuChanged;
    }
    

    #region InputSystem events
    public void Look()
    {
        m_lookDirection = playerInput.Player.Look.ReadValue<Vector2>();
    }
    #endregion

    private void OnMenuChanged(bool inMenu)
    {
        this.enabled = !inMenu;
    }


    void LateUpdate()
    {
        Vector3 vTargetOffset;
        if (target != null)
        {
            m_currentX += m_lookDirection.x * distance * Time.deltaTime * 20;
            m_currentY -= m_lookDirection.y * distance * Time.deltaTime * 20;
            m_currentY = ClampAngle(m_currentY, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(m_currentY, m_currentX, 0);

            // calculate distance 
            desiredDistance = Mathf.Clamp(desiredDistance, distanceMin, distanceMax);
            correctedDistance = desiredDistance;

            // calculate camera position
            vTargetOffset = new Vector3(0, -targetHeight, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + vTargetOffset);

            RaycastHit collisionHit;
            Vector3 trueTargetPosition = new Vector3(target.position.x, target.position.y + targetHeight, target.position.z);

            bool isCorrected = false;

            if (Physics.Linecast(trueTargetPosition, position, out collisionHit, collisionLayers))
            {
                // calculate and remove clipping
                correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                isCorrected = true;
            }

            currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening) : correctedDistance;

            currentDistance = Mathf.Clamp(currentDistance, distanceMin, distanceMax);

            position = target.position - (rotation * Vector3.forward * currentDistance + vTargetOffset);

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public bool IsValid()
    {
        return true;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}