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
	public class QuerySearchAlert: IExposable, IQuerySearch
	{
		public QuerySearch search;
		public QuerySearch Search => search;
		public Alert_QuerySearch alert;

		//For ExposeData
		public QuerySearchAlert() : base()
		{
			alert = new Alert_QuerySearch(this);
		}

		public QuerySearchAlert(QuerySearch search, bool enabled = true) : base()
		{
			this.search = search;
			this.search.active = true;

			alert = new (this);
			alert.enabled = enabled;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref search, "search");

			alert.ExposeData();
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

		public bool enabled = true;
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
			Scribe_Values.Look(ref enabled, "enabled");
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

		public override string GetLabel() => searchAlert.search.name;

		public override AlertReport GetReport()
		{
			if (!enabled || searchAlert == null || !enableAll)	//Alert_Find auto-added as an Alert subclass, exists but never displays anything
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
			stringBuilder.Append(GetLabel() + searchAlert.search.GetMapNameSuffix());
			stringBuilder.AppendLine(" - " + ThingListDrawer.LabelCountThings(things));
			stringBuilder.AppendLine("");
			foreach (Thing thing in things.Take(maxItems))
				stringBuilder.AppendLine("   " + thing.Label);
			if (things.Count() > maxItems)
				stringBuilder.AppendLine("TD.Maximum0Displayed".Translate(maxItems));
			stringBuilder.AppendLine("");
			stringBuilder.AppendLine("TD.ClickToOpen".Translate());

			return stringBuilder.ToString().TrimEndNewlines();
		}

		int lastRemadeTick;
		private IEnumerable<Thing> FoundThings()
		{
			if (Find.TickManager.TicksGame == lastRemadeTick)
				return searchAlert.search.result.allThings;

			lastRemadeTick = Find.TickManager.TicksGame;

			searchAlert.search.RemakeList();


			return searchAlert.search.result.allThings;
		}

		public override void OnClick()
		{
			if (Event.current.button == 1)
			{
				List<FloatMenuOption> options = SearchStorage.ExportSearchOptions(searchAlert.search, SearchAlertTransfer.TransferTag);

				options.Add(new FloatMenuOption("TD.OpenManager".Translate(), () => MainButtonWorker_ToggleAlertsWindow.Open()));
				options.Add(new FloatMenuOption("TD.Inspect".Translate(), () => Inspect(searchAlert)));

				Find.WindowStack.Add(new FloatMenu(options));
			}
			else if (Event.current.shift)
				Inspect(searchAlert);
			else
				base.OnClick();
		}

		public static void Inspect(QuerySearchAlert searchAlert)
		{
			MainButtonWorker_ToggleAlertsWindow.Open().PopUpEditor(searchAlert);
			Find.WindowStack.Add(new ResultThingListWindow(searchAlert.search));
		}
	}
}
