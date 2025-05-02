namespace ConfigureLondonDABs
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Analytics.DataTypes;
	using Skyline.DataMiner.Analytics.Mad; //In more recent DataMiner Versions, you can use Skyline.DataMiner.Analytics.Rad instead of Mad
	using Skyline.DataMiner.Automation;
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
			var elements = engine.FindElementsByProtocol("Empower 2025 - AI - Commtia DAB");
			int counter = 0;
			foreach (var element in elements)
			{
				++counter;
				string elementName = element.ElementName;
				if (elementName.Contains("STH"))
				{
					string groupName = "Southampton_DAB_" + counter.ToString();

					//Get Parameters:
					int dmaId = element.DmaId;
					int elementID = element.ElementId;
					int totalOutput = 1022;
					var totalOutputKey = new ParameterKey(dmaId, elementID, totalOutput);

					int PAOutput = 2243;
					var OutputPA1 = new ParameterKey(dmaId, elementID, PAOutput, "PA1");
					var OutputPA2 = new ParameterKey(dmaId, elementID, PAOutput, "PA2");
					var OutputPA3 = new ParameterKey(dmaId, elementID, PAOutput, "PA3");

					var parameters = new List<ParameterKey>() { totalOutputKey, OutputPA1, OutputPA2, OutputPA3 };

					//get MadGroupInfo
					var madGroupInfo = new MADGroupInfo(groupName, parameters, false); //This feature got renamed from MAD to RAD: so you can also use the RadGroupInfo instead of MadGroupInfo

					//Add group
					var addGroupMessage = new AddMADParameterGroupMessage(madGroupInfo); //This feature got renamed from MAD to RAD: so you can also use the AddRadParameterGroupMessage instead of AddMadParameterGroupMessage
					engine.SendSLNetMessage(addGroupMessage);
				}
			}
		}
	}
}