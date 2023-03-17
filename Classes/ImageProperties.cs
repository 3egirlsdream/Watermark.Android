namespace Watermark.Class
{
    public class ImageProperties
    {

        public ImageProperties(string _path, string _name)
        {
            Path = _path;
            Name = _name;
            Config = new ImageConfig();
            ID = Guid.NewGuid().ToString("N").ToUpper();
        }

        public string ID { get; set; }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
            }
        }

        private string thumbnailPath;
        /// <summary>
        /// 缩略图路径
        /// </summary>
        public string ThumbnailPath
        {
            get { return thumbnailPath; }
            set
            {
                thumbnailPath = value;
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }

        private ImageConfig config;
        public ImageConfig Config
        {
            get=> config;
            set
            {
                config = value;
            }
        }

        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked= value;
            }
        }
    }
}
