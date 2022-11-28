using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace Custom_Alerts
{
	[StaticConstructorOnStartup]
	public static class CustomAlertTex
	{
		public static readonly Texture2D Equal = ContentFinder<Texture2D>.Get("Equals", true);
		public static readonly Texture2D GreaterThan = ContentFinder<Texture2D>.Get("GreaterThan", true);
		public static readonly Texture2D LessThan = ContentFinder<Texture2D>.Get("LessThan", true);

		public static readonly Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor", true);
	}
}
