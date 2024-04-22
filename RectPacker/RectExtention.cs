using System.Windows;

namespace HswDreamer;

public static class RectExtention
{
	public static double Area(this Rect rect) => rect.Width * rect.Height;
    public static bool Include(this Rect rect, Point pt) => pt.X > rect.Left && pt.X < rect.Right && pt.Y > rect.Top && pt.Y < rect.Bottom;
    public static Point TopMid(this Rect rect) => new((rect.Right - rect.Left) / 2, rect.Top);
    public static Point BottomMid(this Rect rect) => new((rect.Right - rect.Left) / 2, rect.Bottom);
    public static Point LeftMid(this Rect rect) => new(rect.Left, (rect.Bottom - rect.Top) / 2);
    public static Point RightMid(this Rect rect) => new(rect.Right, (rect.Bottom - rect.Top) / 2);
}