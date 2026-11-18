using UnityEngine;

namespace UzOpenWorld.World
{
    /// <summary>
    /// PLACEHOLDER Toshkent hududi: protsedural "greybox" shahar.
    ///   - kvartallar to'ri, asfalt yo'llar, o'rta chiziqlar, trotuar (bordyur bilan)
    ///   - markazda baland biznes binolar, chetga qarab past turar-joylar
    ///   - ayrim past binolarda feruza gumbaz (o'zbek arxitekturasidan ilhom)
    ///   - markaziy xiyobon fontan bilan, daraxtlar, chiroq ustunlari, uzoqda tog'lar
    /// Maqsad: Phase 1'da harakat va kamerani real o'lchamdagi shaharda sinash.
    /// Phase 3'da haqiqiy 3D modellar (prefab) bilan almashtiriladi.
    /// </summary>
    public class PrototypeWorldBuilder : MonoBehaviour
    {
        [Header("Grid")]
        public int blocksX = 5;
        public int blocksZ = 5;
        public float blockSize = 60f;
        public float roadWidth = 12f;
        public float sidewalkWidth = 4f;

        [Header("Buildings")]
        public float maxHeightCenter = 70f;
        public float maxHeightEdge = 14f;
        [Range(0f, 1f)] public float emptyLotChance = 0.15f;
        [Range(0f, 1f)] public float parkBlockChance = 0.1f;

        [Header("Props")]
        public float treeSpacing = 15f;

        [Header("Performance")]
        [Tooltip("Barcha statik meshlarni birlashtiradi -> draw call'lar keskin kamayadi")]
        public bool combineStaticMeshes = true;

        public Vector3 SpawnPoint { get; private set; }
        public Quaternion SpawnRotation { get; private set; } = Quaternion.identity;

        static readonly Color Asphalt = new Color(0.17f, 0.17f, 0.19f);
        static readonly Color SidewalkColor = new Color(0.62f, 0.60f, 0.56f);
        static readonly Color GrassColor = new Color(0.32f, 0.48f, 0.22f);
        static readonly Color GroundColor = new Color(0.47f, 0.50f, 0.32f);
        static readonly Color LineColor = new Color(0.92f, 0.92f, 0.86f);
        static readonly Color TrunkColor = new Color(0.36f, 0.25f, 0.17f);
        static readonly Color Turquoise = new Color(0.08f, 0.60f, 0.66f);
        static readonly Color WaterColor = new Color(0.25f, 0.55f, 0.75f);
        static readonly Color MetalColor = new Color(0.22f, 0.23f, 0.25f);
        static readonly Color MountainColor = new Color(0.50f, 0.55f, 0.60f);

        static readonly Color[] WallPalette =
        {
            new Color(0.93f, 0.89f, 0.80f), // och bej
            new Color(0.95f, 0.95f, 0.93f), // oq
            new Color(0.82f, 0.56f, 0.42f), // terrakota (pishgan g'isht)
            new Color(0.76f, 0.78f, 0.80f), // beton
            new Color(0.87f, 0.78f, 0.62f), // qum rang
            new Color(0.62f, 0.74f, 0.84f), // ko'kimtir oyna
        };

        static readonly Color[] LeafPalette =
        {
            new Color(0.22f, 0.45f, 0.18f),
            new Color(0.30f, 0.52f, 0.20f),
            new Color(0.18f, 0.38f, 0.16f),
        };

        System.Random _rng;
        MaterialLibrary _mats;
        Transform _roads, _blocks, _props, _env;

        float Cell => blockSize + roadWidth;

        public void Build(int seed, Material template = null)
        {
            _rng = new System.Random(seed);
            _mats = new MaterialLibrary(template);
            _roads = Group("Roads");
            _blocks = Group("Blocks");
            _props = Group("Props");
            _env = Group("Environment");

            float totalX = blocksX * Cell + roadWidth;
            float totalZ = blocksZ * Cell + roadWidth;
            float originX = -blocksX * Cell * 0.5f; // birinchi yo'l markazi
            float originZ = -blocksZ * Cell * 0.5f;

            BuildGround(totalX, totalZ);
            BuildRoads(originX, originZ, totalX, totalZ);

            int cx = blocksX / 2, cz = blocksZ / 2;
            float maxDist = Mathf.Max(blocksX, blocksZ) * Cell * 0.5f * 1.2f;
            Vector3 centralBlock = Vector3.zero;

            for (int bx = 0; bx < blocksX; bx++)
            for (int bz = 0; bz < blocksZ; bz++)
            {
                var c = new Vector3(originX + bx * Cell + Cell * 0.5f, 0f, originZ + bz * Cell + Cell * 0.5f);
                if (bx == cx && bz == cz)
                {
                    centralBlock = c;
                    BuildPark(c, true);
                }
                else if (Rand01() < parkBlockChance)
                {
                    BuildPark(c, false);
                }
                else
                {
                    float d01 = Mathf.Clamp01(new Vector2(c.x, c.z).magnitude / maxDist);
                    BuildCityBlock(c, d01);
                }
            }

            BuildBoundary(totalX, totalZ);
            BuildMountains(Mathf.Max(totalX, totalZ));

            // Spawn: markaziy xiyobonning janubiy trotuari, shimolga qarab
            SpawnPoint = centralBlock + new Vector3(0f, 0.6f, -(blockSize * 0.5f - sidewalkWidth * 0.5f));
            SpawnRotation = Quaternion.identity;

            if (combineStaticMeshes) StaticBatchingUtility.Combine(gameObject);
        }

