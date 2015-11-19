using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Backup {
	class Program {
		static string folder;
		static string logfile;
		static bool shutdown;
		static int generations = 3;
		static int generation;
		static List<string> disks;
		static DateTime now;

		static void Main(string[] args) {
			try {
				disks = new List<string>();
				now = DateTime.Now.Date;
				generation = (now.Year - 2000) * 12 + (now.Month - 1);
				foreach (string arg in args) {
					if (folder == null) {
						if (arg.StartsWith("-")) {
							switch (arg[1]) {
								case 's':
								case 'S':
									shutdown = true;
									break;
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
									generations = int.Parse(arg.Substring(1));
									break;
								default:
									Usage("Invalid switch '" + arg + "'");
									break;
							}
						} else {
							folder = arg;
						}
					} else {
						if (arg.Length != 1 || arg[0] < 'C' || arg[0] > 'Z' || !Directory.Exists(arg + @":\")) {
							Usage("Invalid disk " + arg);
						}
						disks.Add(arg);
					}
				}
				if (disks.Count < 1) {
					Usage("");
				}
				generation %= generations;
				if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
				logfile = Path.Combine(folder, "Backup.log");
				folder = Path.Combine(folder, generation.ToString());
				if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
				bool delay = false;
				foreach (string disk in disks) {
					if (delay) System.Threading.Thread.Sleep(10000);	// Pause between jobs, otherwise get System.ComponentModel.Win32Exception: The operation was cancelled by the user
					else delay = true;
					Run(disk);
				}
				if (shutdown) {
					// NB: This doesn't work on my 64-bit XP machine - don't know why.
					Process p = new Process();
					Process.Start("cmd", @"/c shutdown /s /t 0");
				}
			} catch(Exception ex) {
				Log("Backup {0}", string.Join(" ", args));
				Log("Exception: {0}", ex);
				Console.WriteLine("Exception: {0}", ex);
				Console.WriteLine();
				Usage(ex.Message);
			}
		}

		static void Run(string disk) {
			string fullname = Path.Combine(folder, disk + ".SNA");
			bool fullback = true;
			if (File.Exists(fullname)) {
				DateTime t = File.GetLastWriteTime(fullname);
				t = t.AddDays(-t.Day);
				if (t > now.AddMonths(-generations)) fullback = false;
			}
			if (fullback) {
				// Delete all the old files
				string fname = Path.Combine(folder, disk + ".SNA");
				if (File.Exists(fname)) File.Delete(fname);
				foreach (string file in Directory.GetFiles(folder, "Snapshot" + disk + "??.SNA")) {
					string name = Path.GetFileNameWithoutExtension(file);
					if (Char.IsDigit(name[9]) && char.IsDigit(name[10])) File.Delete(file);
				}
			}
			Process p = new Process();
			p.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Snapshot\Snapshot.exe");
			p.StartInfo.Arguments = string.Format(fullback ?
				@"-R -L0 -Go --usevss {0}: {1}\{0}.SNA"
				: @"-R -L0 -Go --usevss {0}: {1}\Snapshot{0}{2:00}.SNA -h{1}\{0}.hsh",
				disk, folder, now.Day);
			DateTime started = DateTime.Now;
			p.Start();
			System.Console.WriteLine("Waiting for {1} backup of disk {0} to finish", disk,
				fullback ? "full" : "differential");
			p.WaitForExit();
			string filename = string.Format(fullback ?
				@"{1}\{0}.SNA" : @"{1}\Snapshot{0}{2:00}.SNA",
				disk, folder, now.Day);
			Log("{1} backup of disk {0} to {2} completed in {3}, Exit code {4}", disk,
				fullback ? "Full" : "Differential", filename, DateTime.Now - started, p.ExitCode);
		}

		static void Log(string format, params object[] args) {
			try {
				using (StreamWriter sw = new StreamWriter(logfile, true)) {
					sw.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:"));
					sw.WriteLine(format, args);
				}
			} catch {
			}
		}

		static void Usage(string message) {
			Console.WriteLine(message);
			Console.WriteLine("Usage:-");
			Console.WriteLine("Backup [-<generations>] [-s(hutdown)] <folder> <disk> [<disk> ...]");
			Console.WriteLine();
			Console.WriteLine("Press ENTER to exit");
			Console.ReadLine();
			Environment.Exit(1);
		}
	}
}
