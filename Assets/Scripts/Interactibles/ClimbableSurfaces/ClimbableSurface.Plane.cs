using RPGTest.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Interactibles
{
    public partial class ClimbableSurface
    {
        private Vector3 GetPlanePositionFromTop(Vector3 otherTransform)
        {
            Vector3 outPoint;
            if (MathExtension.LinePlaneIntersection(out outPoint, otherTransform, -transform.up, transform.up, transform.position))
            {
                return CheckDistanceFromTop(otherTransform, outPoint);
            }
            return new Vector3();
        }

        private Vector3 GetPlanePositionFromBottom(Vector3 otherTransform)
        {
            Vector3 outPoint;
            if (MathExtension.LinePlaneIntersection(out outPoint, otherTransform, -transform.up, transform.up, transform.position))
            {
                return CheckDistanceFromBottom(otherTransform, outPoint);
            }
            return new Vector3();
        }

        /// <summary>
        /// Get the position the other object (mainly playable character) should be assuming when moving from one climbable surface to their 'top' counterpart 
        /// </summary>
        /// <param name="otherTransform">Position of the other object (mainly playable character)</param>
        /// <returns>The adjusted position</returns>
        public bool TryGetPositionForTopPlaneConnection(Vector3 otherPosition, Vector3 otherForward, out Vector3 result)
        {
            return TryGetPositionForPlaneConnection(otherPosition, otherForward, TopConnection, out result);
        }

        /// <summary>
        /// Get the position the other object (mainly playable character) should be assuming when moving from one climbable surface to their 'bottom' counterpart 
        /// </summary>
        /// <param name="otherTransform">Position of the other object (mainly playable character)</param>
        /// <returns>The adjusted position</returns>
        public bool TryGetPositionForBottomPlaneConnection(Vector3 otherPosition, Vector3 otherForward, out Vector3 result)
        {
            return TryGetPositionForPlaneConnection(otherPosition, otherForward, BottomConnection, out result);
        }

        private bool TryGetPositionForPlaneConnection(Vector3 otherPosition, Vector3 otherForward, GameObject connection, out Vector3 result)
        {
            result = new Vector3();

            Ray ray = new Ray(otherPosition, otherForward);
            if (Physics.Raycast(ray, out RaycastHit hit, 50, LayerMask.GetMask("Climbable")))
            {
                if (hit.collider.gameObject == connection)
                {
                    Vector3 adjustedPoint;
                    if (MathExtension.LinePlaneIntersection(out adjustedPoint, otherPosition, -connection.transform.up, connection.transform.up, connection.transform.position))
                    {
                        result = adjustedPoint;
                        return true;
                    }
                    result = hit.point;
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// It's a plane. return the normal to make the player face the proper direction
        /// </summary>
        /// <returns></returns>
        private Vector3 GetPlaneFacingDirection()
        {
            return -transform.up;
        }
    }
}
