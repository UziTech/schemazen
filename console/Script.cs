﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ManyConsole;
using SchemaZen.Library;
using SchemaZen.Library.Command;
using SchemaZen.Library.Models;

namespace SchemaZen.console {
	public class Script : BaseCommand {
		public Script()
			: base(
				"Script", "Generate scripts for the specified database.") {
			HasOption(
				"dataTables=",
				"A comma separated list of tables to export data from.",
				o => DataTables = o);
			HasOption(
				"dataTablesPattern=",
				"A regular expression pattern that matches tables to export data from.",
				o => DataTablesPattern = o);
			HasOption(
				"dataTablesExcludePattern=",
				"A regular expression pattern that exclude tables to export data from.",
				o => DataTablesExcludePattern = o);
			HasOption(
				"namePattern=",
				"A regular expression pattern that matches objects name to save.",
				o => NamePattern = o);
			HasOption(
				"nameExcludePattern=",
				"A regular expression pattern that exclude objects name to save.",
				o => NameExcludePattern = o);
			HasOption(
				"tableHint=",
				"Table hint to use when exporting data.",
				o => TableHint = o);
			HasOption(
				"filterTypes=",
				"A comma separated list of the types that will not be scripted. Valid types: " + Database.ValidTypes,
				o => FilterTypes = o);
			HasOption(
				"onlyTypes=",
				"A comma separated list of the types that will only be scripted. Valid types: " + Database.ValidTypes,
				o => OnlyTypes = o);
			HasOption(
				"1|singleDir",
				"",
				o => SingleDir = o != null);
		}

		private Logger _logger;
		protected string DataTables { get; set; }
		protected string FilterTypes { get; set; }
		protected string OnlyTypes { get; set; }
		protected string DataTablesPattern { get; set; }
		protected string DataTablesExcludePattern { get; set; }
		protected string NamePattern { get; set; }
		protected string NameExcludePattern { get; set; }
		protected string TableHint { get; set; }
		protected bool SingleDir { get; set; }

		public override int Run(string[] args) {
			_logger = new Logger(Verbose);

			if (!Overwrite && !OverwriteFiles && Directory.Exists(ScriptDir)) {
				if (ConsoleQuestion.AskYN($"{ScriptDir} already exists - do you want to cancel"))
					return 1;
				if (ConsoleQuestion.AskYN($"{ScriptDir} already exists - do you want to delete all files and folders in it")) {
					Overwrite = true;
				} else {
					OverwriteFiles = true;
				}
			}

			var scriptCommand = new ScriptCommand {
				ConnectionString = ConnectionString,
				DbName = DbName,
				Pass = Pass,
				ScriptDir = ScriptDir,
				Server = Server,
				User = User,
				Logger = _logger,
				Overwrite = Overwrite,
				OverwriteFiles = OverwriteFiles,
			};

			var filteredTypes = HandleFilteredTypes();
			
			var namesAndSchemas = HandleDataTables(DataTables);

			try {
				scriptCommand.Execute(namesAndSchemas, DataTablesPattern, DataTablesExcludePattern, NamePattern, NameExcludePattern, TableHint, filteredTypes, SingleDir);
			} catch (Exception ex) {
				throw new ConsoleHelpAsException(ex.Message);
			}
			return 0;
		}
		
		private List<string> HandleFilteredTypes() {
			var removeTypes = FilterTypes?.Split(',').ToList() ?? new List<string>();
			var keepTypes = OnlyTypes?.Split(',').ToList() ?? new List<string>(Database.Dirs);

			var invalidTypes = removeTypes.Union(keepTypes).Except(Database.Dirs).ToList();
			if (invalidTypes.Any()) {
				var msg = invalidTypes.Count() > 1 ? " are not valid types." : " is not a valid type.";
				_logger.Log(TraceLevel.Warning, String.Join(", ", invalidTypes.ToArray()) + msg);
				_logger.Log(TraceLevel.Warning, $"Valid types: {Database.ValidTypes}");
			}

			return Database.Dirs.Except(keepTypes.Except(removeTypes)).ToList();
		}

		private Dictionary<string, string> HandleDataTables(string tableNames) {
			var dataTables = new Dictionary<string, string>();

			if (string.IsNullOrEmpty(tableNames))
				return dataTables;

			foreach (var value in tableNames.Split(',')) {
				var schema = "dbo";
				var name = value;
				if (value.Contains(".")) {
					schema = value.Split('.')[0];
					name = value.Split('.')[1];
				}

				dataTables[name] = schema;
			}

			return dataTables;
		}
	}
}
