using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGTest.Controllers
{
    public partial class PlayerController
    {

        public void ApplyGroundMovement()
        {
            Transform camera = Camera.main.transform;

            var direction = camera.forward * m_playerDirection.y + camera.right * m_playerDirection.x;

            float directionLength = direction.magnitude;
            direction.y = 0;
            direction = direction.normalized * directionLength;

            if (direction != Vector3.zero)
            {
                var local_Gravity = new Vector3(0, 0, 0);
                if (m_isGrounded && _velocity.y <= 0)
                {
                    local_Gravity = Mathf.Abs(m_groundAngle) > MaxAngle ? new Vector3(0, -1, 0) : new Vector3(0, -8, 0);
                }
                if (!m_isGrounded)
                {
                    direction.x /= Drag.x == 0 ? 1 : Drag.x;
                    direction.z /= Drag.z == 0 ? 1 : Drag.z;
                }

                m_controller.Move((Vector3.ClampMagnitude(direction, 1.0f) + local_Gravity) * Time.deltaTime * Speed);

                if (direction != Vector3.zero && m_playerDirection != Vector2.zero)
                    transform.forward = direction;
            }

            m_animator.SetFloat("MoveSpeed", direction.magnitude);

            if (!m_isGrounded || _velocity.y > 0)
            {
                ApplyGravity(direction);
            }
        }
    }

}
