using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    // When added to an object, draws colored rays from the
    // transform position.
    public int lineCount = 100;
    public float radius = 3.0f;
    static Material lineMaterial;
    public static bool DrawLine = true;

    public List<Vector3> line = new List<Vector3>();
    public Color color;

    private void Awake()
    {
        GameController.ClearPoints += Clear;
    }

    void Clear()
    {
        line.Clear();
    }

    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    // Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        if (!DrawLine) return;
        if (!Application.isPlaying) return;

        lineCount = FindObjectOfType<GameController>().numSteps;
        CreateLineMaterial();
        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform

        // Draw lines
        GL.Begin(GL.LINES);
        for (int i = 0; i < lineCount; ++i)
        {
            GL.Color(color);
            // One vertex at transform position
            if (i >= line.Count)
            {
                GL.End();
                GL.PopMatrix();
                return;
            }
            Vector3 p = line[i];
            GL.Vertex(p);

            // Prevents the line drawn from being dotted
            if (i > 0 && i < lineCount - 1)
                GL.Vertex(p);
        }
        GL.End();
        GL.PopMatrix();
    }
}