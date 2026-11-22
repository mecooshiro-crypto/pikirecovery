// =====================================================================
//  PIKI GAME · Constructor de escenas (solo Editor)
//  Menú: Piki Game ▸ Construir escenas
//  Genera 5 escenas + 4 fundidos .unity con todo el contenido guardado y editable:
//    00_Inicio     → hub de inicio + elección de deporte
//    01_Cancha     → estadio al final del partido + etapa 1 (física)
//    02_Vestuario  → vestuario + etapa 2 (nutrición)
//    03_Calma      → estadio de noche + etapa 3 (respiración) + final
//    04_Partido    → el próximo partido + barra de rendimiento
//  Las texturas, materiales, mallas y sprites se guardan en
//  Assets/PikiGame/Generated. Las escenas se agregan a Build Settings.
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
        const string Root = "Assets/PikiGame";
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

        // Escenas de fundido: (escena, kicker, título, subtítulo, color, siguiente, segundos)
        struct FadeDef { public string name, kicker, title, sub, next, color; public float hold; }
        static readonly FadeDef[] Fades =
        {
            new FadeDef { name = Scenes.FCancha, kicker = "FÚTBOL", title = "Estadio Piki", sub = "Final del partido · Piki FC 2 – 1 Visitante", color = "#22f0bf", next = Scenes.Cancha, hold = 1.8f },
            new FadeDef { name = Scenes.FVestuario, kicker = "ETAPA 2 DE 3", title = "Recuperación nutricional", sub = "Vestuario · Ventana de recuperación post-partido", color = "#ffd93d", next = Scenes.Vestuario, hold = 1.8f },
            new FadeDef { name = Scenes.FCalma, kicker = "ETAPA 3 DE 3", title = "Vuelta a la calma", sub = "El estadio se vació. El ruido se apagó.", color = "#b388ff", next = Scenes.Calma, hold = 2.4f },
            new FadeDef { name = Scenes.FPartido, kicker = "UNA SEMANA DESPUÉS", title = "Tu próximo partido", sub = "Así se nota en la cancha todo el trabajo invisible", color = "#22f0bf", next = Scenes.Partido, hold = 2f },
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
                if (SessionState.GetBool("PikiGame.Asked", false)) return;
                SessionState.SetBool("PikiGame.Asked", true);
                if (EditorUtility.DisplayDialog("Piki Game",
                    "¿Construir ahora las 5 escenas de Piki Game?\n\n00_Inicio · 01_Cancha · 02_Vestuario · 03_Calma · 04_Partido\n\nTambién podés hacerlo después desde el menú Piki Game ▸ Construir escenas.",
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
            if (EditorUtility.DisplayDialog("Piki Game",
                "Faltan los materiales y texturas de las escenas (por eso se vería todo rosa). Hay que construir las escenas de nuevo.\n\n¿Construirlas ahora?",
                "Construir", "Cancelar"))
                EditorApplication.delayCall += BuildAll;
        }

        [MenuItem("Piki Game/Construir escenas", false, 1)]
        public static void BuildAllMenu()
        {
            bool exists = File.Exists(ScenesDir + "/" + Scenes.Inicio + ".unity");
            if (exists && !EditorUtility.DisplayDialog("Piki Game",
                "Las escenas ya existen. Si las reconstruís se reemplazan (se pierden los cambios que les hayas hecho a mano).\n\n¿Reconstruir?",
                "Reconstruir", "Cancelar")) return;
            BuildAll();
        }

        [MenuItem("Piki Game/Abrir escena 00 · Inicio", false, 20)] static void Open0() { OpenScene(Scenes.Inicio); }
        [MenuItem("Piki Game/Abrir escena 01 · Cancha", false, 21)] static void Open1() { OpenScene(Scenes.Cancha); }
        [MenuItem("Piki Game/Abrir escena 02 · Vestuario", false, 22)] static void Open2() { OpenScene(Scenes.Vestuario); }
        [MenuItem("Piki Game/Abrir escena 03 · Calma", false, 23)] static void Open3() { OpenScene(Scenes.Calma); }
        [MenuItem("Piki Game/Abrir escena 04 · Partido", false, 24)] static void Open4() { OpenScene(Scenes.Partido); }
        [MenuItem("Piki Game/Restablecer opciones de accesibilidad", false, 40)] static void ResetAccess() { Access.ResetAll(); EditorUtility.DisplayDialog("Piki Game", "Se restablecieron las opciones de accesibilidad.", "OK"); }
        static void OpenScene(string n)
        {
            string p = ScenesDir + "/" + n + ".unity";
            if (!File.Exists(p)) { EditorUtility.DisplayDialog("Piki Game", "Primero construí las escenas (Piki Game ▸ Construir escenas).", "OK"); return; }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(p);
        }

        public static void BuildAll()
        {
            if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog("Piki Game", "Salí del modo Play antes de construir las escenas.", "OK"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                PikiImportSettings.EnsureAll(Root); Brand.Reload(); ApplyPlayerSettings();
                if (AssetDatabase.IsValidFolder(GenDir)) AssetDatabase.DeleteAsset(GenDir);
                EnsureFolder(Root, "Generated"); EnsureFolder(Root, "Scenes");
                Spr.ClearCache(); Tex.ClearCache(); UI.ClearCache();
                Persist.Save = SaveAsset;
                for (int i = 0; i < Defs.Length; i++)
                {
                    var d = Defs[i];
                    EditorUtility.DisplayProgressBar("Piki Game", "Construyendo " + d.name + "…", (float)i / Defs.Length);
                    EnsureFolder(GenDir, d.name); curDir = GenDir + "/" + d.name; counter = 0; created.Clear();

                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    PikiSession.match = null;
                    var go = new GameObject("Piki Game (juego)");
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
                // Escenas de fundido
                foreach (var f in Fades)
                {
                    EditorUtility.DisplayProgressBar("Piki Game", "Construyendo " + f.name + "…", .95f);
                    EnsureFolder(GenDir, f.name); curDir = GenDir + "/" + f.name; counter = 0; created.Clear();
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    Color c; ColorUtility.TryParseHtmlString(f.color, out c);
                    PikiTransition.Bake(f.kicker, f.title, f.sub, c, f.next, f.hold);
                    foreach (var o in created) if (o != null) EditorUtility.SetDirty(o);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene, ScenesDir + "/" + f.name + ".unity");
                }
                AssetDatabase.SaveAssets();
                AddToBuildSettings();
            }
            finally
            {
                Persist.Save = null; EditorUtility.ClearProgressBar(); AssetDatabase.Refresh();
            }
            EditorSceneManager.OpenScene(ScenesDir + "/" + Scenes.Inicio + ".unity");
            EditorUtility.DisplayDialog("Piki Game",
                "¡Listo! Se crearon las 5 escenas y los 4 fundidos en Assets/PikiGame/Scenes y se agregaron a Build Settings.\n\nEstá abierta 00_Inicio: apretá Play para jugar desde el principio.",
                "OK");
        }

        // Nombre e ícono de la aplicación según la marca
        static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = Brand.Game;
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Marca/piki-icono-app-1024.png");
            if (icon == null) return;
            try
            {
                var m = typeof(PlayerSettings).GetMethod("SetIconsForTargetGroup", new[] { typeof(BuildTargetGroup), typeof(Texture2D[]) });
                if (m != null) m.Invoke(null, new object[] { BuildTargetGroup.Unknown, new[] { icon } });
            }
            catch (System.Exception e) { Debug.LogWarning("Piki Game: no se pudo asignar el ícono (" + e.Message + "). Asignalo en Player Settings ▸ Icon."); }
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
            // Orden: cada fundido va justo antes de su etapa
            foreach (var d in Defs)
            {
                string f = Scenes.FadeFor(d.name);
                if (f != null) list.Add(new EditorBuildSettingsScene(ScenesDir + "/" + f + ".unity", true));
                list.Add(new EditorBuildSettingsScene(ScenesDir + "/" + d.name + ".unity", true));
            }
            foreach (var s in EditorBuildSettings.scenes) if (!list.Exists(x => x.path == s.path)) list.Add(s);
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
