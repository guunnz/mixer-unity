using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;

public class ShopItemsManager : MonoBehaviour
{
    static public ShopItemsManager instance;

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
}