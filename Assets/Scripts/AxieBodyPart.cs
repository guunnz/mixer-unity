using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum AxieAnimation
{
    activity_appear,
    activity_bath,
    attack_ranged_cast_fly,
    attack_ranged_cast_high,
    attack_ranged_cast_low,
    attack_ranged_cast_multi,
    attack_ranged_cast_tail,
    activity_eat_bite,
    activity_eat_chew,
    activity_entrance,
    defense_evade,
    activity_evolve,
    battle_get_buff,
    battle_get_debuff,
    defense_hit_by_normal,
    defense_hit_by_normal_crit,
    defense_hit_by_normal_dramatic,
    defense_hit_by_ranged_attack,
    defense_hit_with_shield,
    attack_melee_horn_gore,
    attack_melee_mouth_bite,
    action_move_back,
    action_move_forward,
    attack_melee_multi_attack,
    action_idle_normal,
    attack_melee_normal_attack,
    activity_prepare,
    action_idle_random_01,
    action_idle_random_02,
    action_idle_random_03,
    action_idle_random_04,
    action_idle_random_05,
    action_run,
    draft_run_origin,
    attack_melee_shrimp,
    activity_sleep,
    attack_melee_tail_multi_slap,
    attack_melee_tail_roll,
    attack_melee_tail_smash,
    attack_melee_tail_thrash,
    activity_victory_pose_back_flip,
    action_mix_ear_animation,
    action_mix_eyes_animation,
    action_mix_normal_mouth_animation,
    action_mix_body_animation
}

[CreateAssetMenu(fileName = "AxieBodyPart", menuName = "Axie/BodyPart")]
public class AxieBodyPart : ScriptableObject
{
    public SkillName skillName;

    [FormerlySerializedAs("statusEffects")]
    public SkillEffect[] skillEffects;

    public TooltipType[] tooltipTypes = new TooltipType[0];
    public BodyPart bodyPart;
    public GameObject prefab; // Prefab can be assigned in the editor if needed
    public GameObject extraPrefabToInstantiate; // Prefab can be assigned in the editor if needed
    public AxieClass bodyPartClass;
    public float damage;
    public float shield;
    public float energy;
    public string description;
    public bool OnlyDamageShields = false;
    public bool wombo => skillEffects.Any(x => x.Wombo);
    public bool isPassive => energy == 0;
}