using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMeshAssigner : MonoBehaviour
{
    public Mesh[] meshes; // Asigna tus meshes en el inspector

    void Start()
    {
        AssignRandomMesh();
    }

    void AssignRandomMesh()
    {
        // Obtiene el componente MeshFilter del GameObject al que está adjunto este script
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        // Verifica que el MeshFilter no sea nulo y que tengas al menos un mesh en el arreglo
        if (meshFilter != null && meshes.Length > 0)
        {
            // Elige un mesh aleatorio del arreglo de meshes
            Mesh randomMesh = meshes[Random.Range(0, meshes.Length)];

            // Asigna el mesh aleatorio al MeshFilter
            meshFilter.mesh = randomMesh;
        }
        else
        {
            Debug.LogWarning("MeshFilter no encontrado o meshes no asignados.");
        }
    }
}