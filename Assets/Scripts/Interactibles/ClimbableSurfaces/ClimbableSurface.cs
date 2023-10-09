using RPGTest.Controllers;
using RPGTest.Helpers;
using UnityEngine;

namespace RPGTest.Interactibles
{
    public enum SurfaceType
    {
        Plane,
        Cylinder
    }

    public enum DebugType
    {
        Jump,
        Top,
        Bottom,
        TopConnection,
        BottomConnection,
        LeftConnection,
        RightConnection
    }

    public partial class ClimbableSurface : MonoBehaviour
    {
        public SurfaceType SurfaceType;

        public bool IsDebug;
        public DebugType DebugType;
        public BoxCollider MainCollider;

        public BoxCollider TopCollider;
        public float MaxDistanceFromTop = 1.5f;
        public LayerMask LayersToHitForTopCheck;

        public BoxCollider BottomCollider;
        public float MaxDistanceFromFloor = 1.5f;
        public LayerMask LayersToHitForBottomCheck;

        public GameObject BottomConnection;
        public GameObject TopConnection;
        public GameObject LeftConnection;
        public GameObject RightConnection;

        private float timerWait = 3.0f;
        private float lastWait;
        private bool waitOver = true;

        //Debug
        private Vector3 m_debugVector;
        private bool m_hitConnected;
        private Vector3 m_debugResponse;

        private PlayerController m_controller => FindObjectOfType<PlayerController>();

        public void Start()
        {
            lastWait = Time.time;
        }

        public void Update()
        {
            if (IsDebug && m_controller.GetCurrentClimbingCollider() == MainCollider)
            {
                waitOver = (Time.time - lastWait) >= timerWait;
                if (waitOver)
                {
                    lastWait = Time.time;

                    m_debugVector = m_controller.transform.position;
                    
                    Vector3 oldPosition = m_controller.transform.position;
                    Vector3 oldForward = m_controller.transform.forward;

                    switch (DebugType)
                    {
                        case DebugType.Top:
                            m_debugResponse = GetPositionViaInteraction(m_debugVector, TopCollider);
                            break;
                        case DebugType.Bottom:
                            m_debugResponse = GetPositionViaInteraction(m_debugVector, BottomCollider);
                            break;
                        case DebugType.Jump:
                            m_debugResponse = GetPositionViaJump(m_debugVector);
                            break;
                        case DebugType.TopConnection:
                            if (TopConnection != null)
                            {
                                m_controller.transform.position += TopConnection.transform.up;
                                m_controller.transform.forward = -TopConnection.transform.up;
                                int loop = 0;
                                do
                                {
                                    m_controller.transform.position += m_controller.transform.up * 0.1f;
                                    m_debugVector = m_controller.transform.position;
                                    if(TryGetPositionForTopPlaneConnection(m_debugVector, m_controller.transform.forward, out m_debugResponse))
                                    {
                                        m_hitConnected = true;

                                        m_debugResponse += TopConnection.transform.up * 0.2f;

                                        m_controller.transform.position = oldPosition;
                                        m_controller.transform.forward = oldForward;
                                        return;
                                    }
                                } while (loop++ <= 15);
                                m_hitConnected = false;
                                m_debugResponse = m_debugVector - TopConnection.transform.up;
                            }
                            break;
                        case DebugType.BottomConnection:
                            if (BottomConnection != null)
                            {
                                m_controller.transform.position += BottomConnection.transform.up;
                                m_controller.transform.forward = -BottomConnection.transform.up;
                                int loop = 0;
                                do
                                {
                                    m_controller.transform.position -= m_controller.transform.up * 0.1f;
                                    m_debugVector = m_controller.transform.position;
                                    if (TryGetPositionForBottomPlaneConnection(m_debugVector, m_controller.transform.forward, out m_debugResponse))
                                    {
                                        m_hitConnected = true;

                                        m_debugResponse += BottomConnection.transform.up * 0.2f;

                                        m_controller.transform.position = oldPosition;
                                        m_controller.transform.forward = oldForward;
                                        return;
                                    }
                                } while (loop++ <= 15);
                                m_hitConnected = false;
                                m_debugResponse = m_debugVector - BottomConnection.transform.up;
                            }
                            break;
                        case DebugType.LeftConnection:
                            break;
                        case DebugType.RightConnection:
                            break;
                    }

                    m_controller.transform.position = oldPosition;
                    m_controller.transform.forward = oldForward;
                }
            }
        }

        /// <summary>
        /// Retrieve the "normal" vector for the appropriate surface.
        /// </summary>
        /// <param name="otherTransform">tranform of the reference object</param>
        /// <returns>normal vector of the climbable surface mathing the tranform caracteristics</returns>
        public Vector3 GetFacingDirection(Transform otherTransform)
        {
            Vector3 final;
            switch (SurfaceType)
            {
                case SurfaceType.Plane:
                    final = GetPlaneFacingDirection();
                    break;
                case SurfaceType.Cylinder:
                    final = GetCylinderFacingDirection(otherTransform);
                    break;
                default:
                    Debug.LogError("Unrecognized surface type");
                    final = new Vector3();
                    break;
            }
            return final;
        }

