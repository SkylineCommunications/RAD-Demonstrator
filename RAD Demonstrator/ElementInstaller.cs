namespace Elements
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Skyline.DataMiner.Analytics.DataTypes;
	using Skyline.DataMiner.Analytics.Rad;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	internal class ElementInstaller
	{
		private readonly IEngine engine;
		private readonly List<string> protocolSuffixes_ = new List<string> {
			"22_1083", "23_146_", "23_196_", "23_19_1", "23_265_", "23_323_", "23_388_", "23_560_", "23_565_", "23_64_1",
			"23_739_", "24_310_", "24_7_10", "28_25_1", "28_568_", "30_100_", "30_30_1", "30_424_", "30_639_", "30_751_",
			"34_1628", "34_1656", "34_1862", "34_240_", "34_282_", "34_298_", "51_576_", "60_118_", "66_349_",
		};

		public ElementInstaller(IEngine engine)
		{
			this.engine = engine;
		}

		public void InstallDefaultContent()
		{
			int LondonViewID = CreateViews(new string[] { "DataMiner Catalog", "Using Relational Anomaly Detection", "London" });
			CreateElement($"RAD - Commtia LON 1", "AI - Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
			CreateElement($"RAD - Commtia LON 2", "AI - Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
			CreateElement($"RAD - Commtia LON 3", "AI - Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
			CreateElement($"RAD - Commtia LON 4", "AI - Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");
			CreateElement($"RAD - Commtia LON 5", "AI - Commtia DAB", "1.0.0.1-fast", LondonViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");

			int radFleetOutlierViewID = CreateViews(new string[] { "DataMiner Catalog", "Using Relational Anomaly Detection", "RAD Fleet Outlier" });
			for (int i = 0; i < protocolSuffixes_.Count; ++i)
				CreateElement($"AI - RAD - Commtia LON {(i + 1):D2}", "Fleet-Outlier-Detection-Commtia DAB", $"1.0.0.1-outlier-radar-{protocolSuffixes_[i]}", radFleetOutlierViewID, "TrendTemplate_PA_Demo", "AlarmTemplate_PA_Demo");

			var dms = engine.GetDms();
			//Verify the elements were all created.
			while (dms.GetElements().Where(e => e.Name.Contains("AI - RAD - Commtia LON")).Count() != 29)
			{
				Thread.Sleep(TimeSpan.FromSeconds(10));
			}

			//Create the RAD shared Group for the AI - RAD - Commtia LON elements
			var subgroupInfos = dms.GetElements()
				.Where(e => e.Name.StartsWith("AI - RAD - Commtia LON"))
				.Select(e => new RADSubgroupInfo(e.Name, new List<RADParameter>()
				{
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA1"), "PA1"),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA2"), "PA2"),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA3"), "PA3"),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 1022), "Total Output Power"),
				}))
				.ToList();
			var groupInfo = new RADGroupInfo("AI - RAD - Commtia", subgroupInfos, false);
			var request = new AddRADParameterGroupMessage(groupInfo);
			engine.SendSLNetMessage(request);
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
