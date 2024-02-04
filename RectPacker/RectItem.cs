using System.Windows;

namespace HswDreamer;

public class RectItem
{
	public object Obj { get; init; }
	public Point Position { get; set; } = new Point();
	public double Width { get; set; }
	public double Height { get; set; }
	public double Size => Width * Height;
	public bool IsRotate { get; set; } = false;
	public bool IsFinish { get; set; } = false;
	public bool IsOverSize { get; set; } = false;
	public bool IsZeroSize { get; set; } = false;
	public bool IsNoStand { get; set; } = false;

	public RectItem(double width, double height, object obj)
	{
		Width = width;
		Height = height;
		Obj = obj;
	}
	public void Rotate()
	{
		var temp = Width;
		Width = Height;
		Height = temp;
		IsRotate = !IsRotate;
	}
}