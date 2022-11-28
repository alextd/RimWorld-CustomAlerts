using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using TD_Find_Lib;

namespace Custom_Alerts
{
	class AlertsManagerWindow : Window
	{
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(900f, 700f);
			}
		}

		//protected but using publicized assembly
		//protected override void SetInitialSizeAndPosition()
		public override void SetInitialSizeAndPosition()
		{
			base.SetInitialSizeAndPosition();
			windowRect.x = (UI.screenWidth - windowRect.width)/2;
			windowRect.y = UI.screenHeight - MainButtonDef.ButtonHeight - this.windowRect.height;
		}

		public AlertsManagerWindow()
		{
			this.forcePause = true;
			this.doCloseX = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
		}

		private const float RowHeight = WidgetRow.IconSize + 6;

		private Vector2 scrollPosition = Vector2.zero;
		private float scrollViewHeight;
		public override void DoWindowContents(Rect inRect)
		{
			//Title
			var listing = new Listing_Standard();
			listing.Begin(inRect);
			Text.Font = GameFont.Medium;
			listing.Label("TD.CustomAlerts".Translate());
			Text.Font = GameFont.Small;
			listing.GapLine();
			listing.End();

			//Check off
			Rect enableRect = inRect.RightHalf().TopPartPixels(Text.LineHeight);
			Widgets.CheckboxLabeled(enableRect, "TD.EnableAlerts".Translate(), ref Alert_QuerySearch.enableAll);

			//Margin
			inRect.yMin += listing.CurHeight;

			//Useful things:
			CustomAlertsGameComp comp = Current.Game.GetComponent<CustomAlertsGameComp>();
			QuerySearchAlert remove = null;

			//Scrolling!
			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, scrollViewHeight);
			Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

			Rect rowRect = viewRect; rowRect.height = RowHeight;
			foreach (QuerySearchAlert searchAlert in comp.alerts)
			{
				WidgetRow row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, rowRect.width);
				rowRect.y += RowHeight;

				row.Label(searchAlert.name + searchAlert.GetMapNameSuffix(), rowRect.width / 4);

				if (row.ButtonText("Rename".Translate()))
					comp.RenameAlert(searchAlert);
				
				if (row.ButtonText("Delete".Translate()))
					remove = searchAlert;

				bool crit = searchAlert.alert.defaultPriority == AlertPriority.Critical;
				if (row.ToggleableIconChanged(ref crit, CustomAlertTex.PassionMajorIcon, "TD.CriticalAlert".Translate()))
				{
					searchAlert.SetPriority(crit ? AlertPriority.Critical : AlertPriority.Medium);
				}

				row.Label("TD.SecondsUntilShown".Translate());
				Rect textRect = row.GetRect(64); textRect.height -= 4; textRect.width -= 4;
				string dummyStr = null;
				Widgets.TextFieldNumeric(textRect, ref searchAlert.alert.secondsBeforeAlert, ref dummyStr, 0, 999999);
				TooltipHandler.TipRegion(textRect, "TD.Tip1000SecondsInARimworldDay".Translate());

				row.Label("TD.ShowWhen".Translate());
				if (row.ButtonIcon(TexFor(searchAlert.alert.compareType)))
					searchAlert.alert.compareType = (CompareType)((int)(searchAlert.alert.compareType + 1) % 3);

				textRect = row.GetRect(64); textRect.height -= 4; textRect.width -= 4;
				dummyStr = null;
				Widgets.TextFieldNumeric(textRect, ref searchAlert.alert.countToAlert, ref dummyStr, 0, 999999);
			}


			scrollViewHeight = rowRect.yMax;
			Widgets.EndScrollView();

			if (remove != null)
			{
				if (Event.current.shift)
					comp.RemoveAlert(remove);
				else
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
						"TD.Delete0".Translate(remove.name), () => comp.RemoveAlert(remove)));
			}
		}

		public static Texture2D TexFor(CompareType comp) =>
			comp == CompareType.Equal ? CustomAlertTex.Equal :
			comp == CompareType.Greater ? CustomAlertTex.GreaterThan :
			CustomAlertTex.LessThan;
	}
}
