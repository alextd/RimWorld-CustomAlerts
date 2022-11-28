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
	// GameComponent to hold the SearchAlerts
	// SearchAlert holds a Alert_Find
	// Alert_Find have to be inserted into the game's AllAlerts
	class CustomAlertsGameComp : GameComponent
	{
		public List<QuerySearchAlert> alerts = new();

		public CustomAlertsGameComp(Game g):base() { }

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref alerts, "alerts");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (alerts == null)
					alerts = new();

				foreach (QuerySearchAlert searchAlert in alerts)
					LiveAlerts.AddAlert(searchAlert);
			}
		}

		public QuerySearchAlert GetSavedAlert(string name) => alerts.First(sa => name == sa.name);
		public bool HasSavedAlert(string name) => alerts.Any(sa => name == sa.name);


		public void AddAlert(QuerySearch search)
		{
			QuerySearchAlert newSearchAlert = new(search);

			if (HasSavedAlert(newSearchAlert.name))
			{
				Find.WindowStack.Add(new Dialog_Name(newSearchAlert.name, 
					name =>
					{
						newSearchAlert.Rename(name);
						LiveAlerts.AddAlert(newSearchAlert);
						alerts.Add(newSearchAlert);
					},
					rejector: name => alerts.Any(sa => sa.name == name)));
			}
			else
			{
				LiveAlerts.AddAlert(newSearchAlert);
				alerts.Add(newSearchAlert);
			}
		}

		public void RenameAlert(QuerySearchAlert searchAlert)
		{
			Find.WindowStack.Add(new Dialog_Name(searchAlert.name,
				name => searchAlert.Rename(name),
				rejector: name => alerts.Any(sa => sa.name == name)));
		}

		public void RemoveAlert(QuerySearchAlert searchAlert)
		{
			alerts.Remove(searchAlert);
			LiveAlerts.RemoveAlert(searchAlert);
		}
	}

	[StaticConstructorOnStartup]
	public class SearchAlertTransfer : ISearchReceiver
	{
		static SearchAlertTransfer()
		{
			SearchTransfer.Register(new SearchAlertTransfer());
		}
		
		public string Source => "Custom Alert";
		public string ReceiveName => "Make Custom Alert";
		public QuerySearch.CloneArgs CloneArgs => QuerySearch.CloneArgs.use;

		public bool CanReceive() => Current.Game?.GetComponent<CustomAlertsGameComp>() != null;
		public void Receive(QuerySearch search) => Current.Game.GetComponent<CustomAlertsGameComp>().AddAlert(search);
	}
}
