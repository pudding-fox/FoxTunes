namespace FoxTunes
{
    public class ImageItem : PersistableComponent
    {
        public ImageItem()
        {

        }

        public ImageItem(string fileName, string imageType)
        {
            this.FileName = fileName;
            this.ImageType = imageType;
        }

        public string FileName { get; set; }

        public string ImageType { get; set; }
    }
}
