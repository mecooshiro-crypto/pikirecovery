// PIKI RECOVERY · Runner (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    /* ------------------------------ Tweens / corrutinas ------------------------------ */
    public class Runner : MonoBehaviour
    {
        static Runner inst;
        // Objeto que corre las animaciones y corrutinas (queda en la escena: "Procesos (animaciones)")
        public static Runner I
        {
            get
            {
                if (inst == null) inst = UnityEngine.Object.FindFirstObjectByType<Runner>();
                if (inst == null) inst = new GameObject("Procesos (animaciones)").AddComponent<Runner>();
                return inst;
            }
        }
        void Awake() { inst = this; }
    }
}
