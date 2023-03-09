using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MauiApp3.Class
{
    public class CharacterWatermarkProperty
    {
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string Content { get; set; } = "";

        private int x = 0;
        public int X
        {
            get => x;
            set
            {
                x = value;
            }
        }
        private int y = 0;
        public int Y
        {
            get => y;
            set
            {
                y = value;
            }
        }

        private int slope = 0;
        public int Slope
        {
            get => slope;
            set
            {
                slope = value;
            }
        }

        private int fontSize = 10;
        public int FontSize
        {
            get => fontSize;
            set 
            { 
                fontSize = value;
            }
        }

        private string color = "#000000";
        public string Color
        {
            get => color;
            set
            {
                color = value;
                if (value[2] == value[1] && value[1] == 'F')
                {
                    color = "#" + color.Substring(3);
                }
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                if (Bold && Italic) return FontStyle.BoldItalic;
                else if (Bold) return FontStyle.Bold;
                else if(Italic) return FontStyle.Italic;
                else return FontStyle.Regular;
            }
        }

        private string fontFamily = "微软雅黑";
        public string FontFamily
        {
            get => fontFamily;
            set
            {
                fontFamily = value;
            }
        }

        private bool bold = false;
        public bool Bold
        {
            get => bold;
            set
            {
                bold = value;
            }
        }

        private bool italic = false;
        public bool Italic
        {
            get=> italic;
            set
            {
                italic = value;
            }
        }
    }
}
