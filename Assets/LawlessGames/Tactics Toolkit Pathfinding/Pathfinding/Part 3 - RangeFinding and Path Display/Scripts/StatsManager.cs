using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Spine.Unity;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class StatsManager : MonoBehaviour
{
    public Rectangle HPRectangle;
    public SpriteRenderer sr;
    private int lastXAxisScale;
    private Transform mainCharacter;
    public TextMeshProUGUI shield;
    public GameObject shieldObject;
    public SpriteRenderer Selected;
    BoneFollower boneFollower;
    private int ManaBars = 1;
    public Rectangle[] ManaBarsList;
    private float ManaBarSpacing = 0.05f;
    private float TotalEnergyBarSize = 1.2709f;
    public Transform AttackAnim;
    private float ManaPerBar = 0.5f;
    internal float shieldValue;
    public List<TextMeshProUGUI> bonusTextsCritical;
    public List<TextMeshProUGUI> bonusTextsReflect;
    public GameObject damagePrefab;
    public Transform DamageTransform;
    private List<GameObject> spawnedDamages = new List<GameObject>();
    public List<RectTransform> rectTransformSpawnPoints;
    private int poolSize = 20; // Set an initial pool size
    private float CurrentMana;

    private void Start()
    {
        boneFollower = this.gameObject.AddComponent<BoneFollower>();

        if (boneFollower.skeletonRenderer == null)
            boneFollower.skeletonRenderer = transform.parent.GetComponent<SkeletonRenderer>();

        boneFollower.boneName = "@body";
        boneFollower.followBoneRotation = false;
        boneFollower.followXYPosition = true;
        boneFollower.followZPosition = false;
        boneFollower.followSkeletonFlip = false;
        boneFollower.yOffset = 3;

        mainCharacter = transform.parent.parent;
        int scaleWished = mainCharacter.transform.localScale.x < 0 ? -3 : 3;

        this.transform.localScale =
            new Vector3(scaleWished, this.transform.localScale.y, this.transform.localScale.z);
        lastXAxisScale = scaleWished;

        // Initialize object pool
        InitializeDamagePool();
    }

    private void InitializeDamagePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(damagePrefab, DamageTransform);
            obj.SetActive(false);
            spawnedDamages.Add(obj);
        }
    }

    public void SpawnDamage(string damage, bool ability)
    {
        StartCoroutine(SpawnDamageCoroutine(damage, ability));
    }

    IEnumerator SpawnDamageCoroutine(string damage, bool ability)
    {
        GameObject obj = GetPooledObject();

        if (obj != null)
        {
            var tmpro = obj.GetComponent<TextMeshProUGUI>();
            var rect = obj.GetComponent<RectTransform>();
            var spawnPoint = rectTransformSpawnPoints[UnityEngine.Random.Range(0, rectTransformSpawnPoints.Count)];
            rect.localPosition = spawnPoint.localPosition;
            rect.anchoredPosition = spawnPoint.anchoredPosition;
            rect.DOLocalMoveX(rect.transform.localPosition.x + (UnityEngine.Random.Range(-1f, 1f)), 0.001f);
            tmpro.text = damage.ToString();
            tmpro.color = ability ? Color.blue : Color.red;

            rect.DOAnchorPosY(rect.anchoredPosition.y + 3f, 1f);

            obj.SetActive(true); // Activate the object

            yield return new WaitForSeconds(.7f);
            tmpro.DOColor(Color.clear, 0.3f);
            yield return new WaitForSeconds(.3f);
            obj.SetActive(false); // Deactivate it after a short time
        }
    }

    private GameObject GetPooledObject()
    {
        // Find the first inactive object in the pool
        foreach (var obj in spawnedDamages)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // If no inactive object is available, increase the pool size
        return ExpandDamagePool();
    }

    private GameObject ExpandDamagePool()
    {
        var obj = Instantiate(damagePrefab, DamageTransform);
        obj.SetActive(false);
        spawnedDamages.Add(obj);
        poolSize++; // Increase the pool size

        return obj;
    }

    public void SetCritical()
    {
        var textChosen = bonusTextsCritical[UnityEngine.Random.Range(0, bonusTextsCritical.Count)];

        textChosen.enabled = true;
        if (UnityEngine.Random.value > 0.5f)
        {
            textChosen.text = "CRIT!";
        }
        else
        {
            textChosen.text = "CRITICAL!";
        }

        Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        textChosen.color = randomColor;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(textChosen.DOFade(1f, 0.1f).SetEase(Ease.Linear));
        sequence.Join(textChosen.DOColor(randomColor, .2f).SetEase(Ease.Linear));
        sequence.Append(textChosen.DOFade(0f, .2f).SetEase(Ease.Linear));
        sequence.AppendCallback(() => textChosen.enabled = false);
    }


    public void SetManaBars(int manaBars)
    {
        ManaBars = manaBars;

        ManaBarsList.ToList().ForEach(x => x.gameObject.SetActive(false));
        ManaPerBar = 1f / manaBars;
        for (int i = 0; i < manaBars; i++)
        {
            ManaBarsList[i].gameObject.SetActive(true);
            var manaBar = ManaBarsList[i].transform;

            if (i != 0)
            {
                manaBar.localScale = -new Vector3(TotalEnergyBarSize / manaBars - ManaBarSpacing, -0.5f, -1);
                manaBar.localPosition = new Vector3(0.4739688f - (ManaBarSpacing) - ((TotalEnergyBarSize / manaBars) * i), manaBar.localPosition.y, manaBar.localPosition.z);
            }
            else
            {
                manaBar.localScale = -new Vector3(TotalEnergyBarSize / manaBars, -0.5f, -1);
            }
        }
    }

    public void SetSR(Sprite sprite)
    {
        sr.sprite = sprite;
    }

    public void SetMana(float mana)
    {
        try
        {
            if (mana > 1)
            {
                mana = 1;
            }

            if (mana < CurrentMana)
            {
                CurrentMana = mana;

                for (int i = 0; i < ManaBars; i++)
                {
                    float width = mana / ManaPerBar;

                    if (width >= 1)
                    {
                        width = 1;
                    }
                    StartCoroutine(ChangeWidthCoroutine(ManaBarsList[i], width));
                    mana -= ManaPerBar;
                }
            }
            else
            {
                CurrentMana = mana;

                for (int i = 0; i < ManaBars; i++)
                {
                    float width = mana / ManaPerBar;

                    if (width >= 1)
                    {
                        width = 1;
                    }
                    ManaBarsList[i].Width = width;
                    mana -= ManaPerBar;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    IEnumerator ChangeWidthCoroutine(Rectangle rectangle, float targetWidth)
    {
        float initialWidth = rectangle.Width;
        float duration = 0.2f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectangle.Width = Mathf.Lerp(initialWidth, targetWidth, elapsed / duration);
            yield return null;
        }

        rectangle.Width = targetWidth;
    }

    public void SetHP(float mana)
    {
        if (mana > 1)
        {
            mana = 1;
        }

        HPRectangle.Width = mana;
    }

    public void SetShield(int shield)
    {
        if (shield <= 0)
        {
            shieldObject.SetActive(false);
        }
        else
        {
            shieldObject.SetActive(true);
        }
        shieldValue = shield;

        this.shield.text = shield.ToString();
    }

    private void Update()
    {
        int scaleWished = mainCharacter.transform.localScale.x < 0 ? 3 : -3;
        if (lastXAxisScale != scaleWished)
        {
            this.transform.localScale =
                new Vector3(scaleWished, this.transform.localScale.y, this.transform.localScale.z);
            lastXAxisScale = scaleWished;
        }
    }
}
