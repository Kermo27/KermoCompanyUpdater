using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KermoCompanyUpdater
{
	public class Logger
	{
		private string logFilePath;

		public Logger(string filePath)
		{
			logFilePath = filePath;
		}

		public void Log(string message)
		{
			try
			{
				using (StreamWriter writer = new StreamWriter(logFilePath, true))
				{
					writer.WriteLine($"{DateTime.Now} - {message}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred while writing to the log file: {ex.Message}");
			}
		}

		public void ClearLog()
		{
			try
			{
				File.WriteAllText(logFilePath, string.Empty);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error clearing log file: {ex.Message}");
			}
		}
	}
}