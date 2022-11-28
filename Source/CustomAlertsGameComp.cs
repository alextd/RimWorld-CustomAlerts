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

		public QuerySearchAlert GetSavedAlert(string name) => alerts.First(sa => name == sa.search.name);
		public bool HasSavedAlert(string name) => alerts.Any(sa => name == sa.search.name);

		public void AddAlert(QuerySearch search) => AddAlert(new QuerySearchAlert(search));


		public void AddAlert(QuerySearchAlert newSearchAlert)
		{ 
			if (HasSavedAlert(newSearchAlert.search.name))
			{
				Find.WindowStack.Add(new Dialog_Name(newSearchAlert.search.name,
					name =>
					{
						newSearchAlert.search.name = name;
						LiveAlerts.AddAlert(newSearchAlert);
						alerts.Add(newSearchAlert);
					},
					rejector: name => alerts.Any(sa => sa.search.name == name)));
			}
			else
			{
				LiveAlerts.AddAlert(newSearchAlert);
				alerts.Add(newSearchAlert);
			}
		}


		public void AddAlerts(SearchGroup searches)
		{
			foreach (QuerySearch search in searches)
			{
				QuerySearchAlert newSearchAlert = new(search);

				if (HasSavedAlert(newSearchAlert.search.name))
					newSearchAlert.search.name += "TD.CopyNameSuffix".Translate();

				LiveAlerts.AddAlert(newSearchAlert);
				alerts.Add(newSearchAlert);
			}
		}

		public void RenameAlert(QuerySearchAlert searchAlert)
		{
			Find.WindowStack.Add(new Dialog_Name(searchAlert.search.name,
				name => searchAlert.search.name = name,
				rejector: name => alerts.Any(sa => sa.search.name == name)));
		}

		public void RemoveAlert(QuerySearchAlert searchAlert)
		{
			alerts.Remove(searchAlert);
			LiveAlerts.RemoveAlert(searchAlert);
		}
	}
}
