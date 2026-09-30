// PIKI RECOVERY · FlagWave (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    public class FlagWave : MonoBehaviour
    {
        public float phase;
        void Update() { transform.localEulerAngles = new Vector3(0, Mathf.Sin(Time.time * 2.2f + phase) * 28 + 30, 0); }
    }
}
