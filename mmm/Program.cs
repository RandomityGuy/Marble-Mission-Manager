﻿using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.IO.Compression;
using Missioneer;
using Missioneer.Utils;
using Path = System.IO.Path;

namespace mmm
{
    public class Program
    {
        public static string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),".mmm");

        public static List<Mission> missionList;

        public static string PQPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlatinumQuest");


        static void Main(string[] args)
        {
            Directory.CreateDirectory(DataFolder);

            string missionlistdata;

            if (!File.Exists(DataFolder + "\\missionlist.json"))
            {
                var client = new WebClient();
                missionlistdata = client.DownloadString("https://cla.higuy.me/api/v1/missions");
                File.WriteAllText(DataFolder + "\\missionlist.json", missionlistdata);
                client.Dispose();
            }
            else
                missionlistdata = File.ReadAllText(DataFolder + "\\missionlist.json");

            missionList = JsonConvert.DeserializeObject<List<Mission>>(missionlistdata);

            var type = args.Length >= 1 ? args[0] : "help";

            switch (type)
            {
                case "help":
                    Console.WriteLine("Marble Mission Manager v1.0");
                    Console.WriteLine("Common commands:");
                    Console.WriteLine("  update: update the missionlist");
                    Console.WriteLine("  install <mission>: install <mission> to %appdata%/PlatinumQuest");
                    Console.WriteLine("  search <mission>: search <mission> in the package");
                    Console.WriteLine("  clean: clean cache");
                    break;

                case "update":
                    Console.WriteLine("Updating missionlist");
                    Update();
                    break;

                case "install":
                    var mis = args[1];
                    Console.WriteLine("Searching mission");
                    Install(mis);
                    break;

                case "search":
                    var mis2 = args[1];
                    Console.WriteLine("Searching mission");
                    Search(mis2);
                    break;

                case "clean":
                    Clean();
                    break;

                default:
                    if (File.Exists(args[0]))
                    {
                        Console.WriteLine("Installing mission");
                        InstallZip(args[0]);
                    }
                    break;
            }
        }

        static void Update()
        {
            var client = new WebClient();
            var missionlistdata = client.DownloadString("https://cla.higuy.me/api/v1/missions");
            File.WriteAllText(DataFolder + "\\missionlist.json", missionlistdata);
            missionList = JsonConvert.DeserializeObject<List<Mission>>(missionlistdata);
            client.Dispose();
        }

        static void Install(string mission)
        {
            var queryresults = missionList.Where(a => a.Name.ToLower() == mission.ToLower() || a.BaseName.ToLower() == mission.ToLower()).ToList();
            if (queryresults.Count > 1)
            {
                Console.WriteLine("Multiple missions of given name have been found! Please use its base name to select:");
                for (int i = 0; i < queryresults.Count; i++)
                {
                    Mission result = queryresults[i];
                    Console.WriteLine($"{i + 1}.{result.name} [{result.baseName}]");
                }
                return;
            }
            if (queryresults.Count == 1)
            {
                Console.WriteLine("Downloading Mission");
                var mis = queryresults.First();
                if (!File.Exists(Program.DataFolder + "\\" + mis.baseName + ".zip"))
                    mis.downloadMission();

                if (File.Exists(PQPath + "\\platinum\\data\\missions\\custom\\" + mis.Artist + "\\" + mis.BaseName))
                {
                    Console.WriteLine("Mission is already installed!");
                    return;
                }

                Console.WriteLine("Installing Mission");
                var zip = ZipFile.OpenRead(Program.DataFolder + "\\" + mis.baseName + ".zip");

                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (!File.Exists(Path.Combine(PQPath + "\\platinum", entry.FullName)))
                        entry.ExtractToFile(Path.Combine(PQPath + "\\platinum", entry.FullName),false);
                }
                zip.Dispose();

                Directory.CreateDirectory(PQPath + "\\platinum\\data\\missions\\custom\\" + mis.Artist);
                File.Move(PQPath + "\\platinum\\data\\missions\\" + mis.BaseName, PQPath + "\\platinum\\data\\missions\\custom\\" + mis.Artist + "\\" + mis.BaseName);
                if (File.Exists(PQPath + "\\platinum\\data\\missions\\" + mis.BaseName.Substring(0,mis.baseName.Length - 3) + "png"))
                    File.Move(PQPath + "\\platinum\\data\\missions\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "png", PQPath + "\\platinum\\data\\missions\\custom\\" + mis.Artist + "\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "png");
                if (File.Exists(PQPath + "\\platinum\\data\\missions\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "jpg"))
                    File.Move(PQPath + "\\platinum\\data\\missions\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "jpg", PQPath + "\\platinum\\data\\missions\\custom\\" + mis.Artist + "\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "jpg");
                if (File.Exists(PQPath + "\\platinum\\data\\missions\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "prev.png"))
                    File.Move(PQPath + "\\platinum\\data\\missions\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "prev.png", PQPath + "\\platinum\\data\\missions\\custom\\" + mis.Artist + "\\" + mis.BaseName.Substring(0, mis.baseName.Length - 3) + "prev.png");

                Console.WriteLine($"Installed to {PQPath + "\\platinum\\data\\missions\\custom\\" + mis.Artist + "\\" + mis.BaseName}");

            }
            else
            {
                Console.WriteLine("No mission by provided name is found!");
                return;
            }
        }

        static void Search(string mission)
        {
            var queryresults = missionList.Where(a => a.name.ToLower().Contains(mission.ToLower()) || a.baseName.ToLower().Contains(mission.ToLower())).ToList();
            if (queryresults.Count > 0)
            {
                Console.WriteLine("Search Results:");
                for (int i = 0; i < queryresults.Count; i++)
                {
                    Mission result = queryresults[i];
                    Console.WriteLine($"{i + 1}.{result.name} [{result.baseName}]");
                }
                return;
            }
            else
            {
                Console.WriteLine("No mission by provided name is found!");
                return;
            }
        }

        static void Clean()
        {
            Console.WriteLine("Cleaned cache");
            foreach (var file in Directory.EnumerateFiles(Program.DataFolder))
            {
                if (Path.GetExtension(file) == "zip")
                    File.Delete(file);
                if (File.GetAttributes(file).HasFlag(FileAttributes.Directory))
                    File.Delete(file);
            }
        }

        static void InstallZip(string zippath)
        {
            var zip = ZipFile.OpenRead(zippath);
            string path = DataFolder + "\\" + Path.GetFileNameWithoutExtension(zippath);
            Directory.CreateDirectory(path);
            zip.ExtractToDirectory(path,true);

            var misfiles = GetRecursiveFileNamesOfExtension(path, ".mis");
            var diffiles = GetRecursiveFileNamesOfExtension(path, ".dif");

            foreach (var mis in misfiles)
            {
                Console.WriteLine($"Installing {Path.GetFileName(mis)}");
                InstallMis(mis, diffiles);
            }

        }

        static void InstallMis(string mispath,List<string> difs)
        {
            var importer = new Importer();
            var MissionGroup = importer.Import(mispath);
            InstallDifs(MissionGroup, difs);

            var artist = MissionGroup.Where(a => a.objname == "MissionInfo").First().dynamicFields.GetValueOrDefault("artist", "Unspecified Artist");
            Directory.CreateDirectory(PQPath + "\\platinum\\data\\missions\\custom\\" + artist);
            File.Copy(mispath, PQPath + "\\platinum\\data\\missions\\custom\\" + artist + "\\" + Path.GetFileName(mispath));
            if (File.Exists(Path.ChangeExtension(mispath,"png")))
                File.Copy(Path.ChangeExtension(mispath, "png"), PQPath + "\\platinum\\data\\missions\\custom\\" + artist + "\\" + Path.GetFileName(Path.ChangeExtension(mispath,"png")));
            if (File.Exists(Path.ChangeExtension(mispath, "prev.png")))
                File.Copy(Path.ChangeExtension(mispath, "prev.png"), PQPath + "\\platinum\\data\\missions\\custom\\" + artist + "\\" + Path.GetFileName(Path.ChangeExtension(mispath, "prev.png")));
            if (File.Exists(Path.ChangeExtension(mispath, "jpg")))
                File.Copy(Path.ChangeExtension(mispath, "jpg"), PQPath + "\\platinum\\data\\missions\\custom\\" + artist + "\\" + Path.GetFileName(Path.ChangeExtension(mispath, "jpg")));
        }

        static void InstallDifs(SimGroup group,List<string> difs)
        {
            foreach (var item in group)
            {
                if (item.GetType() == typeof(SimGroup))
                {
                    InstallDifs(item as SimGroup, difs);
                }
                if (item.GetType() == typeof(InteriorInstance))
                {
                    var interior = item as InteriorInstance;
                    var path = ParseInteriorPath(interior.interiorFile);
                    if (!File.Exists(path))
                    {
                        var interiorfilename = Path.GetFileName(path);
                        if (difs.Select(a => Path.GetFileName(a)).Contains(interiorfilename))
                        {
                            var dif = difs.Where(a => Path.GetFileName(a) == interiorfilename).First();
                            File.Copy(dif, path);
                        }
                        else
                            Console.WriteLine($"ERROR: Interior {path} not found");
                    }
                }
                if (item.GetType() == typeof(PathedInterior))
                {
                    var interior = item as PathedInterior;
                    var path = ParseInteriorPath(interior.interiorResource);
                    if (!File.Exists(path))
                    {
                        var interiorfilename = Path.GetFileName(path);
                        if (difs.Select(a => Path.GetFileName(a)).Contains(interiorfilename))
                        {
                            var dif = difs.Where(a => Path.GetFileName(a) == interiorfilename).First();
                            File.Copy(dif, path);
                        }
                        else
                            Console.WriteLine($"ERROR: Interior {path} not found");
                    }
                }
            }
        }

        static string ParseInteriorPath(string path)
        {
            path = path.Replace("/", "\\");
            path = path.Replace("~", "platinum");
            if (path.Contains("$usermods"))
            {
                path = "platinum" + path.Split(" @ ")[1].Trim('\"');

            }
            return Path.Combine(PQPath, path);
        }

        static List<string> GetRecursiveFileNamesOfExtension(string path,string extension)
        {
            var ret = new List<string>();
            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == extension)
                    ret.Add(file);
            }
            foreach (var dir in Directory.GetDirectories(path))
                ret.AddRange(GetRecursiveFileNamesOfExtension(dir, extension));
            return ret;
        }


    }
}
