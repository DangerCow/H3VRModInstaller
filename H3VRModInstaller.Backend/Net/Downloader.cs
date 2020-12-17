﻿using System;
using System.Threading;
using System.Net;
using H3VRModInstaller.Filesys;
using H3VRModInstaller.Filesys.Common;

namespace H3VRModInstaller.Net
{
	class Downloader
	{

		private readonly WebClient downloader = new();
		private readonly InstallMods installer = new();
		private readonly ModList mods = new();
		private bool finished = false;
		public string[] ModsDownloaded;

		public bool DownloadMod(string[] fileinfo)
		{
			if (ModsDownloaded == null)
			{
				ModsDownloaded = new string[0];
			}
			finished = false;
			if (fileinfo[0] == "" || fileinfo[0] == null) { return false; }

			string fileToDownload = fileinfo[0];
			string locationOfFile = fileinfo[1];

			for (int i = 0; i < ModsDownloaded.Length; i++)
			{
				if (fileToDownload == ModsDownloaded[i])
				{
					if (fileinfo[3] == "0")
					{
						repeat:
						Console.WriteLine("Mod already installed Are you sure you want to reinstall? (y/n)");
						var input = Console.ReadLine();
						if (input == "y")
						{
							break;
						}
						else if (input == "n")
						{
							return false;
						}
						else goto repeat;
					}
					Console.WriteLine("Mod already installed!");
					return false;
				}
			}

			Uri fileloc = new Uri(locationOfFile + fileToDownload);

			Console.WriteLine("Downloading Mod \"{0}\" from \"{1}{0}\"\n", fileToDownload, locationOfFile);


			Console.WriteLine("");

			downloader.DownloadFileCompleted += dlcomplete;
			downloader.DownloadProgressChanged += dlprogress;
			downloader.DownloadFileAsync(fileloc, fileToDownload);
			while (!finished)
			{
				Thread.Sleep(50);
			}
			finished = false;

			Console.WriteLine("File Downloaded");

			Console.WriteLine("Successfully Downloaded Mod \"{0}\" from \"{1}{0}\"\n", fileToDownload, locationOfFile);

			installer.installMod(fileinfo);

			Array.Resize<string>(ref ModsDownloaded, ModsDownloaded.Length + 1);
			ModsDownloaded[ModsDownloaded.Length - 1] = fileToDownload;

			return true;
		}

		public void dlcomplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			finished = true;
		}

		public void dlprogress(object sender, DownloadProgressChangedEventArgs e)
		{
			float percentage = ((float)e.BytesReceived / (float)e.TotalBytesToReceive) * 100;
			string percentagetext = String.Format("{0:00.00}", percentage);
			Console.Write("\r" + percentagetext + "% downloaded!");
		}


		public bool DownloadModDirector(string mod, bool downloadAll = false)
		{
			if (!mods.onlineCheck()) { Console.WriteLine("Not connected to internet, or GitHub is down!"); return false; }
			var result = mods.getModInfo(mod);
			if (result == null) return false;
			for (var i = 0; i < result.Item1.GetLength(0); i++)
			{
				string[] fileinfo = new string[4];
				fileinfo[0] = result.Item1[i, 0];
				fileinfo[1] = result.Item1[i, 1];
				fileinfo[2] = result.Item1[i, 2];
				fileinfo[3] = i.ToString();
				DownloadMod(fileinfo);
			}
			return true;
		}
		
	}
}
