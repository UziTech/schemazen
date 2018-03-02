using System;
using System.Diagnostics;

namespace SchemaZen.Library {
	public class Logger : ILogger {
		private int messageLength = 0;
		private int loadingCount = 0;
		private string[] loading = new String[] { "-", "\\", "/" };
		private readonly bool _verbose;
		public Logger(bool verbose) {
			_verbose = verbose;
		}

		public void Log(TraceLevel level, string message) {
			var prevColor = Console.ForegroundColor;

			switch (level) {
				case TraceLevel.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case TraceLevel.Verbose:
					if (!_verbose)
						return;
					break;
				case TraceLevel.Warning:
					//Console.ForegroundColor = ConsoleColor.Red;
					break;
			}

			if (message.EndsWith("\r")) {
				message = message.TrimEnd('\r');
				if (message.Length < messageLength) {
					// TODO: make it work with tabs
					message += new string(' ', messageLength - message.Length);
				}

				if (message.StartsWith("-")) {
					message = loading[loadingCount++ / 5 % loading.Length] + message.Substring(1);
				}
				messageLength = message.Length;
				Console.Write(message + "\r");
			} else {
				if(message.Length < messageLength) {
					// TODO: make it work with tabs
					message += new string(' ', messageLength - message.Length);
				}
				messageLength = 0;
				Console.WriteLine(message);
			}

			Console.ForegroundColor = prevColor;
		}
	}
}