        // ------------------------------------------------------------------ ground & roads

        void BuildGround(float totalX, float totalZ)
        {
            Prim(PrimitiveType.Cube, "Ground", _env, new Vector3(0f, -0.5f, 0f),
                new Vector3(totalX + 1200f, 1f, totalZ + 1200f), _mats.Get(GroundColor), true, false);
        }

        void BuildRoads(float ox, float oz, float totalX, float totalZ)
        {
            var asphalt = _mats.Get(Asphalt, 0.05f);
            var line = _mats.Get(LineColor);

            for (int i = 0; i <= blocksX; i++)
            {
                float x = ox + i * Cell;
                Prim(PrimitiveType.Cube, "Road_NS", _roads, new Vector3(x, 0.01f, 0f), new Vector3(roadWidth, 0.02f, totalZ), asphalt, false, false);
                for (int b = 0; b < blocksZ; b++)
                {
                    float z = oz + b * Cell + Cell * 0.5f;
                    Prim(PrimitiveType.Cube, "Line", _roads, new Vector3(x, 0.03f, z), new Vector3(0.18f, 0.02f, blockSize - 2f), line, false, false);
                }
            }

            for (int k = 0; k <= blocksZ; k++)
            {
                float z = oz + k * Cell;
                Prim(PrimitiveType.Cube, "Road_EW", _roads, new Vector3(0f, 0.012f, z), new Vector3(totalX, 0.02f, roadWidth), asphalt, false, false);
                for (int b = 0; b < blocksX; b++)
                {
                    float x = ox + b * Cell + Cell * 0.5f;
                    Prim(PrimitiveType.Cube, "Line", _roads, new Vector3(x, 0.03f, z), new Vector3(blockSize - 2f, 0.02f, 0.18f), line, false, false);
                }
            }
        }

        // ------------------------------------------------------------------ city blocks

        void BuildCityBlock(Vector3 c, float d01)
        {
            Prim(PrimitiveType.Cube, "Sidewalk", _blocks, c + Vector3.up * 0.075f,
                new Vector3(blockSize, 0.15f, blockSize), _mats.Get(SidewalkColor), true, false);

            float inner = blockSize - 2f * sidewalkWidth;
            float lot = inner * 0.5f;
            float maxH = Mathf.Lerp(maxHeightCenter, maxHeightEdge, d01);

            for (int lx = 0; lx < 2; lx++)
            for (int lz = 0; lz < 2; lz++)
            {
                Vector3 lc = c + new Vector3((lx - 0.5f) * lot, 0.15f, (lz - 0.5f) * lot);

                if (Rand01() < emptyLotChance)
                {
                    Prim(PrimitiveType.Cube, "Courtyard", _blocks, lc + Vector3.up * 0.02f,
                        new Vector3(lot - 2f, 0.04f, lot - 2f), _mats.Get(GrassColor), false, false);
                    Tree(lc);
                    continue;
                }

                float w = lot * Range(0.6f, 0.9f);
                float d = lot * Range(0.6f, 0.9f);
                float h = Mathf.Max(6.4f, Mathf.Round(maxH * Range(0.4f, 1f) / 3.2f) * 3.2f);
                Building(lc, w, d, h);
            }

            StreetTrees(c);
            StreetLamps(c);
        }

