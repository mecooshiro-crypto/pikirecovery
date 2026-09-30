// PIKI RECOVERY · FollowCam (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    public class FollowCam : MonoBehaviour { void LateUpdate() { if (PikiRig.CamT != null) transform.position = PikiRig.CamT.position; } }
}
