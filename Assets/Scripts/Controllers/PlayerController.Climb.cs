using RPGTest.Interactibles;
using UnityEngine;
using RPGTest.UI;
using RPGTest.Helpers;
using System.Collections.Generic;

namespace RPGTest.Controllers
{
    public partial class PlayerController
    {
        private Collider m_climbingInteration;
        private List<Collider> m_allClimbingColliders = new List<Collider>();
        private Collider m_currentClimbingCollider;

        private bool m_isClimbing = false;
        private int m_climbableLayer = 7;
        private readonly float m_angleTolerance = 40.0f;

        public Collider GetCurrentClimbingCollider()
        {
            return m_currentClimbingCollider;
        }

        public List<Collider> GetAllClimbingColliders()
        {
            return m_allClimbingColliders;
        }

        public void InitiateClimbing(Collider surface)
        {
            ClimbableSurface climbableSurface = surface.gameObject.GetComponent<ClimbableSurface>();
            if (surface == climbableSurface.TopCollider || surface == climbableSurface.BottomCollider)
            {
                m_climbingInteration = surface;
            }
            else
            {
                if(m_allClimbingColliders.Count == 0)
                {
                    m_currentClimbingCollider = surface;
                }

                if(!m_allClimbingColliders.Contains(surface))
                {
                    m_allClimbingColliders.Add(surface);
                }
            }
        }
        
        public bool ValidateClimbingAngle(Collider surface)
        {
            float angle = Vector3.Angle(transform.forward, surface.gameObject.transform.up);
            if (angle.IsInRange( 180 - m_angleTolerance, 180 + m_angleTolerance))
            {
                return true;
            }
            return false;
        }

        public void ClimbingInteractionVisibility(bool visibility)
        {
            if(visibility)
            {
                m_UIManager.GetUIComponent<UI_MainLayer>().DisplayInteractionPanel("Climb");
            }
            else
            {
                m_UIManager.GetUIComponent<UI_MainLayer>().HideInteractionPanel();
            }
        }

        /// <summary>
        /// Climbing via contact
        /// </summary>
        /// <param name="surface"></param>
        public void BeginClimbing(Collider surface)
        {
            ClimbingInteractionVisibility(false);
            _velocity.y = 0;
            m_isGrounded = false;
            m_isClimbing = true;

            SetClimbindPositionViaJump(surface);
            transform.forward = -surface.transform.up;
        }

        /// <summary>
        /// Climbing via interaction
        /// </summary>
        public void BeginClimbing()
        {
            ClimbingInteractionVisibility(false);
            _velocity.y = 0;
            m_isGrounded = false;
            m_isClimbing = true;            
            
            SetClimbingPositionViaInteraction();
            transform.forward = -m_climbingInteration.gameObject.transform.up;
        }

        public void ExitClimbing(Collider surface)
        {
            if(m_climbingInteration == surface)
            {
                m_climbingInteration = null;
                m_UIManager.GetUIComponent<UI_MainLayer>().HideInteractionPanel();
            }
            else
            {
                m_allClimbingColliders.Remove(surface);
                if(m_allClimbingColliders.Count == 0)
                {
                    m_currentClimbingCollider = null;
                    m_isClimbing = false;
                }
            }
        }

        /// <summary>
        /// Stop climbing from cancel action
        /// </summary>
        public void ExitClimbing()
        {
            m_isClimbing = false;
            m_climbingInteration = null;
        }

