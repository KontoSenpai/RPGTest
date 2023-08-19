using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Test.Core
{
    public class Point : MonoBehaviour
    {
        public void OnLook(InputValue value)
        {
            var toto = value.Get<Vector2>();
            this.transform.RotateAround(transform.position, Vector2.up, toto.x * 0.1f);
        }
    }
}


