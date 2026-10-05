using UnityEngine;

[ExecuteAlways]
public class WireframeCube : MonoBehaviour
{
    [Header("Wireframe Settings")]
    [Min(0.001f)]
    public float thickness = 0.05f;

    public Material material;

    private Transform wireframeParent;

    private void OnEnable()
    {
        BuildWireframe();
    }

    private void OnValidate()
    {
        BuildWireframe();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateWireframe();
        }
    }

    private void BuildWireframe()
    {
        MeshRenderer originalRenderer = GetComponent<MeshRenderer>();

        Transform existing = transform.Find("Wireframe");

        if (existing != null)
            wireframeParent = existing;
        else
        {
            GameObject parent = new GameObject("Wireframe");
            parent.transform.SetParent(transform);
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localRotation = Quaternion.identity;
            parent.transform.localScale = Vector3.one;

            wireframeParent = parent.transform;
        }

        while (wireframeParent.childCount < 12)
        {
            GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);

            edge.name = "Edge_" + wireframeParent.childCount;

            edge.transform.SetParent(wireframeParent);

            Collider col = edge.GetComponent<Collider>();

            if (col != null)
            {
                if (Application.isPlaying)
                    Destroy(col);
                else
                    DestroyImmediate(col);
            }
        }

        UpdateWireframe();
    }

    private void UpdateWireframe()
    {
        if (wireframeParent == null)
            return;

        Vector3 scale = transform.lossyScale;

        float xThickness = thickness / Mathf.Max(Mathf.Abs(scale.x), 0.0001f);
        float yThickness = thickness / Mathf.Max(Mathf.Abs(scale.y), 0.0001f);
        float zThickness = thickness / Mathf.Max(Mathf.Abs(scale.z), 0.0001f);

        int index = 0;

        // X edges
        for (int y = -1; y <= 1; y += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                SetEdge(
                    index++,
                    new Vector3(0, y * 0.5f, z * 0.5f),
                    new Vector3(1f, yThickness, zThickness)
                );
            }
        }

        // Y edges
        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                SetEdge(
                    index++,
                    new Vector3(x * 0.5f, 0, z * 0.5f),
                    new Vector3(xThickness, 1f, zThickness)
                );
            }
        }

        // Z edges
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                SetEdge(
                    index++,
                    new Vector3(x * 0.5f, y * 0.5f, 0),
                    new Vector3(xThickness, yThickness, 1f)
                );
            }
        }
    }

    private void SetEdge(int index, Vector3 position, Vector3 scale)
    {
        if (index >= wireframeParent.childCount)
            return;

        Transform edge = wireframeParent.GetChild(index);

        edge.localPosition = position;
        edge.localRotation = Quaternion.identity;
        edge.localScale = scale;

        MeshRenderer renderer = edge.GetComponent<MeshRenderer>();

        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
    }
}