        public void ApplyClimbingMovement()
        {
            Vector3 direction = transform.up * m_playerDirection.y + transform.right * m_playerDirection.x;

            float directionLength = direction.magnitude;
            direction = direction.normalized * directionLength;

            if (direction != Vector3.zero)
            {
                m_controller.Move(Vector3.ClampMagnitude(direction, 0.5f) * Time.deltaTime * Speed);
            }
            
            //Check if we're still on a climbing surface
            if (CheckSurfaceCollision(transform.position, out RaycastHit hit))
            {
                m_currentClimbingCollider = hit.collider;
                ClimbableSurface climbableSurface = hit.collider.gameObject.GetComponent<ClimbableSurface>();
                transform.forward = climbableSurface.GetFacingDirection(transform);
                transform.position = climbableSurface.GetPositionViaJump(transform.position);
            }
            else
            {
                if (m_currentClimbingCollider == null)
                    return;

                ClimbableSurface climbableSurface = m_currentClimbingCollider.GetComponent<ClimbableSurface>();
                if (climbableSurface == null)
                    return;

                Collider newCollider = null;
                int nbAttemps = 0;
                if (m_playerDirection.y < 0 && climbableSurface.BottomConnection != null)
                {
                    ClimbableSurface connection = climbableSurface.BottomConnection.GetComponent<ClimbableSurface>();
                    transform.forward = connection.GetFacingDirection(transform);

                    Vector3 tmpPosition = transform.position;
                    tmpPosition -= transform.forward;

                    do
                    {
                        tmpPosition -= transform.up * 0.05f;
                        if (climbableSurface.TryGetPositionForBottomPlaneConnection(tmpPosition, transform.forward, out Vector3 newPosition))
                        {
                            transform.position = newPosition;
                            if (CheckSurfaceCollision(transform.position, out hit) && hit.collider == connection.MainCollider)
                            {
                                transform.position += connection.transform.up * 0.2f;
                                newCollider = connection.MainCollider;
                                break;
                            }
                        }
                    } while (nbAttemps++ < 20);
                }
                else if (m_playerDirection.y > 0 && climbableSurface.TopConnection != null)
                {
                    ClimbableSurface connection = climbableSurface.TopConnection.GetComponent<ClimbableSurface>();
                    transform.forward = connection.GetFacingDirection(transform);

                    Vector3 tmpPosition = transform.position;
                    tmpPosition -= transform.forward;

                    do
                    {
                        tmpPosition += transform.up * 0.05f;
                        
                        if (climbableSurface.TryGetPositionForTopPlaneConnection(tmpPosition, transform.forward, out Vector3 newPosition))
                        {
                            transform.position = newPosition + connection.transform.up * 0.2f;
                            if (m_allClimbingColliders.Contains(connection.MainCollider))
                            {
                                newCollider = connection.MainCollider;
                                break;
                            }
                        }
                    } while(nbAttemps++ < 15);
                }

                if(newCollider != null)
                {
                    if (!m_allClimbingColliders.Contains(newCollider))
                    {
                        m_allClimbingColliders.Add(newCollider);
                    }
                    m_currentClimbingCollider = newCollider;
                }

            }

            //If going downward and ground below
            if (m_playerDirection.y < 0 && CheckGround())
            {
                ExitClimbing();
            }
        }

        private void SetClimbingPositionViaInteraction()
        {
            ClimbableSurface climbableSurface = m_climbingInteration.gameObject.GetComponent<ClimbableSurface>();
            // transform.position = climbableSurface.GetPosition(transform.position, m_climbingInteration == climbableSurface.TopCollider);

            transform.position = climbableSurface.GetPositionViaInteraction(transform.position, m_climbingInteration);
        }

        private void SetClimbindPositionViaJump(Collider surface)
        {
            ClimbableSurface climbableSurface = surface.gameObject.GetComponent<ClimbableSurface>();
            transform.position = climbableSurface.GetPositionViaJump(transform.position);
        }

        /// <summary>
        /// Check weither or not the player is still facing a climbable surface on each movement iteration
        /// </summary>
        /// <returns></returns>
        private bool CheckSurfaceCollision(Vector3 initialPosition, out RaycastHit hit)
        {
            initialPosition -= (transform.forward / 3);
            initialPosition += transform.up * Height;
            int layerMask = (1 << m_climbableLayer); //Only on climbable layer

            if (Physics.Raycast(initialPosition, transform.forward, out hit, Height * 3, layerMask))
            {
                ClimbableSurface climbableSurface = hit.collider.gameObject.GetComponent<ClimbableSurface>();
                //climbableSurface.SetDebug(initialPosition, hit.point);
                if (hit.collider == climbableSurface.TopCollider)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

