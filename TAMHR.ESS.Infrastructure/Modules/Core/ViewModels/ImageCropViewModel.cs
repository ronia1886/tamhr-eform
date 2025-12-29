namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ImageCropViewModel
    {
        public string imgUrl { get; set; }
        //public int imgInitW { get; set; }
        //public int imgInitH { get; set; }
        //public int imgW { get; set; }
        //public int imgH { get; set; }
        public double imgX1 { get; set; }
        public double imgX2 { get; set; }
        public double cropW { get; set; }
        public double cropH { get; set; }
    }
}
