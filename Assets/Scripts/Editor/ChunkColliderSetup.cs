using UnityEngine;
using UnityEditor;

public static class ChunkColliderSetup
{
    private const string RoadChildName = "Road";
    private const string StartMarkerName = "startPoint";
    private const string EndMarkerName = "endPoint";

    [MenuItem("Tools/Chunk Setup/Fix Markers on All Chunk Prefabs")]
    private static void FixMarkersOnAllChunks()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Environment" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            using (PrefabUtility.EditPrefabContentsScope scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                FixMarkers(scope.prefabContentsRoot);
            }
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Chunk Marker Fix",
            $"Fixed markers on {count} chunk prefab(s).\nstartPoint and endPoint are now flat at Y=0.", "OK");
    }

    private static void FixMarkers(GameObject root)
    {
        FixMarker(root, StartMarkerName);
        FixMarker(root, EndMarkerName);
    }

    private static void FixMarker(GameObject root, string markerName)
    {
        Transform marker = FindChildRecursive(root.transform, markerName);
        if (marker == null)
        {
            return;
        }

        // Zero out local rotation so the marker has no pitch or roll baked in.
        marker.localRotation = Quaternion.identity;

        // Zero out local X and Y — X centers the marker on the road, Y removes height drift.
        // Only Z is kept so the marker stays at the correct start/end depth of the chunk.
        marker.localPosition = new Vector3(0f, 0f, marker.localPosition.z);
    }

    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    private static readonly string[] BlockingStructureKeywords =
    {
        "building", "wall", "house", "tower", "block", "store", "shop", "facade", "front"
    };

    private static readonly string[] NonBlockingPropKeywords =
    {
        "tree", "trash", "bag", "bench", "lamp", "sign", "car", "bush", "rock", "cone", "prop", "deco"
    };

    [MenuItem("Tools/Chunk Setup/Add Colliders to Selected Chunks")]
    private static void AddCollidersToSelectedChunks()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Chunk Collider Setup", "Select one or more chunk prefab roots in the Hierarchy or Project window first.", "OK");
            return;
        }

        int processedCount = 0;
        foreach (GameObject root in selected)
        {
            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
            bool isPrefabAsset = string.IsNullOrEmpty(assetPath) == false;

            using (PrefabUtility.EditPrefabContentsScope scope = new PrefabUtility.EditPrefabContentsScope(
                isPrefabAsset ? assetPath : PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root)))
            {
                ProcessChunk(scope.prefabContentsRoot);
            }

            processedCount++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Chunk Collider Setup", $"Processed {processedCount} chunk(s). Colliders rebuilt for safe driving.", "OK");
    }

    [MenuItem("Tools/Chunk Setup/Add Colliders to All Chunk Prefabs")]
    private static void AddCollidersToAllChunks()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Environment" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            using (PrefabUtility.EditPrefabContentsScope scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                ProcessChunk(scope.prefabContentsRoot);
            }
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Chunk Collider Setup", $"Processed {count} chunk prefab(s). Colliders rebuilt for safe driving.", "OK");
    }

    private static void ProcessChunk(GameObject root)
    {
        MeshFilter[] allMeshFilters = root.GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter mf in allMeshFilters)
        {
            if (mf.sharedMesh == null)
            {
                continue;
            }

            GameObject go = mf.gameObject;

            // Skip the Start/End marker transforms and any non-geometry objects.
            if (go.name == "Start" || go.name == "End")
            {
                continue;
            }

            if (IsRoadSurface(go))
            {
                EnsureRoadCollider(go, mf.sharedMesh);
            }
            else if (IsBlockingStructure(go))
            {
                EnsureStructureCollider(go, mf.sharedMesh);
            }
            else
            {
                // Decorative props should not create hidden bumps on the road.
                RemoveAllColliders(go);
            }
        }
    }

    private static void EnsureRoadCollider(GameObject go, Mesh mesh)
    {
        MeshCollider existingMesh = go.GetComponent<MeshCollider>();
        if (existingMesh != null)
        {
            Object.DestroyImmediate(existingMesh);
        }

        BoxCollider box = go.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = go.AddComponent<BoxCollider>();
        }

        // Flatten the Y extent so the car sits flush on the road surface.
        Bounds bounds = mesh.bounds;
        box.center = bounds.center;
        box.size = new Vector3(bounds.size.x, Mathf.Max(bounds.size.y, 0.05f), bounds.size.z);
    }

    private static void EnsureStructureCollider(GameObject go, Mesh mesh)
    {
        BoxCollider existingBox = go.GetComponent<BoxCollider>();
        if (existingBox != null)
        {
            Object.DestroyImmediate(existingBox);
        }

        MeshCollider collider = go.GetComponent<MeshCollider>();
        if (collider == null)
        {
            collider = go.AddComponent<MeshCollider>();
        }

        collider.sharedMesh = mesh;
        collider.convex = false;
    }

    private static void RemoveAllColliders(GameObject go)
    {
        Collider[] colliders = go.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    private static bool IsBlockingStructure(GameObject go)
    {
        string lowerName = go.name.ToLowerInvariant();

        for (int i = 0; i < NonBlockingPropKeywords.Length; i++)
        {
            if (lowerName.Contains(NonBlockingPropKeywords[i]))
            {
                return false;
            }
        }

        for (int i = 0; i < BlockingStructureKeywords.Length; i++)
        {
            if (lowerName.Contains(BlockingStructureKeywords[i]))
            {
                return true;
            }
        }

        // Default to non-blocking to avoid hidden collision spikes on unknown meshes.
        return false;
    }

    private static bool IsRoadSurface(GameObject go)
    {
        if (go.name.ToLower().Contains("road") || go.name.ToLower().Contains("floor") || go.name.ToLower().Contains("ground"))
        {
            return true;
        }

        Transform parent = go.transform.parent;
        while (parent != null)
        {
            if (parent.name.ToLower() == RoadChildName.ToLower())
            {
                return true;
            }
            parent = parent.parent;
        }

        return false;
    }
}
