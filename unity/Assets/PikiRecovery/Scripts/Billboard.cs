// PIKI RECOVERY · Billboard (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    public class Billboard : MonoBehaviour
    {
        public bool yOnly;
        void LateUpdate()
        {
            var cam = PikiRig.CamT; if (cam == null) return;
            Vector3 d = transform.position - cam.position; if (yOnly) d.y = 0;
            if (d.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(d, Vector3.up);
        }
    }
}
