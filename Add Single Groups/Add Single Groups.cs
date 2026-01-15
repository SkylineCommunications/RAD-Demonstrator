namespace ConfigureLondonDABs
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

			var groupInfos = dms.GetElements()
				.Where(e => e.Name.StartsWith("RAD - Commtia LON "))
				.Select(e => new RADGroupInfo(e.Name, new List<ParameterKey>()
				{
					new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 1022), //This is the parameterKey for the Total Output Power of our transmitter
					new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA1"), //This is the parameterKey for the PA1 Output Power of our transmitter
					new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA2"), //This is the parameterKey for the PA2 Output Power of our transmitter
					new ParameterKey(e.DmsElementId.AgentId, e.DmsElementId.ElementId, 2243, "PA3"), //This is the parameterKey for the PA3 Output Power of our transmitter
				},
				false)).ToList();

			foreach (var groupInfo in groupInfos) {
				engine.SendSLNetMessage(new AddRADParameterGroupMessage(groupInfo));
			}
		}
	}
}