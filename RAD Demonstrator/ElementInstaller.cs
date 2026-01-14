namespace Elements
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	internal class ElementInstaller
	{
		private readonly IEngine engine;

		public ElementInstaller(IEngine engine)
		{
			this.engine = engine;
		}

		public void InstallDefaultContent()
		{
			int LondonViewID = CreateViews(new string[] { "DataMiner Catalog", "Using Relational Anomaly Detection", "London" });
			CreateElement($"RAD - Commtia LON 1", "Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
			CreateElement($"RAD - Commtia LON 2", "Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
			CreateElement($"RAD - Commtia LON 3", "Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
			CreateElement($"RAD - Commtia LON 4", "Commtia DAB - Deviating", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
		}

		private void AssignVisioToView(int viewID, string visioFileName)
		{
			var request = new AssignVisualToViewRequestMessage(viewID, new Skyline.DataMiner.Net.VisualID(visioFileName));

			engine.SendSLNetMessage(request);
		}

		private int? GetView(string viewName)
		{
			var views = engine.SendSLNetMessage(new GetInfoMessage(InfoType.ViewInfo));
			foreach (var m in views)
			{
				var viewInfo = m as ViewInfoEventMessage;
				if (viewInfo == null)
					continue;

				if (viewInfo.Name == viewName)
					return viewInfo.ID;
			}

			return null;
		}

		private int CreateNewView(string viewName, string parentViewName)
		{
			var request = new SetDataMinerInfoMessage
			{
				bInfo1 = int.MaxValue,
				bInfo2 = int.MaxValue,
				DataMinerID = -1,
				HostingDataMinerID = -1,
				IInfo1 = int.MaxValue,
				IInfo2 = int.MaxValue,
				Sa1 = new SA(new string[] { viewName, parentViewName }),
				What = (int)NotifyType.NT_ADD_VIEW_PARENT_AS_NAME,
			};

			var response = engine.SendSLNetSingleResponseMessage(request);
			if (!(response is SetDataMinerInfoResponseMessage infoResponse))
				throw new ArgumentException("Unexpected message returned by DataMiner");

			return infoResponse.iRet;
		}

		private int CreateViews(string[] viewNames)
		{
			int? firstNonExistingViewLevel = null;
			int? lastExistingViewID = null;
			string lastExistingViewName = null;

			for (int i = viewNames.Length - 1; i >= 0; --i)
			{
				int? viewID = GetView(viewNames[i]);
				if (viewID.HasValue)
				{
					lastExistingViewID = viewID;
					lastExistingViewName = viewNames[i];
					firstNonExistingViewLevel = i + 1;
					break;
				}
			}

			if (firstNonExistingViewLevel.HasValue && firstNonExistingViewLevel == viewNames.Length)
				return lastExistingViewID.Value;

			if (!firstNonExistingViewLevel.HasValue)
			{
				// No views in the tree already exist, so create all views starting from the root view
				lastExistingViewID = -1;
				lastExistingViewName = engine.GetDms().GetView(-1).Name;
				firstNonExistingViewLevel = 0;
			}

			for (int i = firstNonExistingViewLevel.Value; i < viewNames.Length; ++i)
			{
				lastExistingViewID = CreateNewView(viewNames[i], lastExistingViewName);
				lastExistingViewName = viewNames[i];
			}

			return lastExistingViewID.Value;
		}

		private void CreateElement(string elementName, string protocolName, string protocolVersion, int viewID,
			string trendTemplate = "Default", string alarmTemplate = "")
		{
			var request = new AddElementMessage
			{
				ElementName = elementName,
				ProtocolName = protocolName,
				ProtocolVersion = protocolVersion,
				TrendTemplate = trendTemplate,
				AlarmTemplate = alarmTemplate,
				ViewIDs = new int[] { viewID },
			};

			var dms = engine.GetDms();
			if (dms.ElementExists(elementName))
			{
				var elementRequest = new GetElementByNameMessage(elementName);
				var elementResponse = engine.SendSLNetSingleResponseMessage(elementRequest);
				if (!(elementResponse is ElementInfoEventMessage elementInfo))
					throw new ArgumentException("Unexpected message returned by DataMiner");

				// Remove the element if it exists
				var deleteRequest = new SetElementStateMessage(elementInfo.DataMinerID, elementInfo.ElementID, Skyline.DataMiner.Net.Messages.ElementState.Deleted, true);
				engine.SendSLNetMessage(deleteRequest);
				System.Threading.Thread.Sleep(TimeSpan.FromSeconds(15));
			}

			engine.SendSLNetSingleResponseMessage(request);
		}
	}
}
