namespace ConfigureFleetOutlierDetectionGroup
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Analytics.DataTypes;
	using Skyline.DataMiner.Analytics.Rad; 
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		// For shared model groups, we have multiple similar setups (e.g. 100 transmitters = 100 similar setups).
		// Each setup is called a "subgroup" and looks similar: a parameter in one subgroup has a natural counterpart
		// to a parameter in every other subgroup.
		//
		// These parameters do not need to have the same name in DataMiner (e.g. Linux vs Windows CPU naming).
		// We map each element-specific parameter to a consistent, user-friendly name in the shared model group.
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
			// Resolve the DataMiner System (DMS) API handle from the Automation engine.
			// This is used to query elements and to push RAD group configurationt.
			var dms = engine.GetDms();

			// Create one RAD subgroup per matching element.
			// The filter below selects all elements that should participate in the fleet outlier detection group.
			//
			// Each subgroup contains a mapping from an element-specific ParameterKey towards a shared model parameter name,
			// allowing RAD to compare "the same" metric across the entire fleet.
			var subgroupInfos = dms.GetElements()
				.Where(e => e.Name.StartsWith("Fleet-Outlier-Detection-Commtia"))
				.Select(e => new RADSubgroupInfo(e.Name, new List<RADParameter>()
				{
					// Parameter 2243 is indexed; each PA ("PA1/PA2/PA3") is mapped to a distinct shared name.
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA1"), OUTPUTPOWERPA1),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA2"), OUTPUTPOWERPA2),
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA3"), OUTPUTPOWERPA3),

					// Parameter 1022 represents the total output power for the transmitter.
					new RADParameter(new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 1022), TOTALOUTPUTPOWER),
				}))
				.ToList();

			// Create or update the RAD parameter group used for fleet outlier detection.
			// The extra numeric arguments configure outlier detection behavior for this group (e.g. sensitivity/windowing),
			// and are passed through to the RAD backend as part of the group definition.
			var groupInfo = new RADGroupInfo("Fleet-Outlier-Group", subgroupInfos, false, 3, 5);

			// Prepare the SLNet message that will add/update the RAD group configuration.
			var request = new AddRADParameterGroupMessage(groupInfo);

			// Configure training:
			// Train using a fixed lookback window (last 8 days up to "now").
			List<TimeRange> timeRanges = new List<TimeRange>
			{
				new TimeRange(DateTime.UtcNow.AddDays(-8), DateTime.UtcNow)
			};

			request.TrainingConfiguration = new TrainingConfiguration(timeRanges);

			// Push the configuration to DataMiner.
			engine.SendSLNetMessage(request);
		}
	}
}