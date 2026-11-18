using UnityEngine;
using UzOpenWorld.World;

namespace UzOpenWorld.PlayerSystem
{
    /// <summary>
    /// PLACEHOLDER o'yinchi: primitivlardan yasalgan oddiy 3D qahramon (tana, bosh, do'ppi).
    /// Haqiqiy humanoid 3D model + Animator tayyor bo'lganda shu factory o'rniga
    /// Player prefab'i ishlatiladi (PlayerController va CharacterController o'zgarmaydi).
    /// </summary>
    public static class PlayerFactory
    {
        const int IgnoreRaycastLayer = 2; // kamera to'qnashuvi o'yinchini e'tiborsiz qoldirishi uchun

        public static PlayerController CreatePlaceholderPlayer(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("Player");
            root.tag = "Player";
            root.layer = IgnoreRaycastLayer;
            root.transform.SetPositionAndRotation(position, rotation);

            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 45f;
            cc.skinWidth = 0.04f;
            cc.minMoveDistance = 0f;

            var mats = new MaterialLibrary(null);
            var visual = new GameObject("Visual (PLACEHOLDER)");
            visual.layer = IgnoreRaycastLayer;
            visual.transform.SetParent(root.transform, false);

            Part(PrimitiveType.Capsule, "Body", visual.transform, new Vector3(0f, 0.75f, 0f), new Vector3(0.7f, 0.75f, 0.7f),
                mats.Get(new Color(0.15f, 0.42f, 0.72f)));
            Part(PrimitiveType.Sphere, "Head", visual.transform, new Vector3(0f, 1.58f, 0f), Vector3.one * 0.34f,
                mats.Get(new Color(0.85f, 0.67f, 0.52f)));
            Part(PrimitiveType.Cylinder, "Doppi", visual.transform, new Vector3(0f, 1.73f, 0f), new Vector3(0.3f, 0.05f, 0.3f),
                mats.Get(new Color(0.08f, 0.08f, 0.1f)));
            Part(PrimitiveType.Cube, "Nose", visual.transform, new Vector3(0f, 1.58f, 0.17f), new Vector3(0.06f, 0.06f, 0.06f),
                mats.Get(new Color(0.78f, 0.58f, 0.45f)));

            var player = root.AddComponent<PlayerController>();
            var anim = visual.AddComponent<PlaceholderCharacterAnimator>();
            anim.player = player;
            return player;
        }

        static void Part(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.layer = IgnoreRaycastLayer;
            Object.DestroyImmediate(go.GetComponent<Collider>()); // to'qnashuv faqat CharacterController'da
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
