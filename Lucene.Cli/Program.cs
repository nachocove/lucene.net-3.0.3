using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Lucene.Cli
{
	public partial class LuceneCli
	{
		private static string IndexDirectory = "lucene.index";

		private static void Help (int exitCode = 1)
		{
			Console.WriteLine ("USAGE: mono Lucene.Cli.exe [COMMANDS] [PARAMS...]\n");
			Console.WriteLine ("add [directories/files] - Add emails or directories of emails");
			Console.WriteLine ("remove [directories/files] - Remove emails");
			Console.WriteLine ("search [PATTERN]");
			Environment.Exit (exitCode);
		}

		private static void ParserOptions (string[] args)
		{
			if (0 == args.Length) {
				Help (0);
			}

			try {
				int index = 0;
				bool done = false;

				// Parse general options
				while (!done && (index < args.Length)) {
					switch (args [index]) {
					case "-d":
						index += 1; // consume -d
						Log.Debug ("Changing working directory to {0}", args [index]);
						System.IO.Directory.SetCurrentDirectory (args [index]);
						index += 1; // consume root directory
						break;
					case "-i":
						index += 1; // consume -i
						IndexDirectory = args [index];
						index += 1; // consume index file path
						break;
					case "-v":
						index += 1; // consume -v
						Log.DebugEnabled = true;
						break;
					default:
						done = true;
						break;
					}
				}

				// Check the command
				var command = args [index].ToLower ();
				string[] parameters = new string[args.Length - 1 - index];
				Array.Copy (args, index + 1, parameters, 0, args.Length - 1 - index);
				switch (command) {
				case "add":
					AddEmails (parameters);
					break;
				case "remove":
					RemoveEmails (parameters);
					break;
				case "search":
					SearchEmails (parameters[0]);
					break;
				default:
					Console.WriteLine ("ERROR: unknown command {0}", args [index]);
					Help ();
					break;
				}
			}
			catch (IndexOutOfRangeException) {
				Help ();
			}
		}

		public static void Main (string[] args)
		{
			ParserOptions (args);
		}
	}
}
