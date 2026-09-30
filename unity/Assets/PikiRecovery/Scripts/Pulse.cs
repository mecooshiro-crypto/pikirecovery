// PIKI RECOVERY · Pulse (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    public class Pulse : MonoBehaviour
    {
        public float amount = .02f, speed = .8f, phase; Vector3 baseScale;
        void Start() { baseScale = transform.localScale; }
        void Update() { transform.localScale = baseScale * (1 + Mathf.Sin(Time.time * speed + phase) * amount); }
    }
}
