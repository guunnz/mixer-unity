using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using static GetMonstersExample;

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
        var monsters = team.GetCharactersAll();
        monsters.ForEach(x => x.ShrimpOnStart = true);
    }

    public void Topaz(Team team)
    {
        if (!team.isGoodTeam)
            return;

        RunManagerSingleton.instance.economyPassive.ExtraCoinsPerRound += 1;
    }

    public void ThreeWondersShoes(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster =>
        {
            monster.stats.skill += 1;
            monster.stats.hp += 1;
            monster.stats.morale += 1;
            monster.stats.speed += 1;
        });
    }

    public void KingsBoots(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster =>
        {
            monster.stats.skill += 3;
            monster.stats.hp += 3;
            monster.stats.morale += 3;
            monster.stats.speed += 3;
        });
    }

    public void SteelShield(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster =>
        {
            monster.monsterSkillController.passives.AbilityReflectDamageAmount += 8;
            monster.monsterSkillController.passives.MeleeReflectDamageAmount += 8;
        });
    }

    public void IronShield(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster =>
        {
            monster.monsterSkillController.passives.AbilityReflectDamageAmount += 4;
            monster.monsterSkillController.passives.MeleeReflectDamageAmount += 4;
        });
    }

    public void SteelHelmet(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.monsterSkillController.passives.ExtraArmorHelmet += 100; });
    }

    public void IronHelmet(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.monsterSkillController.passives.ExtraArmorHelmet += 50; });
    }

    public void HoneyRoastedChicken(Team team)
    {

        var monsters = team.GetCharactersAll();

        int maxHp = monsters.Max(x => x.stats.hp);
        monsters.ForEach(monster => { monster.stats.hp = maxHp; });
    }

    public void IronChainmail(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.monsterSkillController.passives.MeleeReflectDamageAmount += 5; });
    }

    public void SteelChainmail(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.monsterSkillController.passives.MeleeReflectDamageAmount += 10; });
        monsters.ForEach(monster => { monster.monsterSkillController.passives.AbilityReflectDamageAmount += 10; });
    }

    public void SteelSword(Team team)
    {

        Action action = () =>
        {
            var monsters = team.GetCharactersAll();
            foreach (var monsterController in monsters)
            {
                monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.AttackPositive, Attack = 1, skillDuration = 99999 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void SteelHammer(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.morale += 3; });
    }

    public void SteelBoots(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.speed += 3; });
    }

    public void SteelArrow(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.monsterSkillController.passives.rangedAlwaysCritical = true; });
    }

    public void SteakNFries(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.hp += 1; });
    }

    public void SilverTopazBracelet(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Beast || x.monsterIngameStats.monsterClass == MonsterClass.Mech ||
            x.monsterIngameStats.monsterClass == MonsterClass.Bug).ToList();

        monsters.ForEach(monster => { monster.stats.skill += 3; });
    }

    public void SilverRubyBracelet(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Aquatic || x.monsterIngameStats.monsterClass == MonsterClass.Bird ||
            x.monsterIngameStats.monsterClass == MonsterClass.Dawn).ToList();

        monsters.ForEach(monster => { monster.stats.skill += 3; });
    }

    public void SilverEmeraldBracelet(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Plant || x.monsterIngameStats.monsterClass == MonsterClass.Dusk ||
            x.monsterIngameStats.monsterClass == MonsterClass.Reptile).ToList();

        monsters.ForEach(monster => { monster.stats.skill += 3; });
    }

    public void SilverDiamondBracelet(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.skill += 5; });
    }

    public void SilverAmethystBracelet(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.skill += 2; });
    }

    public void Ramen(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.skill += 1; });
    }

    public void PlatinumLantern(Team team)
    {
        var monsters = team.GetCharactersAll();

        int maxSkill = monsters.Max(x => x.stats.skill);
        monsters.ForEach(monster => { monster.stats.skill = maxSkill; });
    }

    public void GoldenSphere(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster =>
        {
            int skillHalved = Mathf.RoundToInt(monster.stats.skill / 2f);
            monster.stats.skill = skillHalved;

            int pointsToAddToStats = Mathf.RoundToInt(skillHalved / 3f);

            monster.stats.hp += pointsToAddToStats;
            monster.stats.speed += pointsToAddToStats;
            monster.stats.morale += pointsToAddToStats;
        });
    }

    public void HoneyCarrotSoup(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 3; });
    }

    public void GrilledLambChop(Team team)
    {
        Action action = () => { StartCoroutine(GrilledLambChopAfterSeconds(team: team)); };

        team.OnBattleStartActions.Add(action);
    }




    public IEnumerator GrilledLambChopAfterSeconds(Team team)
    {
        var monsters = team.GetAliveCharacters();
        yield return new WaitForSeconds(15);

        foreach (var monsterController in monsters)
        {
            monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.AttackPositive, Attack = 3 });
        }
    }

    public void PumpkinPie(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.hp += 2; });
    }

    public void ShellOfStun(Team team)
    {

        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll();
            foreach (var monsterController in monsters)
            {
                monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.Stun, Stun = true, skillDuration = 2 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfSilence(Team team)
    {

        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll();
            foreach (var monsterController in monsters)
            {
                monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.Jinx });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfShock(Team team)
    {

        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll();
            foreach (var monsterController in monsters)
            {
                monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.None, ApplyRandomEffect = true, skillDuration = 1 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfMediumConcentratedDamage(Team team)
    {

        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll().OrderByDescending(x => x.monsterIngameStats.maxHP);
            monsters.First().monsterIngameStats.currentHP *= 0.5f;
        };

        team.OnBattleStartActions.Add(action);
    }

    public void MediumPotionOfShield(Team team)
    {

        Action action = () =>
        {
            var monsters = team.GetCharactersAll();
            if (team.landType == LandType.lunalanding)
            {
                monsters.ForEach(x => x.monsterIngameStats.currentShield += 100f);
            }
            else
            {

                monsters.ForEach(x => x.monsterIngameStats.currentShield += 50f);
            }

        };

        team.OnBattleStartActions.Add(action);
    }

    public void MediumHastePotion(Team team)
    {

        Action action = () =>
        {

            var monsters = team.GetCharactersAll();
            if (team.landType == LandType.lunalanding)
            {
                monsters.ForEach(x => x.stats.speed += 6);
            }
            else
            {
                monsters.ForEach(x => x.stats.speed += 3);

            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargeHastePotion(Team team)
    {

        Action action = () =>
        {

            var monsters = team.GetCharactersAll();
            if (team.landType == LandType.lunalanding)
            {
                monsters.ForEach(x => x.stats.speed += 12);
            }
            else
            {

                monsters.ForEach(x => x.stats.speed += 6);
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargeEnergyPotion(Team team)
    {

        Action action = () =>
        {

            var monsters = team.GetCharactersAll();
            if (team.landType == LandType.lunalanding)
            {
                monsters.ForEach(x => x.stats.skill += 12);
            }
            else
            {

                monsters.ForEach(x => x.stats.skill += 6);
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargePotionOfShield(Team team)
    {

        Action action = () =>
        {

            var monsters = team.GetCharactersAll();
            if (team.landType == LandType.lunalanding)
            {
                monsters.ForEach(x => x.monsterIngameStats.currentShield += 200f);
            }
            else
            {
                monsters.ForEach(x => x.monsterIngameStats.currentShield += 100f);
            }
        };

        team.OnBattleStartActions.Add(action);
    }

    public void LargePotionOfResistance(Team team)
    {
        var monsters = team.GetCharactersAll();
        if (team.landType == LandType.lunalanding)
        {
            monsters.ForEach(x => x.monsterSkillController.passives.DamageReductionAmount += 30);
        }
        else
        {

            monsters.ForEach(x => x.monsterSkillController.passives.DamageReductionAmount += 15);
        }
    }


    public void SteelArmor(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(x => x.monsterSkillController.passives.AbilityReflectDamageAmount += 15);
    }
    public void IronArmor(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(x => x.monsterSkillController.passives.AbilityReflectDamageAmount += 10);
    }
    public void MeatPie(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(x => x.stats.hp += 3);
    }

    public void ShellOfMediumAreaDamage(Team team)
    {


        Action action = () =>
        {

            var monsters = team.enemyTeam.GetCharactersAll();
            monsters.ForEach(x => x.monsterIngameStats.currentHP *= .9f);
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfDistraction(Team team)
    {

        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll().OrderByDescending(x => x.monsterIngameStats.maxHP);

            monsters.First().monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.Stench, Stench = true });
        };

        team.OnBattleStartActions.Add(action);
    }

    public void ShellOfBurn(Team team)
    {
        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll();
            monsters.ForEach(x => x.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.Poison, Poison = true, PoisonStack = 1 }));
        };

        team.OnBattleStartActions.Add(action);

    }

    public void ShellOfBlinding(Team team)
    {
        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll();
            monsters.ForEach(x => x.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.Fear }));
        };

        team.OnBattleStartActions.Add(action);

    }

    public void ShellOfSlow(Team team)
    {

        Action action = () =>
        {
            var monsters = team.enemyTeam.GetCharactersAll();
            foreach (var monsterController in monsters)
            {
                monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.SpeedNegative });
            }
        };

        team.OnBattleStartActions.Add(action);
    }



    public void MoonSphere(Team team)
    {

        Action action = () =>
        {
            var monsters = team.GetCharactersAll();
            monsters.ForEach(x => x.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.None, RandomEffectIsBuff = true, skillDuration = 1 }));
        };

        team.OnBattleStartActions.Add(action);
    }

    public void RubyBook(Team team)
    {

        Action action = () =>
        {
            var monsters = team.GetCharactersAll();
            monsters.ForEach(x => x.monsterIngameStats.CurrentEnergy += x.monsterIngameStats.MaxEnergy * .3f);
        };


        team.OnBattleStartActions.Add(action);
    }

    public void GoldTopazNecklace(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Beast || x.monsterIngameStats.monsterClass == MonsterClass.Mech ||
            x.monsterIngameStats.monsterClass == MonsterClass.Bug).ToList();

        monsters.ForEach(monster => { monster.stats.morale += 3; });
    }

    public void GoldRubyNecklace(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Aquatic || x.monsterIngameStats.monsterClass == MonsterClass.Bird ||
            x.monsterIngameStats.monsterClass == MonsterClass.Dawn).ToList();

        monsters.ForEach(monster => { monster.stats.morale += 3; });
    }

    public void FruitNVegetablesSalad(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Plant).ToList();

        monsters.ForEach(monster => { monster.stats.hp *= 2; });
    }

    public void DeluxeRamen(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Bird).ToList();

        monsters.ForEach(monster => { monster.stats.speed *= 2; });
    }

    public void DeluxeFriedRice(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Beast).ToList();

        monsters.ForEach(monster => { monster.stats.morale *= 2; });
    }

    public void Amethyst(Team team)
    {
        if (team.isGoodTeam)
            RunManagerSingleton.instance.economyPassive.FreeRerollEveryXRolls = 3;
    }


    public void CompositeBow(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);

        monsters.ForEach(monster => { monster.Range += 2; monster.monsterIngameStats.Range += 5; });
    }

    public void GoldEmeraldNecklace(Team team)
    {
        var monsters = team.GetCharactersAll().Where(x =>
            x.monsterIngameStats.monsterClass == MonsterClass.Plant || x.monsterIngameStats.monsterClass == MonsterClass.Dusk ||
            x.monsterIngameStats.monsterClass == MonsterClass.Reptile).ToList();

        monsters.ForEach(monster => { monster.stats.morale += 3; });
    }

    public void GoldDiamondNecklace(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.morale += 5; });
    }

    public void GoldAmethystNecklace(Team team)
    {
        var monsters = team.GetCharactersAll();

        monsters.ForEach(monster => { monster.stats.morale += 2; });
    }

    public void Increase_HP_Bug(Team team)
    {
        var monsters = team.GetCharactersAll();

        if (team.landType == LandType.lunalanding)
        {

            monsters.ForEach(monster => { monster.stats.hp += 2; });
        }
        else
        {

            monsters.ForEach(monster => { monster.stats.hp += 1; });
        }
    }

    public void Increase_HP(Team team)
    {
        var monsters = team.GetCharactersAll();
        if (team.landType == LandType.lunalanding)
        {

            monsters.ForEach(monster => { monster.stats.hp += 2; });
        }
        else
        {
            monsters.ForEach(monster => { monster.stats.hp += 1; });
        }
    }

    public void Increase_Morale(Team team)
    {
        var monsters = team.GetCharactersAll();
        if (team.landType == LandType.lunalanding)
        {

            monsters.ForEach(monster => { monster.stats.morale += 2; });
        }
        else
        {
            monsters.ForEach(monster => { monster.stats.morale += 1; });
        }
    }

    public void Increase_Skill(Team team)
    {
        var monsters = team.GetCharactersAll();
        if (team.landType == LandType.lunalanding)
        {

            monsters.ForEach(monster => { monster.stats.skill += 2; });
        }
        else
        {
            monsters.ForEach(monster => { monster.stats.skill += 1; });
        }

    }

    public void Increase_Speed(Team team)
    {
        var monsters = team.GetCharactersAll();
        if (team.landType == LandType.lunalanding)
        {

            monsters.ForEach(monster => { monster.stats.speed += 2; });
        }
        else
        {
            monsters.ForEach(monster => { monster.stats.speed += 1; });
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
            var monsters = team.GetCharactersAll();
            monsters.ForEach(x => x.monsterIngameStats.CurrentEnergy += x.monsterIngameStats.MaxEnergy * .5f);
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
        var monsters = team.GetCharactersAll();

        foreach (var monsterController in monsters.Where(x => x.Range < 2))
        {
            monsterController.Range += 3;
        }
    }


    /// <summary>
    /// Auguments
    /// </summary>
    /// <param name="team"></param>

    public void GainHPAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPBird(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPBug(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bug);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPPlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }
    public void GainHPReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.hp * 1.1f); });
    }

    public void GainSpeedAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedBird(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedBug(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bug);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedPlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }
    public void GainSpeedReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.speed * 1.1f); });
    }

    public void GainSkillAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillBird(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillBug(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bug);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillPlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }
    public void GainSkillReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.skill * 1.1f); });
    }

    public void GainMoraleAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoraleBird(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoraleDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoraleBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoraleBug(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bug);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoraleMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoralePlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoraleDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }
    public void GainMoraleReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);
        monsters.ForEach(monster => { Mathf.RoundToInt(monster.stats.morale * 1.1f); });
    }

    public IEnumerator AttackBuffAfterSeconds(Team team, MonsterClass monsterClass)
    {
        var monsters = team.GetAliveCharactersByClass(monsterClass);
        yield return new WaitForSeconds(10);

        foreach (var monsterController in monsters)
        {
            monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.AttackPositive, Attack = 2 });
        }
    }

    public IEnumerator SpeedBuffAfterSeconds(Team team, MonsterClass monsterClass)
    {
        var monsters = team.GetAliveCharactersByClass(monsterClass);
        yield return new WaitForSeconds(10);

        foreach (var monsterController in monsters)
        {
            monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.SpeedPositive, Speed = 2 });
        }
    }

    public IEnumerator MoraleBuffAfterSeconds(Team team, MonsterClass monsterClass)
    {
        var monsters = team.GetAliveCharactersByClass(monsterClass);
        yield return new WaitForSeconds(10);

        foreach (var monsterController in monsters)
        {
            monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
            { statusEffect = StatusEffectEnum.MoralePositive, Morale = 2 });
        }
    }

    public void MoraleBuffAfterSecondsAqua(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Aquatic)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsBird(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Bird)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsDawn(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Dawn)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsBeast(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Beast)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsBug(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Bug)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsMech(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Mech)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsPlant(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Plant)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsDusk(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Dusk)); };

        team.OnBattleStartActions.Add(action);
    }
    public void MoraleBuffAfterSecondsReptile(Team team)
    {
        Action action = () => { StartCoroutine(MoraleBuffAfterSeconds(team: team, MonsterClass.Reptile)); };

        team.OnBattleStartActions.Add(action);
    }

    public void SpeedBuffAfterSecondsAqua(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Aquatic)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsBird(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Bird)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsDawn(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Dawn)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsBeast(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Beast)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsBug(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Bug)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsMech(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Mech)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsPlant(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Plant)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsDusk(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Dusk)); };

        team.OnBattleStartActions.Add(action);
    }
    public void SpeedBuffAfterSecondsReptile(Team team)
    {
        Action action = () => { StartCoroutine(SpeedBuffAfterSeconds(team: team, MonsterClass.Reptile)); };

        team.OnBattleStartActions.Add(action);
    }

    public void AttackBuffAfterSecondsAqua(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Aquatic)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsBird(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Bird)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsDawn(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Dawn)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsBeast(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Beast)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsBug(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Bug)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsMech(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Mech)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsPlant(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Plant)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsDusk(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Dusk)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AttackBuffAfterSecondsReptile(Team team)
    {
        Action action = () => { StartCoroutine(AttackBuffAfterSeconds(team: team, MonsterClass.Reptile)); };

        team.OnBattleStartActions.Add(action);
    }
    public void AutoAttackHealAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealBird(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealBug(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bug);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealPlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }
    public void AutoAttackHealReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);

        monsters.ForEach(monster => { monster.monsterSkillController.passives.HealOnDamageDealt += 8; });
    }

    public void ExtraAbilitySlotAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotBird(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotBug(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bug);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotPlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }
    public void ExtraAbilitySlotReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);
        if (monsters.Count == 0)
            return;
        var mostHPMonster = monsters.OrderByDescending(x => x.monsterIngameStats.maxHP).First();
        var accountMonster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == mostHPMonster.MonsterId.ToString());
        if (accountMonster.maxBodyPartAmount < 4)
            accountMonster.maxBodyPartAmount++;
    }

    public void BackdoorAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);
        if (monsters.Count == 0)
            return;
        var mostSpeedAqua = monsters.OrderByDescending(x => x.stats.speed).First();
        mostSpeedAqua.ShrimpOnStart = true;
    }
    public void BackdoorBird(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);
        if (monsters.Count == 0)
            return;
        var lessHPBird = monsters.OrderBy(x => x.stats.hp).First();
        lessHPBird.ShrimpOnStart = true;
    }
    public void BackdoorDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
        if (monsters.Count == 0)
            return;
        var mostHPDawn = monsters.OrderByDescending(x => x.stats.hp).First();
        mostHPDawn.ShrimpOnStart = true;
    }
    public void BackdoorBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);
        if (monsters.Count == 0)
            return;
        var mostMoraleBeast = monsters.OrderByDescending(x => x.stats.morale).First();
        mostMoraleBeast.ShrimpOnStart = true;
    }
    public void BackdoorBug(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bug);
        if (monsters.Count == 0)
            return;
        var lessSpeedBug = monsters.OrderBy(x => x.stats.speed).First();
        lessSpeedBug.ShrimpOnStart = true;
    }
    public void BackdoorMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);
        if (monsters.Count == 0)
            return;
        var leastMoraleMech = monsters.OrderBy(x => x.stats.morale).First();
        leastMoraleMech.ShrimpOnStart = true;
    }
    public void BackdoorPlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);
        if (monsters.Count == 0)
            return;
        var mostHPPlant = monsters.OrderByDescending(x => x.stats.hp).First();
        mostHPPlant.ShrimpOnStart = true;
    }
    public void BackdoorDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);
        if (monsters.Count == 0)
            return;
        var mostSpeedDusk = monsters.OrderByDescending(x => x.stats.speed).First();
        mostSpeedDusk.ShrimpOnStart = true;
    }
    public void BackdoorReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);
        if (monsters.Count == 0)
            return;
        var lessHPReptile = monsters.OrderBy(x => x.stats.hp).First();
        lessHPReptile.ShrimpOnStart = true;
    }

    public void PrismaticAqua(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Aquatic);

        monsters.ForEach(x =>
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
        var monsters = team.GetCharactersAllByClass(MonsterClass.Bird);

        if (monsters.Count == 0)
            return;

        monsters.ForEach(x =>
        {
            x.stats.hp *= 2;
            x.UpdateStats();
        }
        );
    }
    public void PrismaticDawn(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
        if (monsters.Count == 0)
            return;
        monsters.ForEach(x => x.monsterSkillController.passives.DamageReductionAmount += 25);
        Action action = () =>
        {
            var monsters = team.GetCharactersAllByClass(MonsterClass.Dawn);
            foreach (var monsterController in monsters)
            {
                monsterController.monsterSkillEffectManager.AddStatusEffect(new SkillEffect()
                { statusEffect = StatusEffectEnum.Aroma, Aroma = true, skillDuration = 99999 });
            }
        };

        team.OnBattleStartActions.Add(action);
    }
    public void PrismaticBeast(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Beast);
        if (monsters.Count == 0)
            return;
        monsters.ForEach(x =>
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
        var monsters = team.GetCharactersAll();
        if (monsters.Count == 0)
            return;
        var monster = monsters.FirstOrDefault(x => x.monsterIngameStats.monsterClass == MonsterClass.Bug);

        if (monster != null)
        {
            var hpFromBug = monster.stats.hp;
            var speedFromBug = monster.stats.speed;
            var moraleFromBug = monster.stats.morale;
            var skillFromBug = monster.stats.skill;

            monster.stats.speed = 1;
            monster.stats.morale = 1;
            monster.stats.skill = 1;

            monsters.Remove(monster);
            foreach (var item in monsters)
            {
                item.stats.hp += hpFromBug / 3;
                item.stats.speed += speedFromBug / 3;
                item.stats.morale += moraleFromBug / 3;
                item.stats.skill += skillFromBug / 3;
            }
        }
    }
    public void PrismaticMech(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Mech);
        if (monsters.Count == 0)
            return;
        var mostMoraleMonster = monsters.OrderByDescending(x => x.stats.morale).First();

        mostMoraleMonster.stats.hp *= 2;
        mostMoraleMonster.stats.speed *= 2;
        mostMoraleMonster.stats.skill *= 2;
        mostMoraleMonster.stats.morale *= 2;
        mostMoraleMonster.UpdateStats();
    }

    public void PrismaticPlant(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Plant);
        if (monsters.Count == 0)
            return;
        foreach (var item in monsters)
        {
            item.monsterSkillController.passives.ExtraShieldGained = 100;
        }
    }
    public void PrismaticDusk(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Dusk);
        if (monsters.Count == 0)
            return;
        foreach (var item in monsters)
        {
            item.monsterSkillController.passives.bloodmoonImmune = true;
        }
    }
    public void PrismaticReptile(Team team)
    {
        var monsters = team.GetCharactersAllByClass(MonsterClass.Reptile);
        if (monsters.Count == 0)
            return;
        foreach (var item in monsters)
        {
            item.monsterSkillController.passives.AbilityReflectDamageAmount = 50;
            item.monsterSkillController.passives.ExtraDamageReceivedByAbilitiesAmount = 100;
        }
    }
    public void MonsterPark(Team team)
    {
        if (!team.isGoodTeam)
            return;

        var monsters = team.GetCharactersAll();

        foreach (var item in monsters)
        {
            float amountImprovement = item.stats.hp * 1.03f;

            item.stats.hp = Mathf.RoundToInt(amountImprovement);
        }
    }
    public void Savannah(Team team)
    {
        if (!team.isGoodTeam)
            return;
        RunManagerSingleton.instance.RemoveCoins(((ShopManager.instance.reRolls / 2) + 5) * -1);
        ShopManager.instance.reRolls = 0;
    }

    public void Forest(Team team)
    {
        if (!team.isGoodTeam)
            return;

        RunManagerSingleton.instance.economyPassive.premiumForest = true;
        foreach (var item in ShopManager.instance.Potions.ToList())
        {
            item.SetItem(item.shopItem);
        };
        foreach (var item in ShopManager.instance.Items.ToList())
        {
            item.SetItem(item.shopItem);
        };
    }


    public void Arctic(Team team)
    {
        if (!team.isGoodTeam)
            return;

        RunManagerSingleton.instance.economyPassive.frozenItemFree += 2;
        foreach (var item in ShopManager.instance.Potions.ToList())
        {
            item.SetItem(item.shopItem);
        };
        foreach (var item in ShopManager.instance.Items.ToList())
        {
            item.SetItem(item.shopItem);
        };
    }

    public void Mystic(Team team)
    {
        if (!team.isGoodTeam)
            return;

        RunManagerSingleton.instance.IncreaseCoinsAndMax((5 + RunManagerSingleton.instance.economyPassive.atiasNotRerolled / 2));
        RunManagerSingleton.instance.economyPassive.atiasNotRerolled = 0;
    }

    public void Genesis(Team team)
    {
        if (!team.isGoodTeam)
            return;

        RunManagerSingleton.instance.IncreaseCoinsAndMax((5 + (RunManagerSingleton.instance.economyPassive.genesisEconomyGained)));
        RunManagerSingleton.instance.economyPassive.genesisEconomyGained = 0;
    }

    public void LunasLanding(Team team)
    {
        if (!team.isGoodTeam)
            return;

        RunManagerSingleton.instance.IncreaseCoinsAndMax((1 + RunManagerSingleton.instance.economyPassive.smoothPotionsPurchased));
        RunManagerSingleton.instance.economyPassive.smoothPotionsPurchased = 0;
    }
}