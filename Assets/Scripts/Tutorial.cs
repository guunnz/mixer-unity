using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class TutorialStep
{
    public List<Action> actions = new List<Action>();

    public string Dialogue;

    public GameObject objectToEnable;
}

public class Tutorial : MonoBehaviour
{
    public List<ScriptableRendererData> rendererDatas;
    public List<TutorialStep> steps;
    public string featureName = "Tutorial";
    public bool enableFeature = true;
    public Material TutorialBW;
    public string vectorPropertyName = "_Position"; // The name of the Vector2 property in the shader
    public string sizePropertyName = "_Size"; // The name of the size property in the shader
    public Canvas MenuCanvas;

    private bool CanAdvance = false;
    private int tutorialStep = 0;
    public RectTransform Olek;
    public GameObject ComboView;
    public TextMeshProUGUI DialogueText;
    public List<CanvasAlignmentController> AlignmentControllerList;
    private void Awake()
    {
        BuildSteps();
    }

    private void BuildSteps()
    {
        steps[0].actions.Add(() => MoveMaterialPosition(new Vector2(-0.52f, -0.08f), 0.5f));
        steps[0].actions.Add(() => ChangeMaterialSize(0.1f, 0.5f));
        steps[3].actions.Add(() => MoveMaterialPosition(new Vector2(0.63f, 0.2f), 0.5f));
        steps[5].actions.Add(() => MoveMaterialPosition(new Vector2(0.52f, 0.21f), 0.5f));
        steps[5].actions.Add(() => ChangeMaterialSize(-0.14f, 0.5f));
        steps[6].actions.Add(() => MoveMaterialPosition(new Vector2(-0.52f, -0.08f), 0.5f));
        steps[8].actions.Add(() => MoveMaterialPosition(new Vector2(-0.29f, 0.21f), 0.5f));
        steps[8].actions.Add(() => ChangeMaterialSize(0.28f, 0.5f));
        steps[8].actions.Add(() => ReverseOlek());
        steps[8].actions.Add(() => ComboView.SetActive(false));

        steps[9].actions.Add(() => MoveMaterialPosition(new Vector2(0.33f, 0.24f), 0.5f));
        steps[8].actions.Add(() => ChangeMaterialSize(-0.02f, 0.5f));
        steps[10].actions.Add(() => MoveMaterialPosition(new Vector2(0.75f, 0.6f), 0.5f));
        steps[12].actions.Add(() => MoveMaterialPosition(new Vector2(-0.57f, 0.62f), 0.5f));
        steps[13].actions.Add(() => MoveMaterialPosition(new Vector2(0.57f, -0.06f), 0.5f));
    }
    private void OnEnable()
    {
        StartTutorial();
    }

    public IEnumerator AdvanceTutorialStep(float DelayToStartText = 0)
    {
        CanAdvance = false;
        steps.ForEach(x =>
        {
            if (x.objectToEnable != null)
                x.objectToEnable.SetActive(false);

        });
        if (steps[tutorialStep].objectToEnable != null)
        {
            steps[tutorialStep].objectToEnable.SetActive(true);
        }

        steps[tutorialStep].actions.ForEach(x => x.Invoke());
        yield return null;
        yield return new WaitForSeconds(DelayToStartText);
        StartCoroutine(TypeText(steps[tutorialStep].Dialogue, DialogueText));
        tutorialStep++;
    }

    // Coroutine to type text into a TextMeshProUGUI component
    public IEnumerator TypeText(string fullText, TextMeshProUGUI textMeshProUGUI)
    {
        textMeshProUGUI.text = ""; // Clear any existing text
        int index = 0;

        while (index < fullText.Length)
        {
            // Check for rich text tags
            if (fullText[index] == '<')
            {
                // Find the end of the tag
                int endOfTag = fullText.IndexOf('>', index);
                if (endOfTag != -1)
                {
                    // Add the entire tag instantly
                    string tag = fullText.Substring(index, endOfTag - index + 1);
                    textMeshProUGUI.text += tag;
                    index = endOfTag + 1;
                    continue;
                }
            }

            SFXManager.instance.PlaySFX(SFXType.Olek, 0.1f);
            // Add the current character and wait for the next one
            textMeshProUGUI.text += fullText[index];
            index++;
            yield return new WaitForSeconds(0.035f);
        }
        CanAdvance = true;
    }


