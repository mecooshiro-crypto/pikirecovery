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
        public static Runner I { get { if (inst == null) inst = new GameObject("PikiRunner").AddComponent<Runner>(); return inst; } }
    }
}
