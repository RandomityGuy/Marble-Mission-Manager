using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace mmm
{
    public class Mission
    {

        public int id;
        public string name;
        public string desc;
        public string artist;
        public string modification;
        public string gameType;
        public string baseName;
        public int gems;
        public int? difficulty;
        public float? rating;
        public int? weight;
        public bool egg;
        public string bitmap;
        bool isDownloading = false;
        public int Id
        {
            get => id;
        }
        public string Name
        {
            get => name;
        }
        public string Desc { get => desc; }
        public string Artist { get => artist; } 
        public string Modification { get => modification;  }
        public string GameType { get => gameType;  }
        public string BaseName { get => baseName;  }
        public int Gems { get => gems;  }
        public bool Egg { get => egg;  }
        public float? Rating { get => rating; }
        public int? Difficulty { get => difficulty; }
        public int? RatingWeight { get => weight; }
        public void downloadMission()
        {
            if (!isDownloading)
            {
                var client = new WebClient();
                client.DownloadFile(new Uri("https://cla.higuy.me/api/v1/missions/" + Id.ToString() + "/zip"), Program.DataFolder + "\\" + baseName + ".zip");
                Console.WriteLine("Downloaded " + new FileInfo(Program.DataFolder + "\\" + baseName + ".zip").Length/1024 + "kB package");
            }
        }
    }
}
