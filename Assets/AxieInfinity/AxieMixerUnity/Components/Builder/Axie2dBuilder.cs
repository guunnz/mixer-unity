﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkyMavis.AxieMixer.Extensions;
using Spine.Unity;
using UnityEngine;

namespace SkyMavis.AxieMixer.Unity
{
    public class Axie2dBuilderResult
    {
        public string error;
        public SkeletonDataAsset skeletonDataAsset;
        public Dictionary<string, string> adultCombo;
        public Material sharedGraphicMaterial;
    }

    public class Axie2dBuilder
    {
        public IAxieMixerMaterials axieMixerMaterials { get; private set; }
        public bool isGraphicLinear { get; private set; }

        public void Init(IAxieMixerMaterials axieMixerMaterials)
        {
            this.axieMixerMaterials = axieMixerMaterials;
            this.isGraphicLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;
        }

        public int GetSampleColorVariant(CharacterClass characterClass, int colorValue)
        {
            var lst = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal).axieSkinColors.Where(x => x.@class == characterClass && x.skin == 0).ToList();
            var axieSkinColor = lst[colorValue % lst.Count];
            return axieMixerMaterials.GetGenesStuff(AxieFormType.Normal).axieSkinColors.IndexOf(axieSkinColor);
        }

        public Axie2dBuilderResult BuildSpineFromGene(string axieId, string genesStr, Dictionary<string, string> meta, float scale, bool isGraphic = false)
        {
            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);

            if (genesStr.StartsWith("0x"))
            {
                genesStr = genesStr.Substring(2);
            }
            string finalGenes512 = genesStr;
            if (finalGenes512.Length < 128)
            {
                finalGenes512 = finalGenes512.PadLeft(128, '0');
            }
            System.Numerics.BigInteger.TryParse(finalGenes512, System.Globalization.NumberStyles.HexNumber, null, out var genes);
            var bodyStructure = axieGenesStuff.GetAxieBodyStructure512(genes);

