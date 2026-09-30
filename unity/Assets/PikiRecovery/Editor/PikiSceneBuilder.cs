// =====================================================================
//  PIKI RECOVERY · Constructor de escenas (solo Editor)
//  Menú: Piki Recovery ▸ Construir escenas
//  Genera 4 escenas .unity con todo el contenido guardado y editable:
//    00_Inicio     → hub de inicio + elección de deporte
//    01_Cancha     → estadio al final del partido + etapa 1 (física)
//    02_Vestuario  → vestuario + etapa 2 (nutrición)
//    03_Calma      → estadio de noche + etapa 3 (respiración) + final
//  Las texturas, materiales, mallas y sprites se guardan en
//  Assets/PikiRecovery/Generated. Las escenas se agregan a Build Settings.
// =====================================================================
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Piki.EditorTools
{
    [InitializeOnLoad]
    public static class PikiSceneBuilder
    {
        const string Root = "Assets/PikiRecovery";
        const string ScenesDir = Root + "/Scenes";
        const string GenDir = Root + "/Generated";

        struct Def { public string name; public PikiGame.StartAt start; public Def(string n, PikiGame.StartAt s) { name = n; start = s; } }
        static readonly Def[] Defs =
        {
            new Def(Scenes.Inicio, PikiGame.StartAt.Inicio),
            new Def(Scenes.Cancha, PikiGame.StartAt.Cancha),
            new Def(Scenes.Vestuario, PikiGame.StartAt.Nutricion),
            new Def(Scenes.Calma, PikiGame.StartAt.Calma),
        };

        static string curDir; static int counter; static readonly List<Object> created = new List<Object>();

        // La primera vez que se importan los scripts, ofrece construir las escenas
        static PikiSceneBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (File.Exists(ScenesDir + "/" + Scenes.Inicio + ".unity")) return;
                if (SessionState.GetBool("PikiRecovery.Asked", false)) return;
                SessionState.SetBool("PikiRecovery.Asked", true);
                if (EditorUtility.DisplayDialog("Piki Recovery",
                    "¿Construir ahora las 4 escenas de Piki Recovery?\n\n00_Inicio · 01_Cancha · 02_Vestuario · 03_Calma\n\nTambién podés hacerlo después desde el menú Piki Recovery ▸ Construir escenas.",
                    "Construir", "Más tarde"))
                    BuildAll();
            };
        }

        [MenuItem("Piki Recovery/Construir escenas", false, 1)]
        public static void BuildAllMenu()
        {
            bool exists = File.Exists(ScenesDir + "/" + Scenes.Inicio + ".unity");
            if (exists && !EditorUtility.DisplayDialog("Piki Recovery",
                "Las escenas ya existen. Si las reconstruís se reemplazan (se pierden los cambios que les hayas hecho a mano).\n\n¿Reconstruir?",
                "Reconstruir", "Cancelar")) return;
            BuildAll();
        }

        [MenuItem("Piki Recovery/Abrir escena 00 · Inicio", false, 20)] static void Open0() { OpenScene(Scenes.Inicio); }
        [MenuItem("Piki Recovery/Abrir escena 01 · Cancha", false, 21)] static void Open1() { OpenScene(Scenes.Cancha); }
        [MenuItem("Piki Recovery/Abrir escena 02 · Vestuario", false, 22)] static void Open2() { OpenScene(Scenes.Vestuario); }
        [MenuItem("Piki Recovery/Abrir escena 03 · Calma", false, 23)] static void Open3() { OpenScene(Scenes.Calma); }
        static void OpenScene(string n)
        {
            string p = ScenesDir + "/" + n + ".unity";
            if (!File.Exists(p)) { EditorUtility.DisplayDialog("Piki Recovery", "Primero construí las escenas (Piki Recovery ▸ Construir escenas).", "OK"); return; }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(p);
        }

        public static void BuildAll()
        {
            if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog("Piki Recovery", "Salí del modo Play antes de construir las escenas.", "OK"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                if (AssetDatabase.IsValidFolder(GenDir)) AssetDatabase.DeleteAsset(GenDir);
                EnsureFolder(Root, "Generated"); EnsureFolder(Root, "Scenes");
                Spr.ClearCache(); Tex.ClearCache();
                Persist.Save = SaveAsset;
                for (int i = 0; i < Defs.Length; i++)
                {
                    var d = Defs[i];
                    EditorUtility.DisplayProgressBar("Piki Recovery", "Construyendo " + d.name + "…", (float)i / Defs.Length);
                    EnsureFolder(GenDir, d.name); curDir = GenDir + "/" + d.name; counter = 0; created.Clear();

                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    PikiSession.match = null;
                    var go = new GameObject("Piki Recovery (juego)");
                    var game = go.AddComponent<PikiGame>(); game.startAt = d.start;
                    game.Bake(false, true);

                    foreach (var o in created) if (o != null) EditorUtility.SetDirty(o);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene, ScenesDir + "/" + d.name + ".unity");
                }
                AssetDatabase.SaveAssets();
                AddToBuildSettings();
            }
            finally
            {
                Persist.Save = null; EditorUtility.ClearProgressBar(); AssetDatabase.Refresh();
            }
            EditorSceneManager.OpenScene(ScenesDir + "/" + Scenes.Inicio + ".unity");
            EditorUtility.DisplayDialog("Piki Recovery",
                "¡Listo! Se crearon las 4 escenas en Assets/PikiRecovery/Scenes y se agregaron a Build Settings.\n\nEstá abierta 00_Inicio: apretá Play para jugar desde el principio.",
                "OK");
        }

        static void SaveAsset(Object o, string name)
        {
            if (o == null || AssetDatabase.Contains(o)) return;
            string ext = o is Material ? ".mat" : ".asset";
            string safe = name.Replace('/', '_').Replace(' ', '_');
            AssetDatabase.CreateAsset(o, curDir + "/" + safe + "_" + (counter++) + ext);
            created.Add(o);
        }
        static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }
        static void AddToBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var d in Defs) list.Add(new EditorBuildSettingsScene(ScenesDir + "/" + d.name + ".unity", true));
            foreach (var s in EditorBuildSettings.scenes) if (!list.Exists(x => x.path == s.path)) list.Add(s);
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
