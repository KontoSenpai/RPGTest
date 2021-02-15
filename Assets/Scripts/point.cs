using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Test.Core
{
    public class point : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        public void OnLook(InputValue value)
        {
            var toto = value.Get<Vector2>();
            this.transform.RotateAround(transform.position, Vector2.up, toto.x * 0.1f);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