            return BuildSpineFromGene(axieId, bodyStructure, meta, scale, isGraphic);
        }

        public Axie2dBuilderResult BuildSpineFromGene(string axieId, string genesStr, float scale, bool isGraphic = false)
        {
            return BuildSpineFromGene(axieId, genesStr, new Dictionary<string, string>(), scale, isGraphic);
        }

        public Axie2dBuilderResult BuildSpineFromGene(string axieId, AxieBodyStructure bodyStructure, Dictionary<string, string> meta, float scale, bool isGraphic = false)
        {
            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);
            var adultCombo = axieGenesStuff.GetAdultCombo(bodyStructure);
            if (axieId.Length < 6)
            {
                axieId = axieId.PadLeft(axieId.Length + (7 - axieId.Length) / 2);
            }
            adultCombo.Add("body-id", axieId);
            foreach (var p in meta)
            {
                if (!adultCombo.ContainsKey(p.Key))
                {
                    adultCombo.Add(p.Key, p.Value);
                }
            }

            byte colorVariant = (byte)axieGenesStuff.GetAxieColorVariant(bodyStructure.primaryColors[0],bodyStructure.bodySkin, bodyStructure.@class.ToString());

            return BuildSpineAdultCombo(adultCombo, colorVariant, scale, isGraphic);
        }

        public Axie2dBuilderResult BuildSpineFromGeneCursed(string axieId, string genesStr, Dictionary<string, string> cursedMeta, float scale, bool isGraphic = false)
        {
            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);

            if (genesStr.StartsWith("0x"))
            {
                genesStr = genesStr.Substring(2);
            }
            string finalGenes512 = genesStr;
            if (finalGenes512.Length < 128)
            {
                finalGenes512 = finalGenes512.PadLeft(128, '0');
            }
            System.Numerics.BigInteger.TryParse(finalGenes512, System.Globalization.NumberStyles.HexNumber, null, out var genes);
            var bodyStructure = axieGenesStuff.GetAxieBodyStructure512(genes);

            return BuildSpineFromGeneCursed(axieId, bodyStructure, cursedMeta, scale, isGraphic);
        }

        public Axie2dBuilderResult BuildSpineFromGeneCursed(string axieId, AxieBodyStructure bodyStructure, Dictionary<string, string> cursedMeta, float scale, bool isGraphic = false)
        {
            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);
            var adultCombo = axieGenesStuff.GetAdultCombo(bodyStructure);
            if (axieId.Length < 6)
            {
                axieId = axieId.PadLeft(axieId.Length + (7 - axieId.Length) / 2);
            }
            adultCombo.Add("body-id", axieId);
            foreach (var p in cursedMeta)
            {
                adultCombo[p.Key] = p.Value;
            }

            byte colorVariant = (byte)axieGenesStuff.GetAxieColorVariant(bodyStructure.primaryColors[0],bodyStructure.bodySkin,bodyStructure.@class.ToString());

            return BuildSpineAdultCombo(adultCombo, colorVariant, scale, isGraphic);
        }

        public Axie2dBuilderResult BuildSpineAdultCombo(Dictionary<string, string> adultCombo, byte colorVariant, float scale, bool isGraphic = false)
        {
            if (adultCombo.ContainsKey("back.lv2"))
            {
                adultCombo["back"] = adultCombo["back"].Replace("-lv2", "") + "-lv2";
            }
            if (adultCombo.ContainsKey("ears.lv2"))
            {
                adultCombo["ears"] = adultCombo["ears"].Replace("-lv2", "") + "-lv2";
                adultCombo["ear"] = adultCombo["ear"].Replace("-lv2", "") + "-lv2";
            }
            if (adultCombo.ContainsKey("eyes.lv2"))
            {
                adultCombo["eyes"] = adultCombo["eyes"].Replace("-lv2", "") + "-lv2";
            }
            if (adultCombo.ContainsKey("horn.lv2"))
            {
                adultCombo["horn"] = adultCombo["horn"].Replace("-lv2", "") + "-lv2";
            }
            if (adultCombo.ContainsKey("mouth.lv2"))
            {
                adultCombo["mouth"] = adultCombo["mouth"].Replace("-lv2", "") + "-lv2";
            }
            if (adultCombo.ContainsKey("tail.lv2"))
            {
                adultCombo["tail"] = adultCombo["tail"].Replace("-lv2", "") + "-lv2";
            }

            var accessories = adultCombo.Where(x => x.Key.StartsWith("accessory-")).ToList();
            foreach (var p in accessories)
            {
                string accessorySlot = p.Key.Replace("accessory-", "body-");
                string accessoryName = p.Value.Replace("accessory-", "body-");
                adultCombo[accessorySlot] = accessoryName;
            }

            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);
            Axie2dBuilderResult builderResult = new Axie2dBuilderResult();
            builderResult.adultCombo = adultCombo;
            var axieMixerStuff = axieMixerMaterials.GetMixerStuff(AxieFormType.Normal);

            List<(BoneComboType, byte, byte)> colorVariants = new List<(BoneComboType, byte, byte)>();
            int partColorShift = axieGenesStuff.GetAxieColorPartShift(colorVariant);
            for (int i = 0; i < (int)BoneComboType.count; i++)
            {
                BoneComboType boneType = (BoneComboType)i;
                byte shiftValue = 0;
                if ((partColorShift & (1 << ((int)BoneComboType.count - i - 1))) != 0)
                {
                    shiftValue = 2;
                }
                colorVariants.Add((boneType, colorVariant, shiftValue));
            }
            var jMixed = axieMixerStuff.GenerateAssetLite(adultCombo, colorVariants, "");
            var skeletonDataAsset = CreateMixedSkeletonDataAsset(jMixed, scale, isGraphic);

            if (skeletonDataAsset == null)
            {
                builderResult.error = "GenerateAsset Failed";
            }
            else
            {
                builderResult.skeletonDataAsset = skeletonDataAsset;
                builderResult.sharedGraphicMaterial = isGraphicLinear ? axieMixerMaterials.GetSampleLinearGraphicMaterial(AxieFormType.Normal, colorVariant, 0)
                                                                      : axieMixerMaterials.GetSampleGraphicMaterial(AxieFormType.Normal);
            }
            return builderResult;
        }

        SkeletonDataAsset CreateMixedSkeletonDataAsset(MixedSkeletonData mixed, float scale, bool isGraphic)
        {
            if (mixed == null) return null;
            try
            {
                var atlasAsset = isGraphic
                    ? axieMixerMaterials.GetSingleSplatAtlasAsset(AxieFormType.Normal)
                    : axieMixerMaterials.GetFullSplatAtlasAsset(AxieFormType.Normal);
                SkeletonDataAsset skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
                skeletonDataAsset.Clear();
                skeletonDataAsset.atlasAssets = new[] { atlasAsset };
                skeletonDataAsset.scale = scale;

                var atlasArray = skeletonDataAsset.atlasAssets.Select(x => x.GetAtlas()).ToArray();
                var skeletonMixed = new SkeletonMixed(atlasArray)
                {
                    Scale = scale
                };
                var loadedSkeletonData = skeletonMixed.ReadSkeletonData(mixed, true);

                skeletonDataAsset.skeletonJSON = new TextAsset();
                Type thisType = skeletonDataAsset.GetType();
                var theMethod = thisType.GetMethod("InitializeWithData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                theMethod.Invoke(skeletonDataAsset, new object[] { loadedSkeletonData });

                return skeletonDataAsset;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
            return null;
        }
    }
}
