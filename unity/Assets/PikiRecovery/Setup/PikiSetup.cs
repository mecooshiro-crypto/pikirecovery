// =====================================================================
//  PIKI RECOVERY · Instalación automática de dependencias (solo Editor)
//  Está en su propio ensamblado para compilar aunque falten paquetes.
//  Si el proyecto no tiene el paquete Unity UI (com.unity.ugui), lo
//  instala solo. Después Unity recompila y aparece el menú Piki Recovery.
// =====================================================================
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Piki.Setup
{
    [InitializeOnLoad]
    static class PikiSetup
    {
        static AddRequest request;

        static PikiSetup() { EditorApplication.delayCall += Check; }

        static bool HasUGUI() { return System.Type.GetType("UnityEngine.UI.Text, UnityEngine.UI") != null; }

        static void Check()
        {
            if (HasUGUI() || request != null) return;
            if (SessionState.GetBool("PikiRecovery.InstallingUGUI", false)) return;
            SessionState.SetBool("PikiRecovery.InstallingUGUI", true);
            Debug.Log("Piki Recovery: instalando el paquete Unity UI (com.unity.ugui)…");
            request = Client.Add("com.unity.ugui");
            EditorApplication.update += Progress;
        }

        static void Progress()
        {
            if (request == null || !request.IsCompleted) return;
            EditorApplication.update -= Progress;
            if (request.Status == StatusCode.Success)
                Debug.Log("Piki Recovery: Unity UI instalado. Unity va a recompilar y después aparece el menú Piki Recovery ▸ Construir escenas.");
            else
                Debug.LogError("Piki Recovery: no se pudo instalar Unity UI (" + (request.Error != null ? request.Error.message : "error desconocido") + "). Instalalo desde Window ▸ Package Manager ▸ Unity Registry ▸ Unity UI.");
            request = null;
        }
    }
}
