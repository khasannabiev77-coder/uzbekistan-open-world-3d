using UnityEngine;
using UnityEngine.Rendering;
using UzOpenWorld.CameraSystem;
using UzOpenWorld.Localization;
using UzOpenWorld.PlayerSystem;
using UzOpenWorld.UI;
using UzOpenWorld.World;

namespace UzOpenWorld.Core
{
    /// <summary>
    /// PHASE 1 kirish nuqtasi. Sahnadagi bitta bo'sh GameObject'ga ("_Bootstrap") qo'shiladi
    /// va Play bosilganda hamma narsani quradi:
    ///   managerlar -> dunyo (Toshkent prototipi) -> yorug'lik -> o'yinchi -> kamera -> HUD.
    /// Keyingi bosqichlarda dunyo prefab/Addressables orqali yuklanadi, bootstrap esa
    /// faqat managerlarni ishga tushiradi.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Performance")]
        [Tooltip("Mobil uchun 30/45/60. Keyin Settings menyusiga o'tadi.")]
        public int targetFrameRate = 60;

        [Header("World")]
        public bool buildPrototypeWorld = true;
        public int worldSeed = 2026;
        [Tooltip("Ixtiyoriy: URP/Lit material. Bo'sh bo'lsa pipeline'ning default materiali ishlatiladi.")]
        public Material worldBaseMaterial;

        [Header("Debug")]
        [Tooltip("Editor'da Play bosilganda darhol mobil boshqaruvni ko'rsatish")]
        public bool forceMobileControlsInEditor;

        public PlayerController PlayerInstance { get; private set; }

        static GameBootstrap _active;

        void OnDestroy()
        {
            if (_active == this) _active = null;
        }

        void Awake()
        {
            if (_active != null && _active != this)
            {
                Debug.LogWarning("[GameBootstrap] Sahnada bir nechta GameBootstrap bor. Ortiqchasi o'chirildi.");
                Destroy(gameObject);
                return;
            }
            _active = this;

            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            CreateManagers();

            Vector3 spawn = new Vector3(0f, 1f, 0f);
            Quaternion spawnRot = Quaternion.identity;
            if (buildPrototypeWorld)
            {
                var world = new GameObject("World_Tashkent_Prototype").AddComponent<PrototypeWorldBuilder>();
                world.Build(worldSeed, worldBaseMaterial);
                spawn = world.SpawnPoint;
                spawnRot = world.SpawnRotation;
            }

            SetupLighting();

            PlayerInstance = PlayerFactory.CreatePlaceholderPlayer(spawn, spawnRot);

            Camera cam = SetupCamera();
            var tpc = cam.GetComponent<ThirdPersonCamera>();
            tpc.SetTarget(PlayerInstance.transform, PlayerInstance);
            PlayerInstance.cameraTransform = cam.transform;

            new GameObject("[HUD]").AddComponent<GameHUD>().Build();

            Debug.Log("[GameBootstrap] PHASE 1 prototipi tayyor.");
        }

        void CreateManagers()
        {
            var managers = new GameObject("[Managers]");
            if (LocalizationManager.Instance == null) managers.AddComponent<LocalizationManager>();
            var input = InputManager.Instance != null ? InputManager.Instance : managers.AddComponent<InputManager>();
            if (Application.isEditor && forceMobileControlsInEditor) input.forceMobileControls = true;
        }

        static void SetupLighting()
        {
            Light sun = null;
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sun = l; break; }

            if (sun == null)
            {
                sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            // PLACEHOLDER: keyinchalik DayNightManager boshqaradi
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.70f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.56f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.30f, 0.28f, 0.25f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.74f, 0.81f, 0.88f);
            RenderSettings.fogStartDistance = 120f;
            RenderSettings.fogEndDistance = 850f;
        }

        static Camera SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 900f;
            if (cam.GetComponent<ThirdPersonCamera>() == null) cam.gameObject.AddComponent<ThirdPersonCamera>();
            return cam;
        }
    }
}
