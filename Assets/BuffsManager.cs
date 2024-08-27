using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;

public class BuffsManager : MonoBehaviour
{
    static public BuffsManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void DoUpgrade(AtiaBlessing.BuffEffect effect, Team team)
    {
        // Get the method name from the effect
        string methodName = effect.ToString();

        // Call the method by name and pass the 'team' parameter
        MethodInfo methodInfo = GetType().GetMethod(methodName);
        if (methodInfo != null)
        {
            methodInfo.Invoke(this, new object[] { team });
        }
        else
        {
            Debug.LogError("Method not found: " + methodName);
        }
    }

    public void Backdoor(Team team)
    {
        var axies = team.GetCharactersAll();
        axies.ForEach(x => x.ShrimpOnStart = true);
    }

    public void Topaz(Team team)
    {
        if (!team.isGoodTeam)
            return;

        RunManagerSingleton.instance.economyPassive.ExtraCoinsPerRound += 1;
    }

    public void ThreeWondersShoes(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie =>
        {
            axie.stats.skill += 1;
            axie.stats.hp += 1;
            axie.stats.morale += 1;
            axie.stats.speed += 1;
        });
    }

    public void KingsBoots(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie =>
        {
            axie.stats.skill += 3;
            axie.stats.hp += 3;
            axie.stats.morale += 3;
            axie.stats.speed += 3;
        });
    }

    public void SteelShield(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie =>
        {
            axie.axieSkillController.passives.RangedReflectDamageAmount += 10;
            axie.axieSkillController.passives.MeleeReflectDamageAmount += 10;
        });
    }

    public void SteelHelmet(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.axieSkillController.passives.ExtraArmorHelmet += 100; });
    }

    public void IronHelmet(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.axieSkillController.passives.ExtraArmorHelmet += 50; });
    }

    public void HoneyRoastedChicken(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.axieSkillController.passives.MeleeReflectDamageAmount += 10; });
    }

    public void IronChainmail(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.axieSkillController.passives.MeleeReflectDamageAmount += 10; });
    }

    public void SteelChainmail(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.axieSkillController.passives.MeleeReflectDamageAmount += 20; });
    }

    public void SteelSword(Team team)
    {

        Action action = () =>
        {
            var axies = team.GetCharactersAll();
            foreach (var axieController in axies)
            {
                axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.AttackPositive, Attack = 1, skillDuration = 99999 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void SteelHammer(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.morale += 3; });
    }

    public void SteelBoots(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.speed += 3; });
    }

    public void SteelArrow(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.axieSkillController.passives.rangedAlwaysCritical = true; });
    }

    public void SteakNFries(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.hp += 1; });
    }

    public void SilverTopazBracelet(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Beast || x.axieIngameStats.axieClass == AxieClass.Mech ||
            x.axieIngameStats.axieClass == AxieClass.Bug).ToList();

        axies.ForEach(axie => { axie.stats.skill += 3; });
    }

    public void SilverRubyBracelet(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Aquatic || x.axieIngameStats.axieClass == AxieClass.Bird ||
            x.axieIngameStats.axieClass == AxieClass.Dawn).ToList();

        axies.ForEach(axie => { axie.stats.skill += 3; });
    }

    public void SilverEmeraldBracelet(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Plant || x.axieIngameStats.axieClass == AxieClass.Dusk ||
            x.axieIngameStats.axieClass == AxieClass.Reptile).ToList();

        axies.ForEach(axie => { axie.stats.skill += 3; });
    }

    public void SilverDiamondBracelet(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.skill += 5; });
    }

    public void SilverAmethystBracelet(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.skill += 2; });
    }

    public void Ramen(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.skill += 1; });
    }

    public void PlatinumLantern(Team team)
    {
        var axies = team.GetCharactersAll();

        int maxSkill = axies.Max(x => x.stats.skill);
        axies.ForEach(axie => { axie.stats.skill = maxSkill; });
    }

    public void GoldenSphere(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie =>
        {
            int skillHalved = Mathf.RoundToInt(axie.stats.skill / 2f);
            axie.stats.skill = skillHalved;

            int pointsToAddToStats = Mathf.RoundToInt(skillHalved / 3f);

            axie.stats.hp += pointsToAddToStats;
            axie.stats.speed += pointsToAddToStats;
            axie.stats.morale += pointsToAddToStats;
        });
    }

    public void HoneyCarrotSoup(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 10; });
    }

    public void GrilledLambChop(Team team)
    {
        Action action = () => { StartCoroutine(GrilledLambChopAfterSeconds(team: team)); };

        team.OnBattleStartActions.Add(action);
    }

    public IEnumerator GrilledLambChopAfterSeconds(Team team)
    {
        var axies = team.GetCharacters();
        yield return new WaitForSeconds(15);

        foreach (var axieController in axies)
        {
            axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.AttackPositive, Attack = 3 });
        }
    }

    public void PumpkinPie(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.hp += 2; });
    }

    public void ShellOfStun(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll();
            foreach (var axieController in axies)
            {
                axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.Stun, Stun = true, skillDuration = 2 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfSilence(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll();
            foreach (var axieController in axies)
            {
                axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.Jinx });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfShock(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll();
            foreach (var axieController in axies)
            {
                axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.None, ApplyRandomEffect = true, skillDuration = 1 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfMediumConcentratedDamage(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll().OrderByDescending(x => x.axieIngameStats.HP);
            axies.First().axieIngameStats.currentHP *= 0.5f;
        };

        team.OnBattleStartActions.Add(action);
    }

    public void MediumPotionOfShield(Team team)
    {

        Action action = () =>
        {
            var axies = team.GetCharactersAll();
            if (RunManagerSingleton.instance.landType == LandType.lunalanding)
            {
                axies.ForEach(x => x.axieIngameStats.currentShield += 100f);
            }
            else
            {

                axies.ForEach(x => x.axieIngameStats.currentShield += 50f);
            }

        };

        team.OnBattleStartActions.Add(action);
    }

    public void MediumHastePotion(Team team)
    {

        Action action = () =>
        {

            var axies = team.GetCharactersAll();
            if (RunManagerSingleton.instance.landType == LandType.lunalanding)
            {
                axies.ForEach(x => x.stats.speed += 6);
            }
            else
            {
                axies.ForEach(x => x.stats.speed += 3);

            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargeHastePotion(Team team)
    {

        Action action = () =>
        {

            var axies = team.GetCharactersAll();
            if (RunManagerSingleton.instance.landType == LandType.lunalanding)
            {
                axies.ForEach(x => x.stats.speed += 12);
            }
            else
            {

                axies.ForEach(x => x.stats.speed += 6);
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargeEnergyPotion(Team team)
    {

        Action action = () =>
        {

            var axies = team.GetCharactersAll();
            if (RunManagerSingleton.instance.landType == LandType.lunalanding)
            {
                axies.ForEach(x => x.stats.skill += 12);
            }
            else
            {

                axies.ForEach(x => x.stats.skill += 6);
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargePotionOfShield(Team team)
    {

        Action action = () =>
        {

            var axies = team.GetCharactersAll();
            if (RunManagerSingleton.instance.landType == LandType.lunalanding)
            {
                axies.ForEach(x => x.axieIngameStats.currentShield += 200f);
            }
            else
            {
                axies.ForEach(x => x.axieIngameStats.currentShield += 100f);
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargePotionOfResistance(Team team)
    {
        var axies = team.GetCharactersAll();
        if (RunManagerSingleton.instance.landType == LandType.lunalanding)
        {
            axies.ForEach(x => x.axieSkillController.passives.DamageReductionAmount += 30);
        }
        else
        {

            axies.ForEach(x => x.axieSkillController.passives.DamageReductionAmount += 15);
        }
    }


    public void SteelArmor(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(x => x.axieSkillController.passives.MeleeReflectDamageAmount += 20);
    }

    public void MeatPie(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(x => x.stats.hp += 3);
    }

    public void ShellOfMediumAreaDamage(Team team)
    {


        Action action = () =>
        {

            var axies = team.enemyTeam.GetCharactersAll();
            axies.ForEach(x => x.axieIngameStats.currentHP *= .9f);
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfDistraction(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll().OrderByDescending(x => x.axieIngameStats.HP);

            axies.First().axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.Stench, Stench = true });
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfBurn(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll();
            axies.ForEach(x => x.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.Fear }));
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfBlinding(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll();
            axies.ForEach(x => x.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.Poison, Poison = true, PoisonStack = 1 }));
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfSlow(Team team)
    {

        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll();
            foreach (var axieController in axies)
            {
                axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.SpeedNegative });
            }
        };

        team.OnBattleStartActions.Add(action);
    }



    public void MoonSphere(Team team)
    {

        Action action = () =>
        {
            var axies = team.GetCharactersAll();
            axies.ForEach(x => x.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.None, RandomEffectIsBuff = true, skillDuration = 1 }));
        };

        team.OnBattleStartActions.Add(action);
    }

    public void RubyBook(Team team)
    {

        Action action = () =>
        {
            var axies = team.GetCharactersAll();
            axies.ForEach(x => x.axieIngameStats.CurrentEnergy += x.axieIngameStats.MaxEnergy * .3f);
        };


        team.OnBattleStartActions.Add(action);
    }

    public void GoldTopazNecklace(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Beast || x.axieIngameStats.axieClass == AxieClass.Mech ||
            x.axieIngameStats.axieClass == AxieClass.Bug).ToList();

        axies.ForEach(axie => { axie.stats.morale += 3; });
    }

    public void GoldRubyNecklace(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Aquatic || x.axieIngameStats.axieClass == AxieClass.Bird ||
            x.axieIngameStats.axieClass == AxieClass.Dawn).ToList();

        axies.ForEach(axie => { axie.stats.morale += 3; });
    }

    public void FruitNVegetablesSalad(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Plant).ToList();

        axies.ForEach(axie => { axie.stats.hp *= 3; });
    }

    public void DeluxeRamen(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Bird).ToList();

        axies.ForEach(axie => { axie.stats.speed *= 2; });
    }

    public void DeluxeFriedRice(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Beast).ToList();

        axies.ForEach(axie => { axie.stats.morale *= 2; });
    }

    public void Amethyst(Team team)
    {
        RunManagerSingleton.instance.economyPassive.FreeRerollEveryXRolls = 3;
    }


    public void CompositeBow(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.Range += 2; axie.axieIngameStats.Range += 2; });
    }

    public void GoldEmeraldNecklace(Team team)
    {
        var axies = team.GetCharactersAll().Where(x =>
            x.axieIngameStats.axieClass == AxieClass.Plant || x.axieIngameStats.axieClass == AxieClass.Dusk ||
            x.axieIngameStats.axieClass == AxieClass.Reptile).ToList();

        axies.ForEach(axie => { axie.stats.morale += 3; });
    }

    public void GoldDiamondNecklace(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.morale += 5; });
    }

    public void GoldAmethystNecklace(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie => { axie.stats.morale += 2; });
    }

    public void Increase_HP_Bug(Team team)
    {
        var axies = team.GetCharactersAll();

        if (RunManagerSingleton.instance.landType == LandType.lunalanding)
        {

            axies.ForEach(axie => { axie.stats.hp += 2; });
        }
        else
        {

            axies.ForEach(axie => { axie.stats.hp += 1; });
        }
    }

    public void Increase_HP(Team team)
    {
        var axies = team.GetCharactersAll();
        if (RunManagerSingleton.instance.landType == LandType.lunalanding)
        {

            axies.ForEach(axie => { axie.stats.hp += 2; });
        }
        else
        {
            axies.ForEach(axie => { axie.stats.hp += 1; });
        }
    }

    public void Increase_Morale(Team team)
    {
        var axies = team.GetCharactersAll();
        if (RunManagerSingleton.instance.landType == LandType.lunalanding)
        {

            axies.ForEach(axie => { axie.stats.morale += 2; });
        }
        else
        {
            axies.ForEach(axie => { axie.stats.morale += 1; });
        }
    }

    public void Increase_Skill(Team team)
    {
        var axies = team.GetCharactersAll();
        if (RunManagerSingleton.instance.landType == LandType.lunalanding)
        {

            axies.ForEach(axie => { axie.stats.skill += 2; });
        }
        else
        {
            axies.ForEach(axie => { axie.stats.skill += 1; });
        }

    }

    public void Increase_Speed(Team team)
    {
        var axies = team.GetCharactersAll();
        if (RunManagerSingleton.instance.landType == LandType.lunalanding)
        {

            axies.ForEach(axie => { axie.stats.speed += 2; });
        }
        else
        {
            axies.ForEach(axie => { axie.stats.speed += 1; });
        }

    }

    public void Donut(Team team)
    {
        RunManagerSingleton.instance.roundsPassives.ExtraTeamHPPerRound += 2;
    }


    public void GoldenBook(Team team)
    {

        Action action = () =>
        {
            var axies = team.GetCharactersAll();
            axies.ForEach(x => x.axieIngameStats.CurrentEnergy += x.axieIngameStats.MaxEnergy * .5f);
        };


        team.OnBattleStartActions.Add(action);
    }

    public void Ruby(Team team)
    {
        RunManagerSingleton.instance.economyPassive.RollsFreePerRound += 1;
    }

    public void Diamond(Team team)
    {
        RunManagerSingleton.instance.economyPassive.ItemCostPercentage = 50;
    }

    public void Emerald(Team team)
    {
        RunManagerSingleton.instance.economyPassive.CoinsOnStart = 15;
        RunManagerSingleton.instance.economyPassive.RollCost = 2;
    }

    public void RosyHarp(Team team)
    {
        var axies = team.GetCharactersAll();

        foreach (var axieController in axies.Where(x => x.Range < 2))
        {
            axieController.Range += 1;
        }
    }


    /// <summary>
    /// Auguments
    /// </summary>
    /// <param name="team"></param>

    public void GainHPAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainHPReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void GainSpeedAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSpeedReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void GainSkillAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainSkillReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void GainMoraleAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoraleBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoraleDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoraleBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoraleBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoraleMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoralePlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoraleDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void GainMoraleReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void MoraleBuffAfterSecondsAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void MoraleBuffAfterSecondsReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void SpeedBuffAfterSecondsAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void SpeedBuffAfterSecondsReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void AttackBuffAfterSecondsAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AttackBuffAfterSecondsReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void AutoAttackHealReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void ExtraAbilitySlotAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void ExtraAbilitySlotReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void BackdoorAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void BackdoorReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

    public void PrismaticAqua(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticBird(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticDawn(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticBeast(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticBug(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticMech(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticPlant(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticDusk(Team team)
    {
        var axies = team.GetCharactersAll();
    }
    public void PrismaticReptile(Team team)
    {
        var axies = team.GetCharactersAll();
    }

}