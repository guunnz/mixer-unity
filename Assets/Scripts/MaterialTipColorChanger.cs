using finished3;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum LandType
{
    savannah,
    forest,
    arctic,
    mystic,
    genesis
}

public class MaterialTipColorChanger : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    private Color color;
    public LandType landType;

    public Vector3 savannahColorMin;
    public Vector3 savannahColorMax;
    public Vector3 forestColorMin;
    public Vector3 forestColorMax;
    public Vector3 arcticColorMin;
    public Vector3 arcticColorMax;
    public Vector3 mysticColorMin;
    public Vector3 mysticColorMax;
    public Vector3 genesisColorMin;
    public Vector3 genesisColorMax;
    public OverlayTile tile;
    public bool colorAlreadySet = false;

    void Awake()
    {
        // Initialize the renderer and property block
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (!colorAlreadySet)
        {
            colorAlreadySet = true;
            switch (landType)
            {
                case LandType.arctic:
                    color = new Color(Random.Range(arcticColorMin.x, arcticColorMax.x),
                        Random.Range(arcticColorMin.y, arcticColorMax.y),
                        Random.Range(arcticColorMin.z, arcticColorMax.z));
                    break;
                case LandType.forest:
                    color = new Color(Random.Range(forestColorMin.x, forestColorMax.x),
                        Random.Range(forestColorMin.y, forestColorMax.y),
                        Random.Range(forestColorMin.z, forestColorMax.z));
                    break;
                case LandType.genesis:
                    color = new Color(Random.Range(genesisColorMin.x, genesisColorMax.x),
                        Random.Range(genesisColorMin.y, genesisColorMax.y),
                        Random.Range(genesisColorMin.z, genesisColorMax.z));
                    break;
                case LandType.mystic:
                    color = new Color(Random.Range(mysticColorMin.x, mysticColorMax.x),
                        Random.Range(mysticColorMin.y, mysticColorMax.y),
                        Random.Range(mysticColorMin.z, mysticColorMax.z));
                    break;
                case LandType.savannah:
                    color = new Color(Random.Range(savannahColorMin.x, savannahColorMax.x),
                        Random.Range(savannahColorMin.y, savannahColorMax.y),
                        Random.Range(savannahColorMin.z, savannahColorMax.z));
                    break;
            }
        }

        SetTipColor();

        if (tile.beingHovered)
        {
            float a = Mathf.Sin(Time.time*4) * 0.0012f;
            Debug.Log(a);
            Color colorChange = new Color(a, a, a);
            color += colorChange;
        }
    }

    // Call this function to change the "_TipColor" property
    public void SetTipColor()
    {
        // Get the current properties from the renderer
        _renderer.GetPropertyBlock(_propBlock);

        // Set the color on the property block
        _propBlock.SetColor("_TipColor", color);

        // Apply the modified property block back to the renderer
        _renderer.SetPropertyBlock(_propBlock);
    }
}