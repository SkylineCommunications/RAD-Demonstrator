namespace ConfigureLondonDABSharedModelGroup
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Analytics.DataTypes;
	using Skyline.DataMiner.Analytics.Mad;
	using Skyline.DataMiner.Analytics.Rad; 
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Net.ReportsAndDashboards;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{

		//For a shared model groups, we have multiple similar setups: e.g. 100 different dab transmitters = 100 similar setups.
		// Each setup is called a "subgroup" and looks similar: so a parameter in one subgroup has a natural counterpart to a parameter in every other subgroup.
		// These parameters do not need to have the same name in DataMiner: e.g. you may be monitoring a linux and a windows server and the cpu usage parameter may be called differently on both systems: e.g. total processor load vs. cpu usage.
		// We will assign a nice name to each global group parameter.
		public const string TOTALOUTPUTPOWER = "Tx Amplifier Output Power";
		public const string OUTPUTPOWERPA1 = "PA1 Output Power";
		public const string OUTPUTPOWERPA2 = "PA2 Output Power";
		public const string OUTPUTPOWERPA3 = "PA3 Output Power";

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var dms = engine.GetDms();

			var subgroupInfos = dms.GetElements()
				.Where(e => e.Name.StartsWith("RAD - Commtia LON "))
				.Select(e => new RADSubgroupInfo(e.Name, new List<RADParameter>()
				{
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA1"),OUTPUTPOWERPA1),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA2"), OUTPUTPOWERPA2),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA3"), OUTPUTPOWERPA3),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 1022), TOTALOUTPUTPOWER),
				}))
				.ToList();

			var groupInfo = new RADGroupInfo("DAB Fleet", subgroupInfos, false);
			var request = new AddRADParameterGroupMessage(groupInfo);
			engine.SendSLNetMessage(request);
		}
	}
}