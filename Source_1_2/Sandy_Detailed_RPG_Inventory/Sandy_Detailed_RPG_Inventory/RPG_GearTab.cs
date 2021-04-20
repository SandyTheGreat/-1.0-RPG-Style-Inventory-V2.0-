﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Sandy_Detailed_RPG_Inventory
{
	public class Sandy_Detailed_RPG_GearTab : ITab_Pawn_Gear
	{
		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		private const float TopPadding = 20f;

		public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

		public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		private const float ThingIconSize = 28f;

		private const float ThingRowHeight = 28f;

		private const float ThingLeftX = 36f;

		private const float StandardLineHeight = 22f;

		private static List<Thing> workingInvList = new List<Thing>();

		public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

		private bool viewList = false;

		public Sandy_Detailed_RPG_GearTab()
		{
			this.size = new Vector2(Sandy_RPG_Settings.rpgTabWidth, Sandy_RPG_Settings.rpgTabHeight);
			this.labelKey = "TabGear";
			this.tutorTag = "Gear";
		}

		protected override void UpdateSize()
		{
			this.size = new Vector2(Sandy_RPG_Settings.rpgTabWidth, Sandy_RPG_Settings.rpgTabHeight);
		}

		public override bool IsVisible
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return this.ShouldShowInventory(selPawnForGear) || this.ShouldShowApparel(selPawnForGear) || this.ShouldShowEquipment(selPawnForGear);
			}
		}

		/*private bool colonist
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return !this.SelPawnForGear.RaceProps.IsMechanoid && !this.SelPawnForGear.RaceProps.Animal;
			}
		}*/

		private bool CanControl
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return !selPawnForGear.Downed && !selPawnForGear.InMentalState && (selPawnForGear.Faction == Faction.OfPlayer || selPawnForGear.IsPrisonerOfColony) && (!selPawnForGear.IsPrisonerOfColony || !selPawnForGear.Spawned || selPawnForGear.Map.mapPawns.AnyFreeColonistSpawned) && (!selPawnForGear.IsPrisonerOfColony || (!PrisonBreakUtility.IsPrisonBreaking(selPawnForGear) && (selPawnForGear.CurJob == null || !selPawnForGear.CurJob.exitMapOnArrival)));
			}
		}

		private bool CanControlColonist
		{
			get
			{
				return this.CanControl && this.SelPawnForGear.IsColonistPlayerControlled;
			}
		}

		private Pawn SelPawnForGear
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				Corpse corpse = base.SelThing as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + base.SelThing);
			}
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Rect rect0 = new Rect(20f, 0f, 100f, 30f);
			Widgets.CheckboxLabeled(rect0, "Sandy_ViewList".Translate(), ref viewList, false, null, null, false);
			Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
			Rect rect2 = rect.ContractedBy(10f);
			Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, position.width, position.height);
			Rect viewRect = new Rect(0f, 0f, position.width - 20f, this.scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
			float num = 0f;
			if (!viewList)
			{
				if (this.SelPawnForGear.RaceProps.Humanlike)
				{
					Rect rectstat = new Rect(530f, 0f, 128f, 50f);
					this.TryDrawMassInfo1(rectstat);
					this.TryDrawComfyTemperatureRange1(rectstat);
				}
				else
				{
					this.TryDrawMassInfo(ref num, viewRect.width);
					this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
				}
			}
			else if (viewList)
			{
				this.TryDrawMassInfo(ref num, viewRect.width);
				this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
			}
			if (this.ShouldShowOverallArmor(this.SelPawnForGear) && !viewList && this.SelPawnForGear.RaceProps.Humanlike)
			{
				Rect rectarmor = new Rect(530f, 84f, 128f, 85f);
				TooltipHandler.TipRegion(rectarmor, "OverallArmor".Translate());
				Rect rectsharp = new Rect(rectarmor.x, rectarmor.y, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectsharp, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorSharp_Icon", true));
				Rect rectblunt = new Rect(rectarmor.x, rectarmor.y + 30f, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectblunt, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorBlunt_Icon", true));
				Rect rectheat = new Rect(rectarmor.x, rectarmor.y + 60f, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectheat, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHeat_Icon", true));
				if (RPG_ModCheck.IsRWoMActive())
				{
					Rect rectharmony = new Rect(rectarmor.x, rectarmor.y + 90f, rectarmor.width, 27f);
					TryDrawOverallArmor1(rectharmony, RPG_ModCheck.GetHarmonyStatDef(), "RPG_Style_Inventory_ArmorHarmony".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHarmony_Icon", true));
				}
			}
			else if (this.ShouldShowOverallArmor(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "OverallArmor".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate());
			}
			if (this.IsVisible && this.SelPawnForGear.RaceProps.Humanlike && !viewList)
			{
				//Hats
				Rect newRect1 = new Rect(232f, 0f, 64f, 64f);
				GUI.DrawTexture(newRect1, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect1 = newRect1.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect1, "Sandy_Head".Translate());
				//Vests
				Rect newRect2 = new Rect(158f, 148f, 64f, 64f);
				GUI.DrawTexture(newRect2, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect2 = newRect2.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect2, "Sandy_TorsoMiddle".Translate());
				//Shirts
				Rect newRect3 = new Rect(232f, 148f, 64f, 64f);
				GUI.DrawTexture(newRect3, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect3 = newRect3.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect3, "Sandy_TorsoOnSkin".Translate());
				//Dusters
				Rect newRect4 = new Rect(306f, 148f, 64f, 64f);
				GUI.DrawTexture(newRect4, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect4 = newRect4.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect4, "Sandy_TorsoShell".Translate());
				//Belts
				Rect newRect5 = new Rect(232f, 222f, 64f, 64f);
				GUI.DrawTexture(newRect5, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect5 = newRect5.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect5, "Sandy_Belt".Translate());
				//Pants
				Rect newRect6 = new Rect(232f, 296f, 64f, 64f);
				GUI.DrawTexture(newRect6, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect6 = newRect6.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect6, "Sandy_Pants".Translate());
				Color color = new Color(1f, 1f, 1f, 1f);
				GUI.color = color;
				//Pawn
				Rect PawnRect = new Rect(530f, 172f, 128f, 128f);
				if (RPG_ModCheck.IsRWoMActive())
				{
					PawnRect.y = 202f;
				}
				this.DrawColonist(PawnRect, this.SelPawnForGear);
			}
			if (this.ShouldShowEquipment(this.SelPawnForGear) && !viewList && this.SelPawnForGear.RaceProps.Humanlike)
			{
				foreach (ThingWithComps current in this.SelPawnForGear.equipment.AllEquipmentListForReading)
				{
					if (current == this.SelPawnForGear.equipment.Primary)
					{
						Rect newRect = new Rect(562f, 296f, 64f, 64f);
						if (RPG_ModCheck.IsRWoMActive())
						{
							newRect.y = 326f;
						}
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current, false);
						if (this.SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler) && this.SelPawnForGear.equipment.Primary != null && this.SelPawnForGear.equipment.Primary.def.IsRangedWeapon)
						{
							Rect rect6 = new Rect(newRect.x, newRect.yMax - 20f, 20f, 20f);
							GUI.DrawTexture(rect6, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
							TooltipHandler.TipRegion(rect6, "BrawlerHasRangedWeapon".Translate());
						}
						if (RPG_ModCheck.IsRWoMActive() && ShouldDrawEnchantmentIcon(current))
						{
							Rect rectM = new Rect(newRect.x, newRect.yMax - 40f, 20f, 20f);
							GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
							TooltipHandler.TipRegion(rectM, RPG_ModCheck.GetEnchantmentString(current));
						}
					}
					if (current != this.SelPawnForGear.equipment.Primary)
					{
						Rect newRect1 = new Rect(562f, 370f, 64f, 64f);
						if (RPG_ModCheck.IsRWoMActive())
						{
							newRect1.y = 400f;
						}
						GUI.DrawTexture(newRect1, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect1, current, false);
						if (this.SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler) && this.SelPawnForGear.equipment.Primary != null && current.def.IsRangedWeapon)
						{
							Rect rect6 = new Rect(newRect1.x, newRect1.yMax - 20f, 20f, 20f);
							GUI.DrawTexture(rect6, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
							TooltipHandler.TipRegion(rect6, "BrawlerHasRangedWeapon".Translate());
						}
						if (RPG_ModCheck.IsRWoMActive() && ShouldDrawEnchantmentIcon(current))
						{
							Rect rectM = new Rect(newRect1.x, newRect1.yMax - 40f, 20f, 20f);
							GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
							TooltipHandler.TipRegion(rectM, RPG_ModCheck.GetEnchantmentString(current));
						}
					}
				}
			}
			else if (this.ShouldShowEquipment(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Equipment".Translate());
				foreach (ThingWithComps thing in this.SelPawnForGear.equipment.AllEquipmentListForReading)
				{
					this.DrawThingRow(ref num, viewRect.width, thing, false);
				}
			}
			if (this.ShouldShowApparel(this.SelPawnForGear) && !viewList && this.SelPawnForGear.RaceProps.Humanlike)
			{
				bool RWoMIsActive = RPG_ModCheck.IsRWoMActive();
				bool hasCloak = false;
				bool hasCape = false;
				bool hasArtifact_Neck = false;
				bool hasNeckAccessory = false;
				bool hasArtifact_LeftHand = false;
				bool hasLeftHandSkin = false;
				bool hasArtifact_RightHand = false;
				bool hasRightHandSkin = false;
				bool hasArtifact_Arms = false;
				bool hasArmsShell = false;
				int artifactCount = 0;
				if (RWoMIsActive)
				{
					hasCloak = RPG_ModCheck.HasCloak(this.SelPawnForGear);
					hasCape = RPG_ModCheck.HasCape(this.SelPawnForGear);
					hasArtifact_Neck = RPG_ModCheck.HasArtifact_Neck(this.SelPawnForGear);
					hasNeckAccessory = RPG_ModCheck.HasAccessory_Neck(this.SelPawnForGear);
					hasArtifact_LeftHand = RPG_ModCheck.HasArtifact_LeftHand(this.SelPawnForGear);
					hasLeftHandSkin = RPG_ModCheck.HasApparel_LeftHand(this.SelPawnForGear);
					hasArtifact_RightHand = RPG_ModCheck.HasArtifact_RightHand(this.SelPawnForGear);
					hasRightHandSkin = RPG_ModCheck.HasApparel_RightHand(this.SelPawnForGear);
					hasArtifact_Arms = RPG_ModCheck.HasArtifact_Arms(this.SelPawnForGear);
					hasArmsShell = RPG_ModCheck.HasApparel_Arms(this.SelPawnForGear);
				}

				foreach (Apparel current2 in this.SelPawnForGear.apparel.WornApparel)
				{
					/*switch (current2.def.apparel)
					{
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || a.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)
						&& a.layers.Contains(ApparelLayerDefOf.Overhead)):
						break;
					}*/
					switch (current2.def.apparel)
					{
						//Head - UpperHead/FullHead
						case ApparelProperties a when ((a.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || a.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
						&& a.layers.Contains(ApparelLayerDefOf.Overhead)):
							Rect newRectHe = new Rect(232f, 0f, 64f, 64f);
							this.DrawThingRow1(newRectHe, current2, false);
							break;

						//Head - Eyes
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)
						&& a.layers.Contains(ApparelLayerDefOf.Overhead)):
							Rect newRectEy = new Rect(306f, 0f, 64f, 64f);
							//GUI.DrawTexture(newRectEy, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectEy, current2, false);
							break;

						//Head - Teeth
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Teeth) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)
						&& a.layers.Contains(ApparelLayerDefOf.Overhead) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes)):
							Rect newRectTe = new Rect(158f, 0f, 64f, 64f);
							//GUI.DrawTexture(newRectTe, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectTe, current2, false);
							break;

						//Head - Ears - Accessories
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Ears) && (a.layers.Contains(Sandy_Gear_DefOf.Accessories))):
							Rect newRectEa = new Rect(84f, 0f, 64f, 64f);
							//GUI.DrawTexture(newRectEa, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectEa, current2, false);
							break;

						//Neck - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck) && !a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders)
						&& a.layers.Contains(ApparelLayerDefOf.Shell) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectNs = new Rect(232f, 74f, 64f, 64f);
							//GUI.DrawTexture(newRectNb, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectNs, current2, false);
							break;

						//Neck - TM_Cloak
						case ApparelProperties a when (RWoMIsActive && a.layers.Contains(RPG_ModCheck.GetCloakLayer())):
							Rect newRectNc = new Rect(306f, 74f, 64f, 64f);
							//GUI.DrawTexture(newRectMc, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectNc, current2, false);
							break;

						//Neck - TM_Artifact
						case ApparelProperties a when (RWoMIsActive && a.layers.Contains(RPG_ModCheck.GetArtifactLayer()) && a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck)):
							Rect newRectNmA = new Rect(158f, 74f, 64f, 64f);
							//GUI.DrawTexture(newRectMn, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectNmA, current2, false);
							break;

						//Neck - Accessories
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck) && (a.layers.Contains(Sandy_Gear_DefOf.Accessories))):
							Rect newRectNa = new Rect(84f, 74f, 64f, 64f);
							//GUI.DrawTexture(newRectNa, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectNa, current2, false);
							break;

						//Neck - Overhead
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck) && !a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders)
						&& a.layers.Contains(ApparelLayerDefOf.Overhead) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectNo = new Rect(380f, 74f, 64f, 64f);
							//GUI.DrawTexture(newRectNo, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectNo, current2, false);
							break;

						//Torso - UnderwearTop
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && a.layers.Contains(Sandy_Gear_DefOf.UnderwearTop)):
							Rect newRectTu = new Rect(10f, 148f, 64f, 64f);
							this.DrawThingRow1(newRectTu, current2, false);
							break;

						//Torso - Belt
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && a.layers.Contains(ApparelLayerDefOf.Belt)):
							Rect newRectTb = new Rect(84f, 148f, 64f, 64f);
							this.DrawThingRow1(newRectTb, current2, false);
							break;

						//Torso - Middle
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && !a.layers.Contains(ApparelLayerDefOf.Shell)
						&& a.layers.Contains(ApparelLayerDefOf.Middle)):
							Rect newRectTm = new Rect(158f, 148f, 64f, 64f);
							this.DrawThingRow1(newRectTm, current2, false);
							break;

						//Torso - OnSkin
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && a.layers.Contains(ApparelLayerDefOf.OnSkin)
						&& !a.layers.Contains(ApparelLayerDefOf.Middle) && !a.layers.Contains(ApparelLayerDefOf.Shell)):
							Rect newRectTo = new Rect(232f, 148f, 64f, 64f);
							this.DrawThingRow1(newRectTo, current2, false);
							break;

						//Torso - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && a.layers.Contains(ApparelLayerDefOf.Shell)):
							Rect newRectTs = new Rect(306f, 148f, 64f, 64f);
							this.DrawThingRow1(newRectTs, current2, false);
							break;

						//Shoulders - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders) && a.layers.Contains(ApparelLayerDefOf.Shell)
						&& !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand)
						&& !a.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand)):
							Rect newRectSs = new Rect(380f, 148f, 64f, 64f);
							//GUI.DrawTexture(newRectSs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectSs, current2, false);
							break;

						//Waist - Underwear
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Waist) && a.layers.Contains(Sandy_Gear_DefOf.Underwear)
						&& !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectWu = new Rect(10f, 222f, 64f, 64f);
							this.DrawThingRow1(newRectWu, current2, false);
							break;

						//Waist - Belt
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Waist) && a.layers.Contains(ApparelLayerDefOf.Belt)):
							Rect newRectWb = new Rect(232f, 222f, 64f, 64f);
							this.DrawThingRow1(newRectWb, current2, false);
							break;

						//Waist - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Waist) && a.layers.Contains(ApparelLayerDefOf.Shell)):
							Rect newRectWs = new Rect(306f, 222f, 64f, 64f);
							//GUI.DrawTexture(newRectWs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectWs, current2, false);
							break;

						//Legs - Underwear
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && a.layers.Contains(Sandy_Gear_DefOf.Underwear)):
							Rect newRectLu = new Rect(10f, 296f, 64f, 64f);
							this.DrawThingRow1(newRectLu, current2, false);
							break;

						//Legs - Middle
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && a.layers.Contains(ApparelLayerDefOf.Middle)
						&& !a.layers.Contains(ApparelLayerDefOf.Shell) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectLm = new Rect(158f, 296f, 64f, 64f);
							//GUI.DrawTexture(newRectLm, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectLm, current2, false);
							break;

						//Legs - OnSkin
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !a.layers.Contains(ApparelLayerDefOf.Middle)
						&& a.layers.Contains(ApparelLayerDefOf.OnSkin) && !a.layers.Contains(ApparelLayerDefOf.Shell)
						&& !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectLo = new Rect(232f, 296f, 64f, 64f);
							this.DrawThingRow1(newRectLo, current2, false);
							break;

						//Legs - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && a.layers.Contains(ApparelLayerDefOf.Shell)
						&& !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectLs = new Rect(306f, 296f, 64f, 64f);
							//GUI.DrawTexture(newRectLs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectLs, current2, false);
							break;

						//Feet - Middle
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Feet) && !a.layers.Contains(ApparelLayerDefOf.Shell)
						&& a.layers.Contains(ApparelLayerDefOf.Middle) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)):
							Rect newRectFm = new Rect(158f, 370f, 64f, 64f);
							//GUI.DrawTexture(newRectFm, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectFm, current2, false);
							break;

						//Feet - OnSkin
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Feet) && a.layers.Contains(ApparelLayerDefOf.OnSkin)
						&& !a.layers.Contains(ApparelLayerDefOf.Shell) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)
						&& !a.layers.Contains(ApparelLayerDefOf.Middle)):
							Rect newRectFo = new Rect(232f, 370f, 64f, 64f);
							//GUI.DrawTexture(newRectFo, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectFo, current2, false);
							break;

						//Feet - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Feet) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)
						&& (a.layers.Contains(ApparelLayerDefOf.Shell) || a.layers.Contains(ApparelLayerDefOf.Overhead))):
							Rect newRectFs = new Rect(306f, 370f, 64f, 64f);
							//GUI.DrawTexture(newRectFs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectFs, current2, false);
							break;

						//Hands - OnSkin
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
						&& a.layers.Contains(ApparelLayerDefOf.OnSkin) && !a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders)
						&& !a.layers.Contains(ApparelLayerDefOf.Middle) && !a.layers.Contains(ApparelLayerDefOf.Shell)):
							Rect newRectHo = new Rect(454f, 74f, 64f, 64f);
							//GUI.DrawTexture(newRectHo, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectHo, current2, false);
							break;

						//Hands - Middle
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
						&& a.layers.Contains(ApparelLayerDefOf.Middle) && !a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders)
						&& !a.layers.Contains(ApparelLayerDefOf.Shell)):
							Rect newRectHm = new Rect(454f, 148f, 64f, 64f);
							//GUI.DrawTexture(newRectHm, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectHm, current2, false);
							break;

						//Hands - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
						&& (a.layers.Contains(ApparelLayerDefOf.Shell) || a.layers.Contains(ApparelLayerDefOf.Overhead))):
							Rect newRectHs = new Rect(454f, 222f, 64f, 64f);
							//GUI.DrawTexture(newRectHs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectHs, current2, false);
							break;

						//Arms - TM_Artifact
						case ApparelProperties a when (RWoMIsActive && a.layers.Contains(RPG_ModCheck.GetArtifactLayer()) && a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Arms)):
							Rect newRectAmA = new Rect(454f, 296f, 64f, 64f);
							//GUI.DrawTexture(newRectMa, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectAmA, current2, false);
							break;

						//RightHand - Accessories
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand) && (a.layers.Contains(Sandy_Gear_DefOf.Accessories))):
							Rect newRectRHa = new Rect(380f, 222f, 64f, 64f);
							//GUI.DrawTexture(newRectRHa, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectRHa, current2, false);
							break;

						//RightHand - TM_Artifact
						case ApparelProperties a when (RWoMIsActive && a.layers.Contains(RPG_ModCheck.GetArtifactLayer()) && a.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand)):
							Rect newRectRHmA = new Rect(380f, 296f, 64f, 64f);
							//GUI.DrawTexture(newRectMRHo, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectRHmA, current2, false);
							break;

						//RightHand - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand) && !a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
						&& a.layers.Contains(ApparelLayerDefOf.Shell) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectRHs = new Rect(380f, 370f, 64f, 64f);
							//GUI.DrawTexture(newRectRHs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectRHs, current2, false);
							break;

						//LeftHand - Accessories
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && (a.layers.Contains(Sandy_Gear_DefOf.Accessories))):
							Rect newRectLHa = new Rect(84f, 222f, 64f, 64f);
							//GUI.DrawTexture(newRectLHa, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectLHa, current2, false);
							break;

						//LeftHand - TM_Artifact
						case ApparelProperties a when (RWoMIsActive && a.layers.Contains(RPG_ModCheck.GetArtifactLayer()) && a.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand)):
							Rect newRectLHmA = new Rect(84f, 296f, 64f, 64f);
							//GUI.DrawTexture(newRectMLHo, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectLHmA, current2, false);
							break;

						//LeftHand - Shell
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && !a.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
						&& a.layers.Contains(ApparelLayerDefOf.Shell) && !a.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)):
							Rect newRectLHs = new Rect(84f, 370f, 64f, 64f);
							//GUI.DrawTexture(newRectLHs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectLHs, current2, false);
							break;

						//LeftHand - Shield
						case ApparelProperties a when (a.layers.Contains(Sandy_Gear_DefOf.Shield)):
							Rect newRectLHsh = new Rect(158f, 222f, 64f, 64f);
							//GUI.DrawTexture(newRectLs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectLHsh, current2, false);
							break;

						//LeftHand - VFEC_OuterShell
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && a.layers.Contains(Sandy_Gear_DefOf.VFEC_OuterShell)):
							Rect newRectLHos = new Rect(10f, 370f, 64f, 64f);
							//GUI.DrawTexture(newRectLs, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
							this.DrawThingRow1(newRectLHos, current2, false);
							break;

							/*case ApparelProperties a when (RWoMIsActive && a.layers.Contains(RPG_ModCheck.GetArtifactLayer())):  //Draws an incremental artifact slots below everything else if not identified previously
								artifactCount++;
								Rect newRectMAother = new Rect(10f, GetArtifactY(artifactCount), 64f, 64f);
								//GUI.DrawTexture(newRectMAother, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
								this.DrawThingRow1(newRectMAother, current2, false);
								break;*/
					}
				}
			}
			else if (this.ShouldShowApparel(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Apparel".Translate());
				foreach (Apparel thing2 in from ap in this.SelPawnForGear.apparel.WornApparel
										   orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
										   select ap)
				{
					this.DrawThingRow(ref num, viewRect.width, thing2, false);
				}
			}
			if (this.ShouldShowInventory(this.SelPawnForGear))
			{
				if (this.SelPawnForGear.RaceProps.Humanlike && !viewList)
				{
					num = 440f;
					if (RPG_ModCheck.IsRWoMActive())
					{
						num = 450f;
					}
				}
				else if (!this.SelPawnForGear.RaceProps.Humanlike && !viewList)
				{
					num = 44f;
				}
				Widgets.ListSeparator(ref num, viewRect.width, "Inventory".Translate());
				Sandy_Detailed_RPG_GearTab.workingInvList.Clear();
				Sandy_Detailed_RPG_GearTab.workingInvList.AddRange(this.SelPawnForGear.inventory.innerContainer);
				for (int i = 0; i < Sandy_Detailed_RPG_GearTab.workingInvList.Count; i++)
				{
					this.DrawThingRow(ref num, viewRect.width, Sandy_Detailed_RPG_GearTab.workingInvList[i], true);
				}
				Sandy_Detailed_RPG_GearTab.workingInvList.Clear();
			}
			if (Event.current.type == EventType.Layout)
			{
				this.scrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawColonist(Rect rect, Pawn pawn)
		{
			Vector2 pos = new Vector2(rect.width, rect.height);
			GUI.DrawTexture(rect, PortraitsCache.Get(pawn, pos, PawnTextureCameraOffset, 1.18f));
		}

		private void DrawThingRow1(Rect rect, Thing thing, bool inventory = false)
		{
			QualityCategory c;
			if (thing.TryGetQuality(out c))
			{
				switch (c)
				{
					case QualityCategory.Legendary:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Legendary", true));
							break;
						}
					case QualityCategory.Masterwork:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Masterwork", true));
							break;
						}
					case QualityCategory.Excellent:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Excellent", true));
							break;
						}
					case QualityCategory.Good:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Good", true));
							break;
						}
					case QualityCategory.Normal:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Normal", true));
							break;
						}
					case QualityCategory.Poor:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Poor", true));
							break;
						}
					case QualityCategory.Awful:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Awful", true));
							break;
						}
				}
			}
			float mass = thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount;
			string smass = mass.ToString("G") + " kg";
			string text = thing.LabelCap;
			Rect rect5 = rect.ContractedBy(2f);
			float num2 = rect5.height * ((float)thing.HitPoints / (float)thing.MaxHitPoints);
			rect5.yMin = rect5.yMax - num2;
			rect5.height = num2;
			GUI.DrawTexture(rect5, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Not_Tattered"));
			if ((float)thing.HitPoints <= ((float)thing.MaxHitPoints / 2))
			{
				Rect tattered = rect5;
				GUI.DrawTexture(tattered, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tattered"));
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Rect rect1 = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
				Widgets.ThingIcon(rect1, thing, 1f);
			}
			bool flag = false;
			if (Mouse.IsOver(rect))
			{
				GUI.color = Sandy_Detailed_RPG_GearTab.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
				Widgets.InfoCardButton(rect.x, rect.y, thing);
				if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
				{
					Rect rect2 = new Rect(rect.xMax - 24f, rect.y, 24f, 24f);
					bool flag2 = this.SelPawnForGear.IsQuestLodger() && !(thing is Apparel);
					Apparel apparel;
					bool flag3 = (apparel = (thing as Apparel)) != null && this.SelPawnForGear.apparel != null && this.SelPawnForGear.apparel.IsLocked(apparel);
					flag = (flag2 || flag3);
					if (Mouse.IsOver(rect2))
					{
						if (flag3)
						{
							TooltipHandler.TipRegion(rect2, "DropThingLocked".Translate());
						}
						else if (flag2)
						{
							TooltipHandler.TipRegion(rect2, "DropThingLodger".Translate());
						}
						else
						{
							TooltipHandler.TipRegion(rect2, "DropThing".Translate());
						}
					}
					Color color = flag ? Color.grey : Color.white;
					Color mouseoverColor = flag ? color : GenUI.MouseoverColor;
					if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true), color, mouseoverColor, !flag) && !flag)
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						this.InterfaceDrop(thing);
					}
				}
			}
			Apparel apparel2 = thing as Apparel;
			if (apparel2 != null && this.SelPawnForGear.outfits != null && apparel2.WornByCorpse)
			{
				Rect rect3 = new Rect(rect.xMax - 20f, rect.yMax - 20f, 20f, 20f);
				GUI.DrawTexture(rect3, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tainted_Icon", true));
				TooltipHandler.TipRegion(rect3, "WasWornByCorpse".Translate());
			}
			if (apparel2 != null && this.SelPawnForGear.outfits != null && this.SelPawnForGear.outfits.forcedHandler.IsForced(apparel2))
			{
				text += ", " + "ApparelForcedLower".Translate();
				Rect rect4 = new Rect(rect.x, rect.yMax - 20f, 20f, 20f);
				GUI.DrawTexture(rect4, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
				TooltipHandler.TipRegion(rect4, "ForcedApparel".Translate());
			}
			if (apparel2 != null && this.SelPawnForGear.outfits != null && RPG_ModCheck.IsRWoMActive() && ShouldDrawEnchantmentIcon(apparel2))
			{
				Rect rectM = new Rect(rect.x, rect.yMax - 40f, 20f, 20f);
				GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
				TooltipHandler.TipRegion(rectM, RPG_ModCheck.GetEnchantmentString(apparel2));
			}
			if (flag)
			{
				text += " (" + "ApparelLockedLower".Translate() + ")";
			}
			Text.WordWrap = true;
			string text2 = thing.DescriptionDetailed;
			string text3 = text + "\n" + text2 + "\n" + smass;
			if (thing.def.useHitPoints)
			{
				string text4 = text3;
				text3 = string.Concat(new object[]
				{
					text4,
					"\n",
					thing.HitPoints,
					" / ",
					thing.MaxHitPoints
				});
			}
			TooltipHandler.TipRegion(rect, text3);
		}

		public void TryDrawOverallArmor1(Rect rect, StatDef stat, string label, Texture image)
		{
			float num = 0f;
			float num2 = Mathf.Clamp01(this.SelPawnForGear.GetStatValue(stat, true) / 2f);
			List<BodyPartRecord> allParts = this.SelPawnForGear.RaceProps.body.AllParts;
			List<Apparel> list = (this.SelPawnForGear.apparel == null) ? null : this.SelPawnForGear.apparel.WornApparel;
			for (int i = 0; i < allParts.Count; i++)
			{
				float num3 = 1f - num2;
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].def.apparel.CoversBodyPart(allParts[i]))
						{
							float num4 = Mathf.Clamp01(list[j].GetStatValue(stat, true) / 2f);
							num3 *= 1f - num4;
						}
					}
				}
				num += allParts[i].coverageAbs * (1f - num3);
			}
			num = Mathf.Clamp(num * 2f, 0f, 2f);
			Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
			GUI.DrawTexture(rect1, image);
			TooltipHandler.TipRegion(rect1, label);
			Rect rect2 = new Rect(rect.x + 60f, rect.y + 3f, 104f, 24f);
			Widgets.Label(rect2, num.ToStringPercent());
		}

		private void TryDrawMassInfo1(Rect rect)
		{
			if (this.SelPawnForGear.Dead || !this.ShouldShowInventory(this.SelPawnForGear))
			{
				return;
			}
			Rect rect1 = new Rect(rect.x, rect.y, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_MassCarried_Icon", true));
			TooltipHandler.TipRegion(rect1, "SandyMassCarried".Translate());
			float num = MassUtility.GearAndInventoryMass(this.SelPawnForGear);
			float num2 = MassUtility.Capacity(this.SelPawnForGear, null);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 2f, 104f, 24f);
			Widgets.Label(rect2, "SandyMassValue".Translate(num.ToString("0.##"), num2.ToString("0.##")));
		}

		private void TryDrawComfyTemperatureRange1(Rect rect)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Min_Temperature", true));
			TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 28f, 104f, 24f);
			Widgets.Label(rect2, string.Concat(new string[]
			{
				" ",
				statValue.ToStringTemperature("F0")
			}));

			rect1 = new Rect(rect.x, rect.y + 52f, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Max_Temperature", true));
			TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
			float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
			rect2 = new Rect(rect.x + 30f, rect.y + 56f, 104f, 24f);
			Widgets.Label(rect2, string.Concat(new string[]
			{
				" ",
				statValue2.ToStringTemperature("F0")
			}));
		}

		private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
		{
			Rect rect = new Rect(0f, y, width, 28f);
			Widgets.InfoCardButton(rect.width - 24f, y, thing);
			rect.width -= 24f;
			bool flag = false;
			if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
			{
				Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
				bool flag2 = false;
				if (this.SelPawnForGear.IsQuestLodger())
				{
					flag2 = (inventory || !EquipmentUtility.QuestLodgerCanUnequip(thing, this.SelPawnForGear));
				}
				Apparel apparel;
				bool flag3 = (apparel = (thing as Apparel)) != null && this.SelPawnForGear.apparel != null && this.SelPawnForGear.apparel.IsLocked(apparel);
				flag = (flag2 || flag3);
				if (Mouse.IsOver(rect2))
				{
					if (flag3)
					{
						TooltipHandler.TipRegion(rect2, "DropThingLocked".Translate());
					}
					else if (flag2)
					{
						TooltipHandler.TipRegion(rect2, "DropThingLodger".Translate());
					}
					else
					{
						TooltipHandler.TipRegion(rect2, "DropThing".Translate());
					}
				}
				Color color = flag ? Color.grey : Color.white;
				Color mouseoverColor = flag ? color : GenUI.MouseoverColor;
				if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true), color, mouseoverColor, !flag) && !flag)
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					this.InterfaceDrop(thing);
				}
				rect.width -= 24f;
			}
			if (this.CanControlColonist)
			{
				if (FoodUtility.WillIngestFromInventoryNow(this.SelPawnForGear, thing))
				{
					Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
					TooltipHandler.TipRegionByKey(rect3, "ConsumeThing", thing.LabelNoCount, thing);
					if (Widgets.ButtonImage(rect3, ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true), true))
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						FoodUtility.IngestFromInventoryNow(this.SelPawnForGear, thing);
					}
				}
				rect.width -= 24f;
			}
			Rect rect4 = rect;
			rect4.xMin = rect4.xMax - 60f;
			CaravanThingsTabUtility.DrawMass(thing, rect4);
			rect.width -= 60f;
			if (Mouse.IsOver(rect))
			{
				GUI.color = ITab_Pawn_Gear.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = ITab_Pawn_Gear.ThingLabelColor;
			Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
			string text = thing.LabelCap;
			Apparel apparel2 = thing as Apparel;
			if (apparel2 != null && this.SelPawnForGear.outfits != null && this.SelPawnForGear.outfits.forcedHandler.IsForced(apparel2))
			{
				text += ", " + "ApparelForcedLower".Translate();
			}
			if (flag)
			{
				text += " (" + "ApparelLockedLower".Translate() + ")";
			}
			Text.WordWrap = false;
			Widgets.Label(rect5, text.Truncate(rect5.width, null));
			Text.WordWrap = true;
			if (Mouse.IsOver(rect))
			{
				string text2 = thing.DescriptionDetailed;
				if (thing.def.useHitPoints)
				{
					text2 = string.Concat(new object[]
					{
						text2,
						"\n",
						thing.HitPoints,
						" / ",
						thing.MaxHitPoints
					});
				}
				TooltipHandler.TipRegion(rect, text2);
			}
			y += 28f;
		}

		private void TryDrawOverallArmor(ref float curY, float width, StatDef stat, string label)
		{
			float num = 0f;
			float num2 = Mathf.Clamp01(this.SelPawnForGear.GetStatValue(stat, true) / 2f);
			List<BodyPartRecord> allParts = this.SelPawnForGear.RaceProps.body.AllParts;
			List<Apparel> list = (this.SelPawnForGear.apparel == null) ? null : this.SelPawnForGear.apparel.WornApparel;
			for (int i = 0; i < allParts.Count; i++)
			{
				float num3 = 1f - num2;
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].def.apparel.CoversBodyPart(allParts[i]))
						{
							float num4 = Mathf.Clamp01(list[j].GetStatValue(stat, true) / 2f);
							num3 *= 1f - num4;
						}
					}
				}
				num += allParts[i].coverageAbs * (1f - num3);
			}
			num = Mathf.Clamp(num * 2f, 0f, 2f);
			Rect rect = new Rect(0f, curY, width, 100f);
			Widgets.Label(rect, label.Truncate(120f, null));
			rect.xMin += 120f;
			Widgets.Label(rect, num.ToStringPercent());
			curY += 22f;
		}

		private void TryDrawMassInfo(ref float curY, float width)
		{
			if (this.SelPawnForGear.Dead || !this.ShouldShowInventory(this.SelPawnForGear))
			{
				return;
			}
			Rect rect = new Rect(0f, curY, width, 22f);
			float num = MassUtility.GearAndInventoryMass(this.SelPawnForGear);
			float num2 = MassUtility.Capacity(this.SelPawnForGear, null);
			Widgets.Label(rect, "MassCarried".Translate(num.ToString("0.##"), num2.ToString("0.##")));
			curY += 22f;
		}

		private void TryDrawComfyTemperatureRange(ref float curY, float width)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect = new Rect(0f, curY, width, 22f);
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
			Widgets.Label(rect, string.Concat(new string[]
			{
				"ComfyTemperatureRange".Translate(),
				": ",
				statValue.ToStringTemperature("F0"),
				" ~ ",
				statValue2.ToStringTemperature("F0")
			}));
			curY += 22f;
		}

		private void InterfaceDrop(Thing t)
		{
			ThingWithComps thingWithComps = t as ThingWithComps;
			Apparel apparel = t as Apparel;
			if (apparel != null && this.SelPawnForGear.apparel != null && this.SelPawnForGear.apparel.WornApparel.Contains(apparel))
			{
				this.SelPawnForGear.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel), JobTag.Misc);
			}
			else if (thingWithComps != null && this.SelPawnForGear.equipment != null && this.SelPawnForGear.equipment.AllEquipmentListForReading.Contains(thingWithComps))
			{
				this.SelPawnForGear.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps), JobTag.Misc);
			}
			else if (!t.def.destroyOnDrop)
			{
				Thing thing;
				this.SelPawnForGear.inventory.innerContainer.TryDrop(t, this.SelPawnForGear.Position, this.SelPawnForGear.Map, ThingPlaceMode.Near, out thing, null, null);
			}
		}

		private void InterfaceIngest(Thing t)
		{
			Job job = new Job(JobDefOf.Ingest, t);
			job.count = Mathf.Min(t.stackCount, t.def.ingestible.maxNumToIngestAtOnce);
			job.count = Mathf.Min(job.count, FoodUtility.WillIngestStackCountOf(this.SelPawnForGear, t.def, t.GetStatValue(StatDefOf.Nutrition, true)));
			this.SelPawnForGear.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}

		private bool ShouldShowInventory(Pawn p)
		{
			return p.RaceProps.Humanlike || p.inventory.innerContainer.Any;
		}

		private bool ShouldShowApparel(Pawn p)
		{
			return p.apparel != null && (p.RaceProps.Humanlike || p.apparel.WornApparel.Any<Apparel>());
		}

		private bool ShouldShowEquipment(Pawn p)
		{
			return p.equipment != null;
		}

		private bool ShouldShowOverallArmor(Pawn p)
		{
			return p.RaceProps.Humanlike || this.ShouldShowApparel(p) || p.GetStatValue(StatDefOf.ArmorRating_Sharp, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Blunt, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Heat, true) > 0f;
		}

		private bool ShouldDrawEnchantmentIcon(Thing item)
		{
			bool isEnchanted = false;
			TorannMagic.Enchantment.CompEnchantedItem enchantedItem = item.TryGetComp<TorannMagic.Enchantment.CompEnchantedItem>();
			if (enchantedItem != null && enchantedItem.HasEnchantment)
			{
				isEnchanted = true;
			}
			return isEnchanted;
		}

		private float GetArtifactY(int num)
		{
			return (378f + (63f * num));
		}
	}
}