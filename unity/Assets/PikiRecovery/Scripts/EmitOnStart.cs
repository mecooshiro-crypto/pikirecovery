// PIKI RECOVERY · EmitOnStart (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    /* ------------------------------ Componentes de animación del entorno ------------------------------ */
    public class EmitOnStart : MonoBehaviour
    {
        public int count = 100;
        void Start() { var ps = GetComponent<ParticleSystem>(); if (ps == null) return; ps.Play(); ps.Emit(count); }
    }
}
