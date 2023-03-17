﻿using Watermark.Class;
using System.Collections.ObjectModel;

namespace Watermark.Class
{
    public class ImageConfig 
    {
        public ImageConfig()
        {
        }


        private string leftPosition1 = "";
        /// <summary>
        /// 左侧第一行文字
        /// </summary>
        public string LeftPosition1
        {
            get => leftPosition1;
            set
            {
                leftPosition1 = value;
            }
        }


        private string leftPosition2 = "";
        /// <summary>
        /// 左侧第二行文字
        /// </summary>
        public string LeftPosition2
        {
            get => leftPosition2;
            set
            {
                leftPosition2 = value;
            }
        }

        public bool IsCloudIcon { get; set; } = false;

        private string logoName = "";
        /// <summary>
        /// LOGO名
        /// </summary>
        public string LogoName
        {
            get=> logoName;
            set
            {
                logoName = value;
            }
        }


        private string rightPosition1 = "";
        /// <summary>
        /// 右侧第一行文字
        /// </summary>
        public string RightPosition1
        {
            get => rightPosition1;
            set
            {
                rightPosition1 = value;
            }
        }

        private string rightPosition2 = "";
        /// <summary>
        /// 右侧第二行文字
        /// </summary>
        public string RightPosition2
        {
            get => rightPosition2;
            set
            {
                rightPosition2 = value;
            }
        }


        private int borderWidth = 0;
        /// <summary>
        /// 边框宽度 (%)
        /// </summary>
        public int BorderWidth
        {
            get=> borderWidth;
            set
            {
                borderWidth = value;
            }
        }


        private string backgroundColor = "#FFFFFF";
        /// <summary>
        /// 背景底色
        /// </summary>
        public string BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                if (value[2] == value[1] && value[1] == 'F')
                {
                    backgroundColor = "#" + backgroundColor.Substring(3);
                }
            }
        }

        private string row1FontColor = "#000000";
        /// <summary>
        /// 第一行字体颜色
        /// </summary>
        public string Row1FontColor
        {
            get => row1FontColor;
            set
            {
                row1FontColor = value;
                if (value[2] == value[1] && value[1] == 'F')
                {
                    row1FontColor = "#" + row1FontColor.Substring(3);
                }                                                         
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

        private int rorateCount = 0;
        public int RotateCount
        {
            get => rorateCount;
            set
            {
                rorateCount = value % 4;
            }
        }


        private ObservableCollection<CharacterWatermarkProperty> characterWatermarks = new ObservableCollection<CharacterWatermarkProperty>();
        /// <summary>
        /// 文字水印
        /// </summary>
        public ObservableCollection<CharacterWatermarkProperty> CharacterWatermarks
        {
            get => characterWatermarks;
            set
            {
                characterWatermarks = value;
            }
        }

    }
}
