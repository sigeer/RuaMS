namespace Application.Core.Compatible.Extensions
{
    public static class RectangleExtensions
    {
        public static void setBounds(this Rectangle rect, int x, int y, int w, int h)
        {
            rect.X = x;
            rect.Y = y;
            rect.Width = w;
            rect.Height = h;
        }
    }
}
