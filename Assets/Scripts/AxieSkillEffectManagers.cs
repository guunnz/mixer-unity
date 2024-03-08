using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxieSkillEffectManager : MonoBehaviour
{
    public List<SkillEffect> statusEffects = new List<SkillEffect>();

    // Define the events
    public event Action<SkillEffect> OnAddStatusEffect;
    public event Action<SkillEffect> OnRemoveStatusEffect;
    public event Action OnRemoveAllEffects;

    private void Start()
    {
        // Subscribe to the events
        OnAddStatusEffect += HandleAddStatusEffect;
        OnRemoveStatusEffect += HandleRemoveStatusEffect;
        OnRemoveAllEffects += HandleRemoveAllEffects;
    }

    public void AddStatusEffect(SkillEffect skillEffect)
    {
        statusEffects.Add(skillEffect);
        OnAddStatusEffect?.Invoke(skillEffect); // Trigger the event
    }

    public void RemoveStatusEffect(SkillEffect skillEffect)
    {
        statusEffects.RemoveAll(x => !x.isPassive);
        OnRemoveStatusEffect?.Invoke(skillEffect); // Trigger the event
    }

    public void RemoveAllEffects()
    {
        statusEffects.Clear();
        OnRemoveAllEffects?.Invoke(); // Trigger the event
    }

    // Event handlers
    private void HandleAddStatusEffect(SkillEffect skillEffect)
    {
    }

    private void HandleRemoveStatusEffect(SkillEffect skillEffect)
    {
    }

    private void HandleRemoveAllEffects()
    {
    }
}