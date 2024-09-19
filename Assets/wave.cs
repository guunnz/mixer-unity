using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class wave: MonoBehaviour
{
    Mesh mesh;
    Vector3[] originalVertices;
    Vector3[] modifiedVertices;

    public float waveSpeed = 1.0f; // Speed of the wave
    public float waveMagnitude = 0.5f; // Amplitude of the wave
    public float waveFrequency = 1.0f; // Frequency of the wave

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        float time = Time.time * waveSpeed;
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            // Apply a wave along the Y-axis of the cylinder
            float sineWave = Mathf.Sin(time + vertex.y * waveFrequency) * waveMagnitude;
            float cosineWave = Mathf.Cos(time + vertex.y * waveFrequency) * waveMagnitude;
            vertex.x += sineWave;
            vertex.z += cosineWave;
            modifiedVertices[i] = vertex;
        }

        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals(); // Recalculate normals to update lighting
    }
}