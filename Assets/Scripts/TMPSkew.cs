using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_SkewText : MonoBehaviour
{
    [Range(-1f, 1f)] public float skewFactor = 0.2f;

    private TextMeshProUGUI tmp;
    private string lastText;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        if (tmp == null)
            tmp = GetComponent<TextMeshProUGUI>();

        if (tmp.text != lastText)
        {
            ApplySkew();
            lastText = tmp.text;
        }
    }

    void ApplySkew()
    {
        tmp.ForceMeshUpdate();
        var textInfo = tmp.textInfo;
        var mesh = tmp.mesh;
        var vertices = mesh.vertices;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;

            for (int j = 0; j < 4; j++)
            {
                float y = vertices[vertexIndex + j].y;
                vertices[vertexIndex + j].x += skewFactor * y;
            }
        }

        mesh.vertices = vertices;
        tmp.canvasRenderer.SetMesh(mesh);
    }
}
