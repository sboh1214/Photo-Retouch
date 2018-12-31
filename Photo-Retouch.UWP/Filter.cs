namespace Photo_Retouch.UWP
{
    public partial class PictureClass
    {
        public Pixel[,] ReverseFilter(Pixel[,] InArr, Pixel[,] OutArr, int row, int col)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    OutArr[i, j].r = InArr[i, j].g;
                    OutArr[i, j].g = InArr[i, j].b;
                    OutArr[i, j].b = InArr[i, j].r;
                }
            }
            return OutArr;
        }
    }
}