        void Building(Vector3 basePos, float w, float d, float h)
        {
            Color wall = WallPalette[_rng.Next(WallPalette.Length)];
            var tiling = new Vector2(Mathf.Max(1f, Mathf.Round((w + d) * 0.5f / 3.5f)), Mathf.Max(1f, Mathf.Round(h / 3.2f)));

            Prim(PrimitiveType.Cube, "Building", _blocks, basePos + Vector3.up * (h * 0.5f),
                new Vector3(w, h, d), _mats.Building(wall, tiling), true, true);

            // Tom (yuqori yuzadagi derazalarni yopadi)
            Prim(PrimitiveType.Cube, "Roof", _blocks, basePos + Vector3.up * (h + 0.15f),
                new Vector3(w + 0.4f, 0.3f, d + 0.4f), _mats.Get(wall * 0.75f), false, true);

            if (h < 16f && Rand01() < 0.35f)
            {
                // O'zbek uslubidagi feruza gumbaz
                float r = Mathf.Min(w, d) * 0.32f;
                Prim(PrimitiveType.Cylinder, "Drum", _blocks, basePos + Vector3.up * (h + 0.9f),
                    new Vector3(r * 2f, 0.6f, r * 2f), _mats.Get(wall), false, true);
                Prim(PrimitiveType.Sphere, "Dome", _blocks, basePos + Vector3.up * (h + 1.5f),
                    new Vector3(r * 2f, r * 1.7f, r * 2f), _mats.Get(Turquoise, 0.6f), false, true);
            }
            else if (h > 40f)
            {
                Prim(PrimitiveType.Cube, "RoofTech", _blocks, basePos + Vector3.up * (h + 1.5f),
                    new Vector3(w * 0.4f, 2.4f, d * 0.4f), _mats.Get(MetalColor), false, true);
            }
        }

        void StreetTrees(Vector3 c)
        {
            float half = blockSize * 0.5f;
            float edge = half - sidewalkWidth * 0.5f;
            for (float t = -half + treeSpacing * 0.5f; t < half; t += treeSpacing)
            {
                Tree(c + new Vector3(t, 0.15f, edge));
                Tree(c + new Vector3(t, 0.15f, -edge));
                Tree(c + new Vector3(edge, 0.15f, t));
                Tree(c + new Vector3(-edge, 0.15f, t));
            }
        }

        void StreetLamps(Vector3 c)
        {
            float o = blockSize * 0.5f - 0.8f;
            var metal = _mats.Get(MetalColor);
            foreach (var p in new[] { new Vector3(o, 0, o), new Vector3(-o, 0, o), new Vector3(o, 0, -o), new Vector3(-o, 0, -o) })
            {
                Vector3 b = c + p + Vector3.up * 0.15f;
                Prim(PrimitiveType.Cube, "LampPole", _props, b + Vector3.up * 2.5f, new Vector3(0.15f, 5f, 0.15f), metal, true, true);
                Prim(PrimitiveType.Cube, "LampHead", _props, b + Vector3.up * 5f, new Vector3(0.5f, 0.15f, 0.5f), metal, false, false);
            }
        }

        void Tree(Vector3 pos)
        {
            float s = Range(0.8f, 1.2f);
            Prim(PrimitiveType.Cylinder, "Trunk", _props, pos + Vector3.up * (1.2f * s),
                new Vector3(0.3f * s, 1.2f * s, 0.3f * s), _mats.Get(TrunkColor), true, true);
            Prim(PrimitiveType.Sphere, "Crown", _props, pos + Vector3.up * (3.4f * s),
                Vector3.one * (3f * s), _mats.Get(LeafPalette[_rng.Next(LeafPalette.Length)]), false, true);
        }

        // ------------------------------------------------------------------ parks