        /// <summary>
        /// Calculate and returns the position on which the interacting object (mainly playable character) should be when engaging climbing via interaction
        /// </summary>
        /// <param name="otherPosition">Position of the other object (mainly playable character)</param>
        /// <param name="interationCollider">Collider of this object who served as interaction engagement</param>
        /// <returns>Adjusted position on the climbable surface</returns>
        public Vector3 GetPositionViaInteraction(Vector3 otherPosition, Collider interationCollider)
        {
            Vector3 final;

            switch (SurfaceType)
            {
                case SurfaceType.Plane:
                    Vector3 position = otherPosition;
                    position.y += 0.2f;
                    if(interationCollider == TopCollider)
                    {
                        final = GetPlanePositionFromTop(position) + transform.up * 0.2f;
                    } 
                    else if (interationCollider == BottomCollider)
                    {
                        final = GetPlanePositionFromBottom(position) + transform.up * 0.2f;
                    }
                    else
                    {
                        final = new Vector3();
                    }
                    break;
                case SurfaceType.Cylinder:
                    //TODO
                    final = new Vector3();
                    break;
                default:
                    Debug.LogError("Unrecognized surface type");
                    final = new Vector3();
                    break;
            }

            return final;
        }

        /// <summary>
        /// Calculate and returns the position on which the other object (mainly playable character) should be when engaging climbing via jumping
        /// </summary>
        /// <param name="otherTransform">Position of the other object (mainly playable character)</param>
        /// <returns>Adjusted position on the climbable surface</returns>
        public Vector3 GetPositionViaJump(Vector3 otherPosition)
        {
            Vector3 response;
            if (MathExtension.LinePlaneIntersection(out response, otherPosition, -transform.up, transform.up, transform.position))
            {
                return response + transform.up * 0.2f;
            }
            return new Vector3();
        }

        public bool TryGetPositionForConnection(GameObject connection, Vector3 otherTransform, Vector3 otherForward, out Vector3 result)
        {
            result = new Vector3();
            switch(SurfaceType)
            {
                case SurfaceType.Plane:
                    if (connection == TopConnection)
                        return TryGetPositionForTopPlaneConnection(otherTransform, otherForward, out result);
                    if (connection == BottomConnection)
                        return TryGetPositionForBottomPlaneConnection(otherTransform, otherForward, out result);
                    if (connection == RightConnection)
                        return TryGetPositionForRightPlaneConnection(otherTransform, otherForward, out result);
                    if (connection == LeftConnection)
                        return TryGetPositionForLeftPlaneConnection(otherTransform, otherForward, out result);
                    break;
                case SurfaceType.Cylinder:
                    break;
                default:
                    Debug.LogError("Not supported");
                    break; 
            }
            return false;
        }

        private Vector3 CheckDistanceFromTop(Vector3 source, Vector3 IntersectionPoint)
        {
            Ray ray = new Ray(IntersectionPoint, Vector3.up);

            if (Physics.Raycast(ray, out RaycastHit hit, 50, LayersToHitForTopCheck))
            {
                float distanceFromCheckPoint = Mathf.Abs(IntersectionPoint.y - hit.point.y);
                bool inRange = distanceFromCheckPoint.IsInRange(MaxDistanceFromTop - 0.1f, MaxDistanceFromTop + 0.1f);

                if (!inRange)
                {
                    switch(SurfaceType)
                    {
                        case SurfaceType.Plane:
                            source.y += distanceFromCheckPoint > MaxDistanceFromTop ? 0.1f : -0.1f;
                            IntersectionPoint = GetPlanePositionFromTop(source);
                            break;
                    }
                }
            }

            return IntersectionPoint;
        }

        private Vector3 CheckDistanceFromBottom(Vector3 source, Vector3 IntersectionPoint)
        {
            Ray ray = new Ray(IntersectionPoint, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 50, LayersToHitForBottomCheck))
            {
                float distanceFromCheckPoint = Mathf.Abs(IntersectionPoint.y - hit.point.y);
                bool inRange = distanceFromCheckPoint.IsInRange(MaxDistanceFromFloor - 0.1f, MaxDistanceFromFloor + 0.1f);

                if (!inRange)
                {
                    switch (SurfaceType)
                    {
                        case SurfaceType.Plane:
                            source.y += distanceFromCheckPoint > MaxDistanceFromFloor ? 0.1f : -0.1f;
                            IntersectionPoint = GetPlanePositionFromBottom(source);
                            break;
                    }
                }
            }

            return IntersectionPoint;
        }

        // Show a Scene view example.
        void OnDrawGizmosSelected()
        {
            if(IsDebug)
            {
                // Show a connection between vector and response.
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(m_debugVector, m_debugResponse);

                // Now show the input position.
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(m_debugVector, 0.05f);

                // And finally the resulting position.
                Gizmos.color = m_hitConnected ? Color.green : Color.red;
                Gizmos.DrawSphere(m_debugResponse + transform.up * 0.2f, 0.05f);
            }         
        }
    }
}
