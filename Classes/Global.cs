using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp3.Classes
{
    public class Global
    {
        public static string BasePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        public static char SeparatorChar { get; set; } = System.IO.Path.DirectorySeparatorChar;
        public static string Path_temp { get; set; }
        public static string Path_output { get; set; }
        public static string Path_logo { get; set; }
        public static string Http { get; set; } = "http://thankful.top:4396";



        public static dynamic GetThumbnailPath(Stream stream)
        {
            var bp = SixLabors.ImageSharp.Image.Load(stream);

            var profile = bp.Metadata.ExifProfile?.Values;
            var meta = new Dictionary<string, object>();
            if (profile != null)
            {
                var meta_origin = profile.Select(x => new
                {
                    Key = x.Tag.ToString(),
                    Value = x.GetValue() is ushort[]? ((ushort[])x.GetValue())[0] : x.GetValue()
                });

                foreach (var item in meta_origin)
                {
                    meta[item.Key] = item.Value;
                }
                if (meta.ContainsKey("ExposureProgram"))
                {
                    meta["ExposureProgram"] = ExposureProgram[Convert.ToInt32(meta["ExposureProgram"])];
                }

                if (meta.ContainsKey("FNumber") && meta["FNumber"] is SixLabors.ImageSharp.Rational rational && rational.Denominator != 0)
                {
                    meta["FNumber"] = rational.Numerator * 1.0 / rational.Denominator;
                }
            }

            var config = GetDefaultExifConfig(meta);

            var right1 = config[2];
            var right2 = config[3];
            var left1 = config[0];
            var left2 = config[1];

            if (bp.Width <= 1920 || bp.Height <= 1080)
            {
                return new
                {
                    right1,
                    left1,
                    right2,
                    left2,
                }; ;
            }
            var xs = bp.Width / 1920M;

            var w = (int)(bp.Width / xs);
            var h = (int)(bp.Height / xs);
            bp.Mutate(x => x.Resize(w, h));
            //try
            //{
            //    bp.SaveAsJpeg(p);
            //}
            //catch { }
            return new
            {
                //path = p,
                right1,
                left1,
                right2,
                left2,
            };

        }

        public static Dictionary<int, string> ExposureProgram { get; set; } = new Dictionary<int, string>()
        {
            {0, "未知" },
            {1, "手动" },
            {2, "正常" },
            {3, "光圈优先" },
            {4, "快门优先" },
            {5, "创作程序(偏重使用视野深度)" },
            {6, "操作程序(偏重使用快门速度)" },
            {7, "纵向模式" },
            {8, "横向模式" },
        };

        public static List<string> GetDefaultExifConfig(Dictionary<string, object> meta)
        {
            var model = InitConfig();
            if (model == null) return new List<string>();
            var ls = new List<string>();

            foreach (var parent in model.Config)
            {
                var cs = new List<string>();
                foreach(var child in parent.Config)
                {
                    if (meta.TryGetValue(child.Key, out object rtl))
                    {
                        var c = child.Front + rtl + child.Behind;
                        cs.Add(c);
                    }
                    else
                    {
                        var c = child.Front + child.Value + child.Behind;
                        cs.Add(c);
                    }
                }

                var p = string.Join(" ", cs);
                ls.Add(p);
            }

            return ls;
        }

        public static MainModel InitConfig()
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("ExifConfig.json").Result;
            
            using (var reader = new StreamReader(stream))
            {
                var c = reader.ReadToEnd();
                var result = JsonConvert.DeserializeObject<MainModel>(c);
                return result ?? new MainModel();
            }
        }

        public static Dictionary<string, byte[]> FontResourrce { get; set; } = new Dictionary<string, byte[]>
        {
        //    { "金陵宋体", Properties.Resources.FZXiJinLJW },
        //    { "Pamega", Properties.Resources.Pamega_demo_2 },
        //    { "Hey-November", Properties.Resources.Hey_November_2},
        //    { "Facon", Properties.Resources.Facon_2 }
        };

        public static bool SaveConfig(string json)
        {
            try
            {
                File.WriteAllText(Global.BasePath + Global.SeparatorChar + "ExifConfig.json", json);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static void SendMsg(string msg)
        {
        }

        public static string Resolution { get; set; }

    }

    public class MainModel
    {
        public List<ExifInfo> Exifs { get; set; } = new List<ExifInfo>();

        public List<LeftTextList> Config { get; set; } = new List<LeftTextList>();

        public List<string> Icons { get; set; } = new List<string>();
        public bool ShowGuide { get; set; }
    }

    public class ExifInfo
    {
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
            }
        }

        public string Key { get; set; } = "";

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }

        private string _value = "";
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
            }
        }


    }


    public class ExifConfigInfo
    {
        public int SEQ { get; set; }
        public string? Front { get; set; }
        public string? Behind { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
    }

    public class LeftTextList
    {
        public LeftTextList()
        {

        }

        public LeftTextList(string text, ObservableCollection<ExifConfigInfo> config)
        {
            Text=text;
            Config=config;
        }

        public string Text { get; set; } = "";

        private ObservableCollection<ExifConfigInfo> config = new();
        public ObservableCollection<ExifConfigInfo> Config
        {
            get => config;
            set
            {
                config = value;
            }
        }
    }
}
