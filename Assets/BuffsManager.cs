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

    public void IronShield(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(axie =>
        {
            axie.axieSkillController.passives.RangedReflectDamageAmount += 5;
            axie.axieSkillController.passives.MeleeReflectDamageAmount += 5;
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

        int maxHp = axies.Max(x => x.stats.hp);
        axies.ForEach(axie => { axie.stats.hp = maxHp; });
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

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 3; });
    }

    public void GrilledLambChop(Team team)
    {
        Action action = () => { StartCoroutine(GrilledLambChopAfterSeconds(team: team)); };

        team.OnBattleStartActions.Add(action);
    }
    



    public IEnumerator GrilledLambChopAfterSeconds(Team team)
    {
        var axies = team.GetAliveCharacters();
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
            var axies = team.enemyTeam.GetCharactersAll().OrderByDescending(x => x.axieIngameStats.maxHP);
            axies.First().axieIngameStats.currentHP *= 0.5f;
        };

        team.OnBattleStartActions.Add(action);
    }

    public void MediumPotionOfShield(Team team)
    {

        Action action = () =>
        {
            var axies = team.GetCharactersAll();
            if (team.landType == LandType.lunalanding)
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
            if (team.landType == LandType.lunalanding)
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
            if (team.landType == LandType.lunalanding)
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
            if (team.landType == LandType.lunalanding)
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
            if (team.landType == LandType.lunalanding)
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
        if (team.landType == LandType.lunalanding)
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
    public void IronArmor(Team team)
    {
        var axies = team.GetCharactersAll();

        axies.ForEach(x => x.axieSkillController.passives.RangedReflectDamageAmount += 10);
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
            var axies = team.enemyTeam.GetCharactersAll().OrderByDescending(x => x.axieIngameStats.maxHP);

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
            { statusEffect = StatusEffectEnum.Poison, Poison = true, PoisonStack = 1 }));
        };

        team.OnBattleStartActions.Add(action);

    }

    public void ShellOfBlinding(Team team)
    {
        Action action = () =>
        {
            var axies = team.enemyTeam.GetCharactersAll();
            axies.ForEach(x => x.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.Fear }));
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
        if (team.isGoodTeam)
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

        if (team.landType == LandType.lunalanding)
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
        if (team.landType == LandType.lunalanding)
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
        if (team.landType == LandType.lunalanding)
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
        if (team.landType == LandType.lunalanding)
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
        if (team.landType == LandType.lunalanding)
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
        if (team.isGoodTeam)
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
        if (!team.isGoodTeam)
            return;
        RunManagerSingleton.instance.economyPassive.RollsFreePerRound += 1;
    }

    public void Diamond(Team team)
    {
        if (!team.isGoodTeam)
            return;
        RunManagerSingleton.instance.economyPassive.ItemCostPercentage = 50;
    }

    public void Emerald(Team team)
    {
        if (!team.isGoodTeam)
            return;
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
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPBug(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bug);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPPlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }
    public void GainHPReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.hp * 1.1f); });
    }

    public void GainSpeedAqua(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedBug(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bug);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedPlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }
    public void GainSpeedReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.speed * 1.1f); });
    }

    public void GainSkillAqua(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillBug(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bug);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillPlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }
    public void GainSkillReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.skill * 1.1f); });
    }

    public void GainMoraleAqua(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoraleBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoraleDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoraleBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoraleBug(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bug);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoraleMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoralePlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoraleDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }
    public void GainMoraleReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);
        axies.ForEach(axie => { Mathf.RoundToInt(axie.stats.morale * 1.1f); });
    }

    public IEnumerator AttackBuffAfterSeconds(Team team, AxieClass axieClass)
    {
        var axies = team.GetAliveCharactersByClass(axieClass);
        yield return new WaitForSeconds(10);

        foreach (var axieController in axies)
        {
            axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.AttackPositive, Attack = 2 });
        }
    }

    public IEnumerator SpeedBuffAfterSeconds(Team team, AxieClass axieClass)
    {
        var axies = team.GetAliveCharactersByClass(axieClass);
        yield return new WaitForSeconds(10);

        foreach (var axieController in axies)
        {
            axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.SpeedPositive, Speed = 2 });
        }
    }

    public IEnumerator MoraleBuffAfterSeconds(Team team, AxieClass axieClass)
    {
        var axies = team.GetAliveCharactersByClass(axieClass);
        yield return new WaitForSeconds(10);

        foreach (var axieController in axies)
        {
            axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.MoralePositive, Morale = 2 });
        }
    }

    public void MoraleBuffAfterSecondsAqua(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Aquatic)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsBird(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Bird)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsDawn(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Dawn)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsBeast(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Beast)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsBug(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Bug)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsMech(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Mech)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsPlant(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Plant)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsDusk(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Dusk)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsReptile(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, AxieClass.Reptile)); };

        team.OnBattleStartActions.Add(action);
    }

    public void SpeedBuffAfterSecondsAqua(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Aquatic)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsBird(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Bird)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsDawn(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Dawn)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsBeast(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Beast)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsBug(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Bug)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsMech(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Mech)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsPlant(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Plant)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsDusk(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Dusk)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsReptile(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, AxieClass.Reptile)); };

        team.OnBattleStartActions.Add(action);
    }

    public void AttackBuffAfterSecondsAqua(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Aquatic)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsBird(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Bird)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsDawn(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Dawn)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsBeast(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Beast)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsBug(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Bug)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsMech(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Mech)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsPlant(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Plant)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsDusk(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Dusk)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsReptile(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, AxieClass.Reptile)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AutoAttackHealAqua(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealBug(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bug);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealPlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);

        axies.ForEach(axie => { axie.axieSkillController.passives.HealOnDamageDealt += 8; });
    }

    public void ExtraAbilitySlotAqua(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotBug(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bug);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotPlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);
        if (axies.Count == 0)
            return;
        var mostHPAxie = axies.OrderByDescending(x => x.axieIngameStats.maxHP).First();
        var accountAxie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == mostHPAxie.AxieId.ToString());
        if (accountAxie.maxBodyPartAmount < 4)
            accountAxie.maxBodyPartAmount++;
    }

    public void BackdoorAqua(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);
        if (axies.Count == 0)
            return;
        var mostSpeedAqua = axies.OrderByDescending(x => x.stats.speed).First();
        mostSpeedAqua.ShrimpOnStart = true;
    }
    public void BackdoorBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);
        if (axies.Count == 0)
            return;
        var lessHPBird = axies.OrderBy(x => x.stats.hp).First();
        lessHPBird.ShrimpOnStart = true;
    }
    public void BackdoorDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
        if (axies.Count == 0)
            return;
        var mostHPDawn = axies.OrderByDescending(x => x.stats.hp).First();
        mostHPDawn.ShrimpOnStart = true;
    }
    public void BackdoorBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);
        if (axies.Count == 0)
            return;
        var mostMoraleBeast = axies.OrderByDescending(x => x.stats.morale).First();
        mostMoraleBeast.ShrimpOnStart = true;
    }
    public void BackdoorBug(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bug);
        if (axies.Count == 0)
            return;
        var lessSpeedBug = axies.OrderBy(x => x.stats.speed).First();
        lessSpeedBug.ShrimpOnStart = true;
    }
    public void BackdoorMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);
        if (axies.Count == 0)
            return;
        var leastMoraleMech = axies.OrderBy(x => x.stats.morale).First();
        leastMoraleMech.ShrimpOnStart = true;
    }
    public void BackdoorPlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);
        if (axies.Count == 0)
            return;
        var mostHPPlant = axies.OrderByDescending(x => x.stats.hp).First();
        mostHPPlant.ShrimpOnStart = true;
    }
    public void BackdoorDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);
        if (axies.Count == 0)
            return;
        var mostSpeedDusk = axies.OrderByDescending(x => x.stats.speed).First();
        mostSpeedDusk.ShrimpOnStart = true;
    }
    public void BackdoorReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);
        if (axies.Count == 0)
            return;
        var lessHPReptile = axies.OrderBy(x => x.stats.hp).First();
        lessHPReptile.ShrimpOnStart = true;
    }

    public void PrismaticAqua(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Aquatic);

        axies.ForEach(x =>
        {
            var speed = x.stats.speed;
            var morale = x.stats.morale;

            x.stats.speed = morale;
            x.stats.morale = speed;
            x.stats.hp += morale;
            x.UpdateStats();
        }
        );
    }
    public void PrismaticBird(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Bird);

        if (axies.Count == 0)
            return;

        axies.ForEach(x =>
        {
            x.stats.hp *= 2;
            x.UpdateStats();
        }
        );
    }
    public void PrismaticDawn(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
        if (axies.Count == 0)
            return;
        axies.ForEach(x => x.axieSkillController.passives.DamageReductionAmount += 25);
        Action action = () =>
        {
            var axies = team.GetCharactersAllByClass(AxieClass.Dawn);
            foreach (var axieController in axies)
            {
                axieController.axieSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.Aroma, Aroma = true, skillDuration = 99999 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }
    public void PrismaticBeast(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Beast);
        if (axies.Count == 0)
            return;
        axies.ForEach(x =>
        {
            var speed = x.stats.speed;
            var morale = x.stats.morale;

            x.stats.speed = morale;
            x.stats.morale = speed;
            x.stats.skill += speed;
            x.UpdateStats();
        }
        );
    }
    public void PrismaticBug(Team team)
    {
        var axies = team.GetCharactersAll();
        if (axies.Count == 0)
            return;
        var axie = axies.FirstOrDefault(x => x.axieIngameStats.axieClass == AxieClass.Bug);

        if (axie != null)
        {
            var hpFromBug = axie.stats.hp;
            var speedFromBug = axie.stats.speed;
            var moraleFromBug = axie.stats.morale;
            var skillFromBug = axie.stats.skill;

            axie.stats.speed = 1;
            axie.stats.morale = 1;
            axie.stats.skill = 1;

            axies.Remove(axie);
            foreach (var item in axies)
            {
                item.stats.hp += hpFromBug;
                item.stats.speed += speedFromBug;
                item.stats.morale += moraleFromBug;
                item.stats.skill += skillFromBug;
            }
        }
    }
    public void PrismaticMech(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Mech);
        if (axies.Count == 0)
            return;
        var mostMoraleAxie = axies.OrderByDescending(x => x.stats.morale).First();

        mostMoraleAxie.stats.hp *= 2;
        mostMoraleAxie.stats.speed *= 2;
        mostMoraleAxie.stats.skill *= 2;
        mostMoraleAxie.stats.morale *= 2;
        mostMoraleAxie.UpdateStats();
    }

    public void PrismaticPlant(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Plant);
        if (axies.Count == 0)
            return;
        foreach (var item in axies)
        {
            item.axieSkillController.passives.ExtraShieldGained = 100;
        }
    }
    public void PrismaticDusk(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Dusk);
        if (axies.Count == 0)
            return;
        foreach (var item in axies)
        {
            item.axieSkillController.passives.bloodmoonImmune = true;
        }
    }
    public void PrismaticReptile(Team team)
    {
        var axies = team.GetCharactersAllByClass(AxieClass.Reptile);
        if (axies.Count == 0)
            return;
        foreach (var item in axies)
        {
            item.axieSkillController.passives.AbilityReflectDamageAmount = 100;
            item.axieSkillController.passives.ExtraDamageReceivedByAbilitiesAmount = 100;
        }
    }
}