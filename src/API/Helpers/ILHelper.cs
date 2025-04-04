using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace SerousCommonLib.API {
	/// <summary>
	/// A helper class for manipulating and logging <see cref="ILCursor"/> objects
	/// </summary>
	public static class ILHelper {
		private static readonly HashSet<string> _autologgingSources = [];

		internal static void ClearPatchSources() {
			_autologgingSources.Clear();
		}

		internal static void PrepareInstruction(Instruction instr, out string offset, out string opcode, out string operand) {
			offset = $"IL_{instr.Offset:X5}:";

			opcode = instr.OpCode.Name;

			if (instr.Operand is null)
				operand = "";
			else if (instr.Operand is ILLabel label) {
				//This label's target should NEVER be null!  If it is, the IL edit wouldn't load anyway
				if (label.Target is not null)
					operand = $"IL_{label.Target.Offset:X5}";
				else
					throw new NullReferenceException("Branch instruction had a null target: " + instr);
			} else if (instr.OpCode == OpCodes.Switch)
				operand = "(" + string.Join(", ", (instr.Operand as ILLabel[])!.Select(l => $"IL_{l.Target.Offset:X5}")) + ")";
			else
				operand = instr.Operand.ToString()!;
		}

		/// <summary>
		/// Logs information about an <see cref="ILCursor" /> object's method body. <br />
		/// </summary>
		/// <param name="c">The IL editing cursor</param>
		/// <param name="logFilePath">The destination file</param>
		public static void LogMethodBody(this ILCursor c, string logFilePath) {
			// Ensure that the instructions listed have the correct offset
			UpdateInstructionOffsets(c);

			int index = 0;

			Directory.CreateDirectory(new FileInfo(logFilePath).DirectoryName!);

			// If the file already exists, add a numeric suffix to avoid overwriting
			if (File.Exists(logFilePath)) {
				logFilePath = Path.GetFileNameWithoutExtension(logFilePath);

				// Special cases for CommonPatchingWrapper
				bool hasAfter = false, hasBefore = false;
				if (logFilePath.EndsWith(" - After")) {
					hasAfter = true;
					logFilePath = logFilePath[..^8]; // Remove " - After"
				} else if (logFilePath.EndsWith(" - Before")) {
					hasBefore = true;
					logFilePath = logFilePath[..^9]; // Remove " - Before"
				}

				int suffix = 1;
				string specialSuffix = hasBefore ? " - Before" : hasAfter ? " - After" : "";
				string newPath;
				do {
					newPath = $"{logFilePath} ({suffix}){specialSuffix}.txt";
				} while (File.Exists(newPath));

				logFilePath = newPath;
			}

			FileStream file = File.Open(logFilePath, FileMode.Create);

			using StreamWriter writer = new(file);

			writer.WriteLine(DateTime.Now.ToString("'['ddMMMyyyy '-' HH:mm:ss']'"));
			writer.WriteLine($"// ILCursor: {c.Method.Name}\n");

			writer.WriteLine("// Arguments:");

			var args = c.Method.Parameters;
			if (args.Count == 0)
				writer.WriteLine($"{"none",8}");
			else {
				foreach (var arg in args) {
					string argIndex = $"[{arg.Index}]";
					writer.WriteLine($"{argIndex,8} {arg.ParameterType.FullName} {arg.Name}");
				}
			}

			writer.WriteLine();

			writer.WriteLine("// Locals:");

			if (!c.Body.HasVariables)
				writer.WriteLine($"{"none",8}");
			else {
				foreach (var local in c.Body.Variables) {
					string localIndex = $"[{local.Index}]";
					writer.WriteLine($"{localIndex,8} {local.VariableType.FullName} V_{local.Index}");
				}
			}

			writer.WriteLine();

			writer.WriteLine("// Body:");
			do {
				PrepareInstruction(c.Instrs[index], out string offset, out string opcode, out string operand);

				writer.WriteLine($"{offset,-10}{opcode,-12} {operand}");
				index++;
			} while (index < c.Instrs.Count);
		}

		/// <summary>
		/// Updates the instruction offsets within <paramref name="c"/>
		/// </summary>
		/// <param name="c">The cursor</param>
		public static void UpdateInstructionOffsets(ILCursor c) {
			var instrs = c.Instrs;
			int curOffset = 0;

			static Instruction[] ConvertToInstructions(ILLabel[] labels) {
				Instruction[] ret = new Instruction[labels.Length];

				for (int i = 0; i < labels.Length; i++)
					ret[i] = labels[i].Target;

				return ret;
			}

			foreach (var ins in instrs) {
				ins.Offset = curOffset;

				if (ins.OpCode != OpCodes.Switch)
					curOffset += ins.GetSize();
				else {
					//'switch' opcodes don't like having the operand as an ILLabel[] when calling GetSize()
					//thus, this is required to even let the mod compile

					Instruction copy = Instruction.Create(ins.OpCode, ConvertToInstructions((ILLabel[])ins.Operand));
					curOffset += copy.GetSize();
				}
			}
		}

		/// <summary>
		/// Initializes automatic dumping of MonoMod assemblies to the tModLoader install directory.<br/>
		/// Currently does not work due to an issue in MonoMod.
		/// </summary>
		public static void InitMonoModDumps() {
			//see: https://discord.com/channels/103110554649894912/445276626352209920/953380019072270419

			Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "Auto");
			Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG", "1");

			string dumpDir = Path.GetFullPath("MonoModDump");

			Directory.CreateDirectory(dumpDir);

			Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", dumpDir);
		}

		/// <summary>
		/// De-initializes automatic dumping of MonoMod assemblies to the tModLoader install directory.<br/>
		/// Currently does not work due to an issue in MonoMod.
		/// </summary>
		public static void DeInitMonoModDumps() {
			Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG", "0");
		}

		/// <summary>
		/// Gets the instruction at the given index, represented as a string.
		/// </summary>
		/// <param name="c">The IL cursor.</param>
		/// <param name="index">The instruction index.</param>
		/// <returns>The string-represented instruction.</returns>
		public static string GetInstructionString(ILCursor c, int index) {
			if (index < 0 || index >= c.Instrs.Count)
				return "ERROR: Index out of bounds.";

			PrepareInstruction(c.Instrs[index], out string offset, out string opcode, out string operand);

			return $"{offset} {opcode}   {operand}";
		}

		/// <summary>
		/// Verifies that each <see cref="MemberInfo" /> is not null.
		/// </summary>
		/// <param name="memberInfos">
		/// An array of <see cref="MemberInfo" /> objects, paired with an identifier used when throwing the <see cref="NullReferenceException" /> if the object is null.
		/// </param>
		/// <exception cref="NullReferenceException"/>
		public static void EnsureAreNotNull(params (MemberInfo member, string identifier)[] memberInfos) {
			foreach (var (member, identifier) in memberInfos)
				if (member is null)
					throw new NullReferenceException($"Member reference \"{identifier}\" is null");
		}

		/// <summary>
		/// A delegate taking in a cursor and default error message as input and returns if the edit was successful
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="badReturnReason">The error message to display when the edit fails</param>
		/// <returns></returns>
		public delegate bool PatchingContextDelegate(ILCursor c, ref string badReturnReason);

		/// <summary>
		/// This method logs the instructions within the method tied to <paramref name="il"/>, invokes <paramref name="doEdits"/> and then logss the instructions within the method again
		/// </summary>
		/// <param name="il">The context</param>
		/// <param name="patchSource">Which mod is performing the edits.  This affects the output directory of the file</param>
		/// <param name="doEdits">The delegate used to perform the edit</param>
		/// <remarks>The generated log file will be at <c>Documents/My Games/Terraria/tModLoader/aA Mods/ModName/</c></remarks>
		/// <exception cref="Exception"/>
		public static void CommonPatchingWrapper(ILContext il, Mod patchSource, PatchingContextDelegate doEdits) {
			CommonPatchingWrapper(il, patchSource, true, doEdits);
		}

		/// <summary>
		/// This method logs the instructions within the method tied to <paramref name="il"/>, invokes <paramref name="doEdits"/> and then logss the instructions within the method again
		/// </summary>
		/// <param name="il">The context</param>
		/// <param name="patchSource">Which mod is performing the edits.  This affects the output directory of the file</param>
		/// <param name="throwOnFail">Whether an exception should be thrown if <paramref name="doEdits"/> fails</param>
		/// <param name="doEdits">The delegate used to perform the edit</param>
		/// <remarks>The generated log file will be at <c>Documents/My Games/Terraria/tModLoader/aA Mods/ModName/</c></remarks>
		/// <exception cref="Exception"/>
		public static void CommonPatchingWrapper(ILContext il, Mod patchSource, bool throwOnFail, PatchingContextDelegate doEdits) {
			ArgumentNullException.ThrowIfNull(doEdits);

			ILCursor c = new(il);

			// Use the mod type's namespace start
			// Can't use "Mod.Name" since that uses "Mod.File" which might be null
			string modName = patchSource.GetType().Namespace!;

			string localDir = Path.Combine(Program.SavePath, "aA Mods", modName);

			// Clear the directory if this is the first patch applied by the mod
			if (_autologgingSources.Add(modName)) {
				try {
					if (Directory.Exists(localDir))
						Directory.Delete(localDir, true);

					Directory.CreateDirectory(localDir);
				} catch (Exception ex) {
					patchSource.Logger.Error("Failed to clear patch source directory: " + ex.Message);
				}
			}

			// Get the method name
			string method = c.Method.Name;
			if (!method.Contains("ctor"))
				method = method[(method.LastIndexOf(':') + 1)..];
			else
				method = method[method.LastIndexOf('.')..];

			// Get the class name
			string type = c.Method.Name;
			type = type[..type.IndexOf(':')];
			type = type[(type.LastIndexOf('.') + 1)..];

			try {
				LogMethodBody(c, Path.Combine(localDir, $"{type}.{method} - Before.txt"));
			} catch (Exception ex) {
				patchSource.Logger.Error("Failed to log method body for before edits: " + ex.Message);
			}

			string error = $"Unable to fully patch {il.Method.Name}()";
			string badReturnReason = error;
			if (!doEdits(c, ref badReturnReason)) {
				if (throwOnFail)
					throw new Exception(badReturnReason);

				patchSource.Logger.Error(error + "\n  Reason: " + badReturnReason);
				// Do not report an After file
				return;
			}

			try {
				LogMethodBody(c, Path.Combine(localDir, $"{type}.{method} - After.txt"));
			} catch (Exception ex) {
				patchSource.Logger.Error("Failed to log method body for after edits: " + ex.Message);
			}
		}
	}
}
