using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace Custom_Alerts
{
	public static class WidgetRowEx
	{
		public static bool ToggleableIconChanged(this WidgetRow row, ref bool toggleable, Texture2D tex, string tooltip, SoundDef mouseoverSound = null, string tutorTag = null)
		{
			bool before = toggleable;
			row.ToggleableIcon(ref toggleable, tex, tooltip, mouseoverSound, tutorTag);
			return before != toggleable;
		}
	}
}
