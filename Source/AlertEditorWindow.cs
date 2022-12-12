using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TD_Find_Lib;
using Verse;

namespace Custom_Alerts
{
	class AlertEditorWindow : SearchEditorRevertableWindow
	{
		QuerySearchAlert searchAlert;

		public AlertEditorWindow(QuerySearchAlert searchAlert) : base(searchAlert.search, SearchAlertTransfer.TransferTag)
		{
			this.searchAlert = searchAlert;

			title = "TD.EditingAlert".Translate();
			showNameAfterTitle = true;
		}

		public override void PostClose()
		{
			if (search.changed)
			{
				if (!searchAlert.alert.enabled)

					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
						"TD.StartAlert".Translate(), () => searchAlert.alert.enabled = true));
				else
					base.PostClose();
			}
		}
	}
}
