using finished3;
using System;
using UnityEngine;
using DG.Tweening;

public enum LandType
{
    savannah,
    forest,
    arctic,
    mystic,
    genesis,
    axiepark,
    lunaslanding
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
    public Vector3 lunasColorMin;
    public Vector3 lunasColorMax;
    public OverlayTile tile;
    public FakeOverlayTile tileFake;
    public bool colorAlreadySet = false;
    private bool floorUp;
    public float floorMoveAmount = 0.1f;
    private float startYPosition;

    // Instance of System.Random
    private System.Random random = new System.Random(0); // Default seed

    void Awake()
    {
        // Initialize the renderer and property block
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        this.transform.position = new Vector3(this.transform.position.x,
            this.transform.position.y + RandomRange(-0.1f, 0.1f),
            this.transform.position.z);

        startYPosition = this.transform.position.y;
    }

    // Function to set the random seed
    public void SetRandomSeed(int seed)
    {
        random = new System.Random(seed);
    }

    private float RandomRange(float min, float max)
    {
        return (float)(min + random.NextDouble() * (max - min));
    }

    private void Update()
    {
        if (!colorAlreadySet)
        {
            colorAlreadySet = true;
            switch (landType)
            {
                case LandType.arctic:
                    color = new Color(RandomRange(arcticColorMin.x, arcticColorMax.x),
                        RandomRange(arcticColorMin.y, arcticColorMax.y),
                        RandomRange(arcticColorMin.z, arcticColorMax.z));
                    break;
                case LandType.forest:
                case LandType.axiepark:
                    color = new Color(RandomRange(forestColorMin.x, forestColorMax.x),
                        RandomRange(forestColorMin.y, forestColorMax.y),
                        RandomRange(forestColorMin.z, forestColorMax.z));
                    break;
                case LandType.lunaslanding:
                    color = new Color(RandomRange(lunasColorMin.x, lunasColorMax.x),
                       RandomRange(lunasColorMin.y, lunasColorMax.y),
                       RandomRange(lunasColorMin.z, lunasColorMax.z));
                    break;
                case LandType.genesis:
                    color = new Color(RandomRange(genesisColorMin.x, genesisColorMax.x),
                        RandomRange(genesisColorMin.y, genesisColorMax.y),
                        RandomRange(genesisColorMin.z, genesisColorMax.z));
                    break;
                case LandType.mystic:
                    color = new Color(RandomRange(mysticColorMin.x, mysticColorMax.x),
                        RandomRange(mysticColorMin.y, mysticColorMax.y),
                        RandomRange(mysticColorMin.z, mysticColorMax.z));
                    break;
                case LandType.savannah:
                    color = new Color(RandomRange(savannahColorMin.x, savannahColorMax.x),
                        RandomRange(savannahColorMin.y, savannahColorMax.y),
                        RandomRange(savannahColorMin.z, savannahColorMax.z));
                    break;
            }
        }

        SetTipColor();

        if (tile != null && tile.beingHovered || tileFake != null && tileFake.beingHovered)
        {
            float a = Mathf.Sin(Time.time * 4) * 0.0012f;
            Color colorChange = new Color(a, a, a);
            color += colorChange;
            if (!floorUp)
            {
                floorUp = true;
                transform.DOMoveY(startYPosition + floorMoveAmount, 0.5f);
            }
        }
        else if (floorUp)
        {
            floorUp = false;
            transform.DOMoveY(startYPosition - floorMoveAmount, 0.5f);
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