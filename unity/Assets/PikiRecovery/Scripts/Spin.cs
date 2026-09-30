// PIKI RECOVERY · Spin (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    public class Spin : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0, 10, 0);
        void Update() { transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self); }
    }
}
