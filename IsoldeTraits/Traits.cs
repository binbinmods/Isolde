using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Isolde.CustomFunctions;
using static Isolde.Plugin;
using static Isolde.DescriptionFunctions;
using static Isolde.CharacterFunctions;
// using static Obeliskial_Essentials.CustomFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Isolde
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }

            if (_trait == trait0)
            {
                // At the start of your turn, apply 1 Sharp, 4 Chill, 4 Insane to all heroes and monsters.
                ApplyAuraCurseToAll("sharp", 1, AppliesTo.Global, _character, useCharacterMods: true);
                ApplyAuraCurseToAll("chill", 4, AppliesTo.Global, _character, useCharacterMods: true);
                ApplyAuraCurseToAll("insane", 4, AppliesTo.Global, _character, useCharacterMods: true);
            }


            else if (_trait == trait2a)
            {
                // trait2a
                // When you play a Song, reduce the cost of the highest cost Elemental Spell card by 1 until discarded.
                //  When you play an Elemental Spell, reduce the cost of the highest cost Song by 1 until discarded. (3 times/turn) 
                string traitName = traitData.TraitName;
                string traitId = _trait;
                int bonusActivations = _character.HaveTrait(trait4b) ? 1 : 0;

                DualityCardType(ref _character, ref _castedCard, [Enums.CardType.Song], [Enums.CardType.Fire_Spell, Enums.CardType.Cold_Spell, Enums.CardType.Lightning_Spell], traitId, bonusActivations);

            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // Sharp +1. Sharp on enemies reduces All Resistances by 1% per charge.
                // Handled in GACM
                string traitName = traitData.TraitName;
                string traitId = _trait;

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // Once per turn, when you play a Cold Spell, add a randomly upgraded Last Requiem to your hand. Cost 0 and Vanish.

                string traitName = traitData.TraitName;
                string traitId = _trait;

                if (CanIncrementTraitActivations(traitId) && _castedCard.HasCardType(Enums.CardType.Cold_Spell))
                {
                    if (!((UnityEngine.Object)MatchManager.Instance != (UnityEngine.Object)null) || !((UnityEngine.Object)_castedCard != (UnityEngine.Object)null))
                        return;
                    AddCardToHand("lastrequiem", randomlyUpgraded: true);
                    IncrementTraitActivations(traitId);
                    ProgressStanza(_character);
                }

                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                // Buffer +1, Fast +1. Sharp on Monsters reduces all damage by 0.5 per charge. Soprano Duality can be activated an additional time.
                // Handled in GACM
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            bool hasRust = characterOfInterest.EffectCharges("rust") >= 0;
            switch (_acId)
            {

                // trait2b:
                // Sharp on enemies reduces All Resistances by 1% per charge.

                // trait 4b:
                // Sharp on Monsters reduces All Damage by 0.5 per charge

                case "sharp":
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.ResistModified = Enums.DamageType.All;
                        __result.ResistModifiedPercentagePerStack = hasRust ? -4.5f : -3;
                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.AuraDamageType4 = Enums.DamageType.All;
                        __result.AuraDamageIncreasedPerStack4 = hasRust ? -0.75f : -0.5f;
                    }
                    traitOfInterest = trait4a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.ConsumedAtTurn = false;
                        __result.AuraConsumed = 0;
                    }

                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        public static void HealAuraCursePrefix(ref Character __instance, AuraCurseData AC, ref int __state)
        {
            LogInfo($"HealAuraCursePrefix {subclassName}");
            string traitOfInterest = trait4b;
            if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth"))
            {
                __state = Mathf.FloorToInt(__instance.GetAuraCharges("stealth") * 0.25f);
                // __instance.SetAuraTrait(null, "stealth", 1);

            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        public static void HealAuraCursePostfix(ref Character __instance, AuraCurseData AC, int __state)
        {
            LogInfo($"HealAuraCursePrefix {subclassName}");
            string traitOfInterest = trait4b;
            if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth") && __state > 0)
            {
                // __state = __instance.GetAuraCharges("stealth");
                __instance.SetAuraTrait(null, "stealth", __state);
            }

        }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardData), nameof(CardData.SetDescriptionNew))]
        public static void SetDescriptionNewPostfix(ref CardData __instance, bool forceDescription = false, Character character = null, bool includeInSearch = true)
        {
            // LogInfo("executing SetDescriptionNewPostfix");
            if (__instance == null)
            {
                LogDebug("Null Card");
                return;
            }
            if (!Globals.Instance.CardsDescriptionNormalized.ContainsKey(__instance.Id))
            {
                LogError($"missing card Id {__instance.Id}");
                return;
            }


            if (__instance.CardName == "Mind Maze")
            {
                StringBuilder stringBuilder1 = new StringBuilder();
                LogDebug($"Current description for {__instance.Id}: {stringBuilder1}");
                string currentDescription = Globals.Instance.CardsDescriptionNormalized[__instance.Id];
                stringBuilder1.Append(currentDescription);
                // stringBuilder1.Replace($"When you apply", $"When you play a Mind Spell\n or apply");
                stringBuilder1.Replace($"Lasts one turn", $"Lasts two turns");
                BinbinNormalizeDescription(ref __instance, stringBuilder1);
            }
        }

    }
}

