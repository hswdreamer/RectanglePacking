using System.Windows;

namespace HswDreamer;

public static class RectExtention
{
	public static double Area(this Rect rect) => rect.Width * rect.Height;
	public static double Left(this Rect rect) => rect.Left;
	public static double Right(this Rect rect) => rect.Right;
	public static double Top(this Rect rect) => rect.Bottom;
	public static double Bottom(this Rect rect) => rect.Top;
	public static Point LeftTop(this Rect rect) => new Point(rect.Left(), rect.Top());
	public static Point RightTop(this Rect rect) => new Point(rect.Right(), rect.Top());
	public static Point LeftBottom(this Rect rect) => new Point(rect.Left(), rect.Bottom());
	public static Point RightBottom(this Rect rect) => new Point(rect.Right(), rect.Bottom());
}
