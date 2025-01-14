#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples
{

#if NEW_PREFAB_SYSTEM
    [ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class OutlineController : MonoBehaviour
    {
        public MeshRenderer referenceRenderer;
        public bool IsFocusing { get; set; } = false;
        bool updateViaSkeletonCallback = false;
        MeshFilter referenceMeshFilter;
        MeshRenderer ownRenderer;
        MeshFilter ownMeshFilter;

        [System.Serializable]
        public struct MaterialReplacement
        {
            public Material originalMaterial;
            public Material replacementMaterial;
        }
        public MaterialReplacement[] replacementMaterials = new MaterialReplacement[0];

        private Dictionary<Material, Material> replacementMaterialDict = new Dictionary<Material, Material>();
        private Material[] sharedMaterials = new Material[0];

#if UNITY_EDITOR
        private void Reset()
        {
            if (referenceRenderer == null)
            {
                referenceRenderer = this.transform.parent.GetComponentInParent<MeshRenderer>();
                if (!referenceRenderer)
                    return;
            }

            Material[] parentMaterials = referenceRenderer.sharedMaterials;
            if (replacementMaterials.Length != parentMaterials.Length)
            {
                replacementMaterials = new MaterialReplacement[parentMaterials.Length];
            }
            for (int i = 0; i < parentMaterials.Length; ++i)
            {
                replacementMaterials[i].originalMaterial = parentMaterials[i];
                replacementMaterials[i].replacementMaterial = parentMaterials[i];
            }
            Awake();
            LateUpdate();
        }
#endif

        void Awake()
        {
            if (referenceRenderer == null)
            {
                referenceRenderer = this.transform.parent.GetComponentInParent<MeshRenderer>();
            }

            // subscribe to OnMeshAndMaterialsUpdated
            SkeletonAnimation skeletonRenderer = referenceRenderer.GetComponent<SkeletonAnimation>();
            if (skeletonRenderer)
            {
                skeletonRenderer.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
                skeletonRenderer.OnMeshAndMaterialsUpdated += UpdateOnCallback;
                updateViaSkeletonCallback = true;
            }
            referenceMeshFilter = referenceRenderer.GetComponent<MeshFilter>();
            ownRenderer = this.GetComponent<MeshRenderer>();
            ownMeshFilter = this.GetComponent<MeshFilter>();

            InitializeDict();
        }

#if UNITY_EDITOR
        // handle disabled scene reload
        private void OnEnable()
        {
            if (Application.isPlaying)
                Awake();
        }

        private void Update()
        {
            if (!Application.isPlaying)
                InitializeDict();
        }
#endif

        void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateMaterials();
                return;
            }
#endif

            if (IsFocusing == false)
                return;
            if (updateViaSkeletonCallback)
                return;
            UpdateMaterials();
        }

        void UpdateOnCallback(SkeletonRenderer r)
        {
            UpdateMaterials();
        }

        void UpdateMaterials()
        {
            ownMeshFilter.sharedMesh = referenceMeshFilter.sharedMesh;

            Material[] parentMaterials = referenceRenderer.sharedMaterials;
            if (sharedMaterials.Length != parentMaterials.Length)
            {
                sharedMaterials = new Material[parentMaterials.Length];
            }
            for (int i = 0; i < parentMaterials.Length; ++i)
            {
                Material parentMaterial = parentMaterials[i];
                if (replacementMaterialDict.ContainsKey(parentMaterial))
                {
                    sharedMaterials[i] = replacementMaterialDict[parentMaterial];
                }
            }
            ownRenderer.sharedMaterials = sharedMaterials;
        }

        void InitializeDict()
        {
            replacementMaterialDict.Clear();
            for (int i = 0; i < replacementMaterials.Length; ++i)
            {
                MaterialReplacement entry = replacementMaterials[i];
                replacementMaterialDict[entry.originalMaterial] = entry.replacementMaterial;
            }
        }

        public void SetActive(bool isON, Color color)
        {
            if (isON)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
                return;
            }

            // Iterate over all replacement materials and set the OutlineColor
            foreach (var entry in replacementMaterials)
            {
                if (entry.replacementMaterial != null)
                {
                    // Check if the material has the OutlineColor property
                    if (entry.replacementMaterial.HasProperty("_OutlineColor"))
                    {
                        entry.replacementMaterial.SetColor("_OutlineColor", color);
                    }
                }
            }

            // Update the materials on the renderer
            UpdateMaterials();
        }

        public void Clear()
        {
            gameObject.SetActive(false);
        }
    }
}