        void BuildPark(Vector3 c, bool central)
        {
            Prim(PrimitiveType.Cube, "ParkGround", _blocks, c + Vector3.up * 0.075f,
                new Vector3(blockSize, 0.15f, blockSize), _mats.Get(GrassColor), true, false);

            var path = _mats.Get(SidewalkColor);
            Prim(PrimitiveType.Cube, "Path_NS", _blocks, c + Vector3.up * 0.16f, new Vector3(5f, 0.02f, blockSize), path, false, false);
            Prim(PrimitiveType.Cube, "Path_EW", _blocks, c + Vector3.up * 0.16f, new Vector3(blockSize, 0.02f, 5f), path, false, false);
            // Chetki trotuar halqasi
            float e = blockSize * 0.5f - sidewalkWidth * 0.5f;
            Prim(PrimitiveType.Cube, "Edge_N", _blocks, c + new Vector3(0, 0.16f, e), new Vector3(blockSize, 0.02f, sidewalkWidth), path, false, false);
            Prim(PrimitiveType.Cube, "Edge_S", _blocks, c + new Vector3(0, 0.16f, -e), new Vector3(blockSize, 0.02f, sidewalkWidth), path, false, false);
            Prim(PrimitiveType.Cube, "Edge_E", _blocks, c + new Vector3(e, 0.16f, 0), new Vector3(sidewalkWidth, 0.02f, blockSize), path, false, false);
            Prim(PrimitiveType.Cube, "Edge_W", _blocks, c + new Vector3(-e, 0.16f, 0), new Vector3(sidewalkWidth, 0.02f, blockSize), path, false, false);

            if (central)
            {
                Prim(PrimitiveType.Cylinder, "FountainBasin", _blocks, c + Vector3.up * 0.55f, new Vector3(14f, 0.4f, 14f), _mats.Get(SidewalkColor * 1.1f), true, true);
                Prim(PrimitiveType.Cylinder, "FountainWater", _blocks, c + Vector3.up * 0.9f, new Vector3(13f, 0.05f, 13f), _mats.Get(WaterColor, 0.9f), false, false);
                Prim(PrimitiveType.Cylinder, "FountainColumn", _blocks, c + Vector3.up * 2.2f, new Vector3(1.2f, 2f, 1.2f), _mats.Get(WallPalette[0]), true, true);
                Prim(PrimitiveType.Sphere, "FountainTop", _blocks, c + Vector3.up * 4.4f, new Vector3(1.6f, 1.3f, 1.6f), _mats.Get(Turquoise, 0.6f), false, true);
            }

            float lim = blockSize * 0.5f - sidewalkWidth - 2f;
            for (float x = -lim; x <= lim; x += 9f)
            for (float z = -lim; z <= lim; z += 9f)
            {
                if (Mathf.Abs(x) < 4f || Mathf.Abs(z) < 4f) continue;
                if (central && new Vector2(x, z).magnitude < 11f) continue;
                Tree(c + new Vector3(x + Range(-1.5f, 1.5f), 0.15f, z + Range(-1.5f, 1.5f)));
            }

            StreetLamps(c);
        }

        // ------------------------------------------------------------------ environment

        void BuildBoundary(float totalX, float totalZ)
        {
            // Ko'rinmas devorlar: o'yinchi prototip hududidan chiqib ketmasin
            float hx = totalX * 0.5f + 1f, hz = totalZ * 0.5f + 1f;
            Wall(new Vector3(0, 10, hz), new Vector3(totalX + 4, 20, 2));
            Wall(new Vector3(0, 10, -hz), new Vector3(totalX + 4, 20, 2));
            Wall(new Vector3(hx, 10, 0), new Vector3(2, 20, totalZ + 4));
            Wall(new Vector3(-hx, 10, 0), new Vector3(2, 20, totalZ + 4));
        }

        void Wall(Vector3 pos, Vector3 size)
        {
            var go = new GameObject("BoundaryWall");
            go.transform.SetParent(_env, false);
            go.transform.localPosition = pos;
            go.AddComponent<BoxCollider>().size = size;
        }

        void BuildMountains(float citySize)
        {
            float radius = citySize * 0.5f + 280f;
            var mat = _mats.Get(MountainColor);
            for (int i = 0; i < 16; i++)
            {
                // Shimoli-sharqda balandroq tog'lar (Chimyon tomonga ishora)
                float angle = i / 16f * Mathf.PI * 2f + Range(-0.1f, 0.1f);
                float northEast = Mathf.Clamp01(Mathf.Cos(angle - Mathf.PI * 0.25f));
                float hgt = Mathf.Lerp(50f, 160f, northEast) * Range(0.8f, 1.2f);
                var pos = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * (radius + Range(0f, 80f));
                Prim(PrimitiveType.Sphere, "Mountain", _env, pos + Vector3.down * 10f,
                    new Vector3(Range(140f, 240f), hgt, Range(140f, 240f)), mat, false, false);
            }
        }

        // ------------------------------------------------------------------ helpers

        Transform Group(string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(transform, false);
            return t;
        }

        GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale,
            Material mat, bool collider, bool castShadows)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;

            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;

            var col = go.GetComponent<Collider>();
            if (!collider)
            {
                DestroyImmediate(col);
            }
            else if (type == PrimitiveType.Cylinder && scale.y * 2f < Mathf.Max(scale.x, scale.z))
            {
                // Yassi silindrda CapsuleCollider katta sferaga aylanadi -> MeshCollider bilan almashtiramiz
                DestroyImmediate(col);
                go.AddComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            }

            go.isStatic = true;
            return go;
        }

        float Rand01() => (float)_rng.NextDouble();
        float Range(float a, float b) => a + (b - a) * (float)_rng.NextDouble();
    }
}
