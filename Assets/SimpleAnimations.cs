using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class SimpleAnimations : MonoBehaviour
{
    public float animationSpeed = 1f; // Velocidad de animación
    [ColorUsageAttribute(false, true)]  public Color startColor = Color.white; // Color inicial
    [ColorUsageAttribute(false, true)]  public Color endColor = Color.black; // Color final
    private Material material; // Referencia al material del GameObject

    void Start()
    {
        // Obtener el material del GameObject
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
        else
        {
            Debug.LogError("El GameObject no tiene un Renderer adjunto.");
            enabled = false;
        }
    }

    void Update()
    {
        // Calcular el nuevo color interpolado
        Color newColor = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time * animationSpeed, 1f));

        // Asignar el nuevo color al HDR del material
        if (material != null)
        {
            material.SetColor("_Color", newColor);
        }
    }
}