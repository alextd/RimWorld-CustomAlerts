using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using TD_Find_Lib;

namespace Custom_Alerts
{
	public class QuerySearchAlert : QuerySearch
	{
		public Alert_QuerySearch alert;

		public QuerySearchAlert() : base()
		{
			alert = new Alert_QuerySearch(this);
		}

		public QuerySearchAlert(QuerySearch search) : base()
		{
			name = search.name;
			parameters = search.parameters.Clone();
			active = true;

			// If you loaded from a search that chose the map, but didn't choose, I guess we'll choose for you.
			if (parameters.mapType == SearchMapType.ChosenMaps && parameters.searchMaps.Count == 0)
				SetSearchMap(Find.CurrentMap, false);

			children = search.Children.Clone(this);

			alert = new (this);
			alert.defaultLabel = name;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.LoadingVars)
				alert.defaultLabel = name;

			alert.ExposeData();
		}

		public void Rename(string newName)
		{
			name = newName;
			alert.defaultLabel = newName;
		}
		public void SetPriority(AlertPriority p)
		{
			alert.defaultPriority = p;
		}
	}


	public enum CompareType { Greater, Equal, Less }
	public class Alert_QuerySearch : Alert
	{
		public QuerySearchAlert searchAlert;

		public int secondsBeforeAlert;
		public int countToAlert;
		public CompareType compareType;

		public int maxItems = 16;
		int lastTickInactive;

		public static bool enableAll = true;

		public Alert_QuerySearch()
		{
			//The vanilla alert added to AllAlerts will be constructed but never be active with null filter
		}

		public Alert_QuerySearch(QuerySearchAlert searchAlert) : this()
		{
			this.searchAlert = searchAlert;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref defaultPriority, "alertPriority");
			Scribe_Values.Look(ref secondsBeforeAlert, "ticksToShowAlert");
			Scribe_Values.Look(ref countToAlert, "countToAlert");
			Scribe_Values.Look(ref compareType, "countComp");
		}
		
		//protected but using publicized assembly
		//protected override Color BGColor
		public override Color BGColor
		{
			get
			{
				if (defaultPriority != AlertPriority.Critical) return base.BGColor;
				float i = Pulser.PulseBrightness(Alert_Critical.PulseFreq, Pulser.PulseBrightness(Alert_Critical.PulseFreq, Alert_Critical.PulseAmpCritical));
				return new Color(i, i, i) * Color.red;
			}
		}
		
		public override AlertReport GetReport()
		{
			if (searchAlert == null || !enableAll)	//Alert_Find auto-added as an Alert subclass, exists but never displays anything
				return AlertReport.Inactive;

			var things = FoundThings();
			int count = things.Sum(t => t.stackCount);
			bool active = compareType switch
			{
				CompareType.Greater => count > countToAlert,
				CompareType.Equal => count == countToAlert,
				CompareType.Less => count < countToAlert,
				_ => false
			};

			if (!active)
				lastTickInactive = Find.TickManager.TicksGame;
			else if (Find.TickManager.TicksGame - lastTickInactive >= secondsBeforeAlert * GenTicks.TicksPerRealSecond)
			{
				if (count == 0)
					return AlertReport.Active;
				return AlertReport.CulpritsAre(things.Take(maxItems).ToList());
			}
			return AlertReport.Inactive;
		}

		public override TaggedString GetExplanation()
		{
			var things = FoundThings();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(defaultLabel + searchAlert.GetMapNameSuffix());
			stringBuilder.AppendLine(" - " + ThingListDrawer.LabelCountThings(things));
			stringBuilder.AppendLine("");
			foreach (Thing thing in things.Take(maxItems))
				stringBuilder.AppendLine("   " + thing.Label);
			if (things.Count() > maxItems)
				stringBuilder.AppendLine("TD.Maximum0Displayed".Translate(maxItems));
			stringBuilder.AppendLine("");
			stringBuilder.AppendLine("TD.Right-clickToOpenFindTab".Translate());

			return stringBuilder.ToString().TrimEndNewlines();
		}

		int currentTick;
		private IEnumerable<Thing> FoundThings()
		{
			if (Find.TickManager.TicksGame == currentTick)
				return searchAlert.result.allThings;

			currentTick = Find.TickManager.TicksGame;

			searchAlert.RemakeList();


			return searchAlert.result.allThings;
		}

		public override Rect DrawAt(float topY, bool minimized)
		{
			Text.Font = GameFont.Small;
			string label = GetLabel();
			float height = Text.CalcHeight(label, Alert.Width - 6); //Alert.TextWidth = 148f
			Rect rect = new Rect((float)UI.screenWidth - Alert.Width, topY, Alert.Width, height);
			//if (this.alertBounce != null)
			//rect.x -= this.alertBounce.CalculateHorizontalOffset();
			if (Event.current.button == 1 && Widgets.ButtonInvisible(rect, false))
			{
				SearchStorage.ChooseExportSearch(searchAlert, "Custom Alert");

				Event.current.Use();
			}
			return base.DrawAt(topY, minimized);
		}
	}
}