    private void OnDisable()
    {
        SetRendererFeatureEnabled(featureName, false);
        EndTutorial();
    }
    void StartTutorial()
    {
        DOTween.Init();
        AlignmentControllerList.ForEach(x => x.SetAlignment(TextAnchor.MiddleCenter));
        // Sequence to chain animations
        Sequence mySequence = DOTween.Sequence();

        // Animate to left -85
        mySequence.Append(Olek.DOAnchorPosX(-650, .7f).From(new Vector2(-1250, Olek.anchoredPosition.y)));
        mySequence.AppendInterval(0.35f);
        // Optionally, add a callback at the end
        mySequence.OnComplete(() =>
        {
            DialogueText.transform.parent.gameObject.SetActive(true);
            StartCoroutine(AdvanceTutorialStep());
        });

        MenuCanvas.gameObject.AddComponent<PreciseWorldSpaceCanvasController>();
        SetRendererFeatureEnabled(featureName, enableFeature);
    }

    void EndTutorial()
    {
        AlignmentControllerList.ForEach(x => x.SetStretch());
        DialogueText.transform.parent.gameObject.SetActive(false);
        TutorialBW.SetVector("_position", new Vector2(0f, 0f));
        TutorialBW.SetFloat("_size", 1f);

        SetRendererFeatureEnabled(featureName, false);
        Destroy(MenuCanvas.GetComponent<PreciseWorldSpaceCanvasController>());
        MenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        ComboView.SetActive(true);
        DOTween.Init();

        // Sequence to chain animations
        Sequence mySequence = DOTween.Sequence();

        DialogueText.gameObject.SetActive(false);
        Olek.transform.localScale = new Vector3(0.6621594f, 0.6621594f, 0.6621594f);
        // Animate to left 600
        mySequence.Append(Olek.DOAnchorPosX(-1270, .7f));

        // Optionally, add a callback at the end
        mySequence.OnComplete(() =>
        {
            this.gameObject.SetActive(false);
        });
    }

    public void ReverseOlek()
    {
        DOTween.Init();

        // Sequence to chain animations
        Sequence mySequence = DOTween.Sequence();
        DialogueText.transform.parent.gameObject.SetActive(false);
        DialogueText.text = "";

        mySequence.Append(Olek.DOAnchorPosX(635, .7f).From(new Vector2(-650, Olek.anchoredPosition.y)));

        // Optionally, add a callback at the end
        mySequence.OnComplete(() =>
        {
            Olek.transform.localScale = new Vector3(-Olek.transform.localScale.x, Olek.transform.localScale.y, Olek.transform.localScale.z);
            DialogueText.transform.localScale = new Vector3(-DialogueText.transform.localScale.x, DialogueText.transform.localScale.y, DialogueText.transform.localScale.z);
            DialogueText.transform.parent.gameObject.SetActive(true);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CanAdvance && tutorialStep != 0 && tutorialStep < steps.Count)
        {
            if (tutorialStep == 8)
            {
                StartCoroutine(AdvanceTutorialStep(.8f));
            }
            else
            {
                StartCoroutine(AdvanceTutorialStep());

            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0) && tutorialStep > 14 && CanAdvance)
        {
            EndTutorial();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTutorial();
        }
    }

    // Function to smoothly move the Vector2 position value over time
    public void MoveMaterialPosition(Vector2 targetPosition, float duration)
    {
        if (TutorialBW.HasProperty(vectorPropertyName))
        {
            Vector2 currentPos = TutorialBW.GetVector(vectorPropertyName);
            DOTween.To(() => currentPos, x => TutorialBW.SetVector(vectorPropertyName, x), targetPosition, duration)
                   .SetEase(Ease.Linear);
        }
        else
        {
            Debug.LogWarning($"Material does not have a Vector2 property named '{vectorPropertyName}'.");
        }
    }

    // Function to smoothly adjust the size value over time
    public void ChangeMaterialSize(float targetSize, float duration)
    {
        if (TutorialBW.HasProperty(sizePropertyName))
        {
            float currentSize = TutorialBW.GetFloat(sizePropertyName);
            DOTween.To(() => currentSize, x => TutorialBW.SetFloat(sizePropertyName, x), targetSize, duration)
                   .SetEase(Ease.Linear);
        }
        else
        {
            Debug.LogWarning($"Material does not have a float property named '{sizePropertyName}'.");
        }
    }

    public void SetRendererFeatureEnabled(string featureName, bool enable)
    {
        if (rendererDatas != null)
        {
            foreach (var rendererData in rendererDatas)
            {
                // Find the renderer feature by name
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature.name == featureName)
                    {
                        feature.SetActive(enable);
                        Debug.Log($"{featureName} feature is now {(enable ? "enabled" : "disabled")}.");
                        continue;
                    }
                }
            }

            Debug.LogWarning($"Renderer Feature '{featureName}' not found.");
        }
        else
        {
            Debug.LogWarning("Renderer Data is not assigned.");
        }
    }

}
