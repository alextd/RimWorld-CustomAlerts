using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Custom_Alerts
{
	public static class LiveAlerts
	{
		// Vanilla game works by statically listing all alerts
		private static List<Alert> AllAlerts =>
			((Find.UIRoot as UIRoot_Play)?.alerts as AlertsReadout)?.AllAlerts;

		// ... and copying them to activeAlerts to be displayed
		// (We only need this to remove from it)
		private static List<Alert> activeAlerts =>
			((Find.UIRoot as UIRoot_Play)?.alerts as AlertsReadout)?.activeAlerts;


		public static void AddAlert(QuerySearchAlert searchAlert)
		{
			AllAlerts.Add(searchAlert.alert);
		}

		public static void RemoveAlert(QuerySearchAlert searchAlert)
		{
			AllAlerts.Remove(searchAlert.alert);
			activeAlerts.Remove(searchAlert.alert);
		}
	}
}
