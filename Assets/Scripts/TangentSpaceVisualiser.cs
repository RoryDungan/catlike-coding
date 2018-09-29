using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangentSpaceVisualiser : MonoBehaviour
{
    [SerializeField]
    float offset = 0f;

    [SerializeField]
    float scale = 0.1f;

    void OnDrawGizmos()
    {
        var mesh = GetComponent<MeshFilter>()?.sharedMesh;
        if (mesh != null)
        {
            ShowTangentSpace(mesh);
        }
    }

    void ShowTangentSpace(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var tangents = mesh.tangents;
        for (var i = 0; i < vertices.Length; i++)
        {
            DrawTangentGizmo(
                transform.TransformPoint(vertices[i]),
                transform.TransformDirection(normals[i]),
                transform.TransformDirection(tangents[i]),
                tangents[i].w
            );
        }
    }

    void DrawTangentGizmo(Vector3 vertex, Vector3 normal, Vector3 tangent, float binormalSign)
    {
        var vert = vertex + normal * offset;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(vert, vert + normal * scale);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(vert, vert + tangent * scale);
        Gizmos.color = Color.blue;
        var binormal = Vector3.Cross(normal, tangent) * binormalSign;
        Gizmos.DrawLine(vert, vert + binormal * scale);
    }
}
