// =====================================================================
//  PIKI RECOVERY · Constructor de escenas (solo Editor)
//  Menú: Piki Recovery ▸ Construir escenas
//  Genera 5 escenas .unity con todo el contenido guardado y editable:
//    00_Inicio     → hub de inicio + elección de deporte
//    01_Cancha     → estadio al final del partido + etapa 1 (física)
//    02_Vestuario  → vestuario + etapa 2 (nutrición)
//    03_Calma      → estadio de noche + etapa 3 (respiración) + final
//    04_Partido    → el próximo partido + barra de rendimiento
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

        struct Def
        {
            public string name, audio; public PikiGame.StartAt start; public float volume;
            public Def(string n, PikiGame.StartAt s, string audio = null, float volume = .5f) { name = n; start = s; this.audio = audio; this.volume = volume; }
        }
        static readonly Def[] Defs =
        {
            new Def(Scenes.Inicio, PikiGame.StartAt.Inicio),
            new Def(Scenes.Cancha, PikiGame.StartAt.Cancha, "Publico_Estadio.mp3", .55f),
            new Def(Scenes.Vestuario, PikiGame.StartAt.Nutricion, "Musica_Vestuario.mp3", .5f),
            new Def(Scenes.Calma, PikiGame.StartAt.Calma, "Musica_Calma.mp3", .12f),   // bien bajo, de fondo
            new Def(Scenes.Partido, PikiGame.StartAt.Partido, "Publico_Estadio.mp3", .55f),
        };

        static string curDir; static int counter; static readonly List<Object> created = new List<Object>();

        // La primera vez que se importan los scripts, ofrece construir las escenas
        static PikiSceneBuilder()
        {
            EditorApplication.playModeStateChanged -= CheckBeforePlay;
            EditorApplication.playModeStateChanged += CheckBeforePlay;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (File.Exists(ScenesDir + "/" + Scenes.Inicio + ".unity")) return;
                if (SessionState.GetBool("PikiRecovery.Asked", false)) return;
                SessionState.SetBool("PikiRecovery.Asked", true);
                if (EditorUtility.DisplayDialog("Piki Recovery",
                    "¿Construir ahora las 5 escenas de Piki Recovery?\n\n00_Inicio · 01_Cancha · 02_Vestuario · 03_Calma · 04_Partido\n\nTambién podés hacerlo después desde el menú Piki Recovery ▸ Construir escenas.",
                    "Construir", "Más tarde"))
                    BuildAll();
            };
        }

        // Si se borraron los assets generados (por ejemplo al reemplazar la carpeta), las escenas
        // quedan con materiales faltantes (todo rosa). Antes de dar Play se ofrece reconstruirlas.
        static void CheckBeforePlay(PlayModeStateChange st)
        {
            if (st != PlayModeStateChange.ExitingEditMode) return;
            var scene = EditorSceneManager.GetActiveScene();
            bool ours = scene.path.StartsWith(ScenesDir) || Object.FindFirstObjectByType<PikiGame>() != null;
            if (!ours) return;
            bool broken = !AssetDatabase.IsValidFolder(GenDir) || !File.Exists(ScenesDir + "/" + Scenes.Inicio + ".unity") || string.IsNullOrEmpty(scene.path);
            if (!broken) return;
            EditorApplication.isPlaying = false;
            if (EditorUtility.DisplayDialog("Piki Recovery",
                "Faltan los materiales y texturas de las escenas (por eso se vería todo rosa). Hay que construir las escenas de nuevo.\n\n¿Construirlas ahora?",
                "Construir", "Cancelar"))
                EditorApplication.delayCall += BuildAll;
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
                Spr.ClearCache(); Tex.ClearCache(); UI.ClearCache();
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
                    // Audio de fondo: un objeto en la escena con su AudioSource (loop constante, suena al iniciar)
                    if (d.audio != null)
                    {
                        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/" + d.audio);
                        if (clip != null)
                        {
                            var ago = new GameObject("Audio de la escena (" + Path.GetFileNameWithoutExtension(d.audio) + ")");
                            var src = ago.AddComponent<AudioSource>();
                            src.clip = clip; src.loop = true; src.playOnAwake = true; src.volume = d.volume; src.spatialBlend = 0; src.priority = 64;
                            game.sceneAudio = src;
                        }
                    }
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
                "¡Listo! Se crearon las 5 escenas en Assets/PikiRecovery/Scenes y se agregaron a Build Settings.\n\nEstá abierta 00_Inicio: apretá Play para jugar desde el principio.",
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
