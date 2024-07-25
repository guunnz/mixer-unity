using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class AxieGeneUtils
{
    public enum Cls
    {
        beast,
        bug,
        bird,
        plant,
        aquatic,
        reptile,
        mech,
        dusk,
        dawn,
    }

    public enum PartType
    {
        eyes,
        ears,
        mouth,
        horn,
        back,
        tail,
    }

    public class GeneBinGroup
    {
        public string cls;
        public string region;
        public string xMas;
        public string eyes;
        public string ears;
        public string mouth;
        public string horn;
        public string back;
        public string tail;
    }

    public enum PartSkin
    {
        undefined = -1,
        Global,
        Mystic,
        Japan,
        Xmas1,
        Xmas2,
        Bionic,
    }

    static Dictionary<string, Cls> binClassMap = new Dictionary<string, Cls>
    {
        // 256 Classes
        { "0000", Cls.beast },
        { "0001", Cls.bug },
        { "0010", Cls.bird },
        { "0011", Cls.plant },
        { "0100", Cls.aquatic },
        { "0101", Cls.reptile },
        { "1000", Cls.mech },
        { "1001", Cls.dawn },
        { "1010", Cls.dusk },
        // 512 Classes
        { "00000", Cls.beast },
        { "00001", Cls.bug },
        { "00010", Cls.bird },
        { "00011", Cls.plant },
        { "00100", Cls.aquatic },
        { "00101", Cls.reptile },
        { "10000", Cls.mech },
        { "10001", Cls.dawn },
        { "10010", Cls.dusk },
    };

    static Dictionary<string, PartSkin> binPartSkinMap = new Dictionary<string, PartSkin>
    {
        // 256 Classes
        { "00000", PartSkin.Global },
        { "00001", PartSkin.Japan },
        { "010101010101", PartSkin.Xmas1 },
        { "01", PartSkin.Bionic },
        { "10", PartSkin.Xmas2 },
        { "11", PartSkin.Mystic },
        // 512 PartSkins
        { "0000", PartSkin.Global },
        { "0001", PartSkin.Mystic },
        { "0011", PartSkin.Japan },
        { "0100", PartSkin.Xmas1 },
        { "0101", PartSkin.Xmas2 },
        { "0010", PartSkin.Bionic },
    };

    enum HexType
    {
        Bit256 = 256,
        Bit512 = 512,
    }

    const HexType hexType = HexType.Bit256;
    //GeneBinGroup geneBinGroup;

    public static List<string> ParsePartIdsFromHex(string hex)
    {
        string hexBin = "";
        hex = hex.Replace("0x", "");
        hexBin = String.Join(String.Empty,
            hex.Select(
                c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
            )
        );
        hexBin = hexBin.PadLeft(hexType == HexType.Bit256 ? 256 : 512, '0');
        GeneBinGroup geneBinGroup = new GeneBinGroup
        {
            cls = hexType == HexType.Bit256
                ? hexBin.Substring(0, 4)
                : hexBin.Substring(0, 5),
            region = hexType == HexType.Bit256
                ? hexBin.Substring(8, 5)
                : hexBin.Substring(22, 18),
            xMas = hexType == HexType.Bit256 ? hexBin.Substring(22, 12) : "",
            eyes = hexType == HexType.Bit256
                ? hexBin.Substring(64, 32)
                : hexBin.Substring(149, 43),
            mouth = hexType == HexType.Bit256
                ? hexBin.Substring(96, 32)
                : hexBin.Substring(213, 43),
            ears = hexType == HexType.Bit256
                ? hexBin.Substring(128, 32)
                : hexBin.Substring(277, 43),
            horn = hexType == HexType.Bit256
                ? hexBin.Substring(160, 32)
                : hexBin.Substring(341, 43),
            back = hexType == HexType.Bit256
                ? hexBin.Substring(192, 32)
                : hexBin.Substring(405, 43),
            tail = hexType == HexType.Bit256
                ? hexBin.Substring(224, 32)
                : hexBin.Substring(469, 43),
        };

        Cls cls = parsePartClass(geneBinGroup.cls);
        List<string> lst = new List<string>();
        lst.Add(parsePart(geneBinGroup, geneBinGroup.eyes, PartType.eyes));
        lst.Add(parsePart(geneBinGroup, geneBinGroup.ears, PartType.ears));
        lst.Add(parsePart(geneBinGroup, geneBinGroup.horn, PartType.horn));
        lst.Add(parsePart(geneBinGroup, geneBinGroup.mouth, PartType.mouth));
        lst.Add(parsePart(geneBinGroup, geneBinGroup.back, PartType.back));
        lst.Add(parsePart(geneBinGroup, geneBinGroup.tail, PartType.tail));
        return lst;
    }

    private static Cls parsePartClass(string bin)
    {
        var ret = binClassMap[bin];
        return ret;
    }

    static string parsePart(GeneBinGroup geneBinGroup, string partBin, PartType partType)
    {
        //var regionBin = geneBinGroup.region;

        //var dSkinBin =  hexType == HexType.Bit256
        //    ? partBin.Substring(0, 2)
        //    : partBin.Substring(0, 4);
        //var dSkin = parsePartSkin(geneBinGroup, regionBin, dSkinBin);

        var dClass = parsePartClass(
            hexType == HexType.Bit256
                ? partBin.Substring(2, 4)
                : partBin.Substring(4, 5)
        );

        var dBin =
            hexType == HexType.Bit256
                ? partBin.Substring(6, 6)
                : partBin.Substring(11, 6);
        int index = Convert.ToInt32(dBin, 2);
        string abilityId = $"{dClass}-{partType}-{index:00}";
        return abilityId;
    }

    static PartSkin parsePartSkin(GeneBinGroup geneBinGroup, string regionBin, string skinBin)
    {
        binPartSkinMap.TryGetValue(skinBin, out var ret);
        if (skinBin == "00")
        {
            if (geneBinGroup.xMas == "010101010101") ret = PartSkin.Xmas1;
            else binPartSkinMap.TryGetValue(regionBin, out ret);
        }

        if (ret == PartSkin.undefined)
        {
            //throw new Error('cannot recognize part skin');
        }

        return ret;
    }

    public static BigInteger ParseGenes(string genesStr)
    {
        BigInteger genes = BigInteger.Zero;
        if (!string.IsNullOrEmpty(genesStr))
        {
            if (genesStr.CustomStartsWith("0x"))
            {
                string finalGenes256 = genesStr.Substring(2);
                if (finalGenes256.Length < 64)
                {
                    finalGenes256 = finalGenes256.PadLeft(64, '0');
                }

                BigInteger.TryParse(finalGenes256, System.Globalization.NumberStyles.AllowHexSpecifier, null,
                    out genes);
            }
            else
            {
                BigInteger.TryParse(genesStr, out genes);
            }
        }

        return genes;
    }

    public static (int, int, int, int) GetStats(List<string> classCombo)
    {
        float hp, speed, skill, morale;
        hp = speed = skill = morale = 0f;
        switch (classCombo[0])
        {
            case "beast":
                hp = -1;
                skill = -1;
                morale = 2;
                break;
            case "bug":
                speed = -1;
                morale = 1;
                break;
            case "bird":
                hp = -2;
                speed = 2;
                break;
            case "plant":
                hp = 2;
                speed = -1;
                skill = -1;
                break;
            case "aquatic":
                hp = 1;
                speed = 1;
                morale = -2;
                break;
            case "reptile":
                hp = 1;
                skill = -1;
                break;
            case "mech":
                hp = -1;
                speed = 1;
                skill = 2;
                morale = -2;
                break;
            case "dawn":
                speed = 1;
                skill = 1;
                morale = -2;
                break;
            case "dusk":
                hp = 2;
                speed = 1;
                skill = -2;
                morale = -1;
                break;
            default:
                hp = -8.75f;
                speed = -8.75f;
                skill = -8.75f;
                morale = -8.75f;
                break;
        }

        int iHP = 35 + (int)(Mathf.Round(hp) * 4);
        int iSpeed = 35 + (int)(Mathf.Round(speed) * 4);
        int iSkill = 35 + (int)(Mathf.Round(skill) * 4);
        int iMorale = 35 + (int)(Mathf.Round(morale) * 4);

        for (int i = 1; i < classCombo.Count; i++)
        {
            switch (classCombo[i])
            {
                case "beast":
                    iSpeed += 1;
                    iMorale += 3;
                    break;
                case "Bug":
                    iHP += 1;
                    iMorale += 3;
                    break;
                case "bird":
                    iSpeed += 3;
                    iMorale += 1;
                    break;
                case "plant":
                    iHP += 3;
                    iMorale += 1;
                    break;
                case "aquatic":
                    iHP += 1;
                    iSpeed += 3;
                    break;
                case "reptile":
                    iHP += 3;
                    iSpeed += 1;
                    break;
            }
        }


        return (
            iHP * 6 + 150,
            iSpeed,
            iSkill,
            iMorale
        );
    }
}

public static class StringExtensions
{
    public static bool CustomStartsWith(this string str, string prefix)
    {
        return str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }
}