using System;
using System.Linq;
using HswDreamer;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sample;

public class MainViewModel : ReactiveObject
{
	[Reactive] public double CanvasWidth { get; set; } = 0;
	[Reactive] public double CanvasHeight { get; set; } = 0;

	public IReactiveCommand RunCommand { get; set; }
	public MainViewModel()
	{
		RunCommand = ReactiveCommand.Create<Canvas>(Run);
	}

	private void Run(Canvas canvas)
	{
		RectPacker rectPacker = new(1000, 500);
		List<RectItem> Items = new();
		Random rnd = new Random();
		for (int i = 0; i < 200; i++)
		{
			// CASE 1
			//var w = rnd.Next(1000);
			//var h = rnd.Next(500);
			//if (rnd.Next(100) == 1)
			//	w += 100;
			//if (rnd.Next(100) == 1)
			//	h += 100;

			// CASE 2
			//var w = rnd.Next(80) + 20;
			//var h = rnd.Next(80) + 20;

			// CASE 3
			var w = rnd.Next(100) + 20;
			var h = rnd.Next(200) + 20;

			var color = new Color() { A = 255, R = (byte)(rnd.Next(200) + 55), G = (byte)(rnd.Next(200) + 55), B = (byte)(rnd.Next(200) + 55) };
			var item = new RectItem(w, h, color);
			Items.Add(item);
		}
			
		Stopwatch timer = Stopwatch.StartNew();
		var Document = rectPacker.Packing(Items);
		timer.Stop();

		StringBuilder sb = new();
		sb.AppendLine($"작업시간 : {timer.Elapsed}");
		sb.AppendLine($"불량 : {Document[0].Count}");
		sb.AppendLine($"페이지 : {Document.Count - 1}");
		MessageBox.Show(sb.ToString(), "Result");
		Document.Remove(0);

		double maxX = 0;
		foreach (var page in Document)
		{
			double pageWidth = 0;
            List<Rect?> rects = new List<Rect?>();
            foreach (var item in page.Value)
			{
				Rectangle rect = new Rectangle();
				rect.Width = item.Width;
				rect.Height = item.Height;
				rect.Fill = new SolidColorBrush((Color)item.Obj);
				Canvas.SetTop(rect, item.Position.Y);
				Canvas.SetLeft(rect, item.Position.X + maxX);
				canvas.Children.Add(rect);
				pageWidth = Math.Max(pageWidth, item.Width + item.Position.X);

				// 검증
				var find = rects.FirstOrDefault(x =>
				{
                    Rect r = new(item.Position, new Size(item.Width, item.Height));
                    r.Intersect(x!.Value);
					if (r.IsEmpty)
	                    return false;
                    return r.Width * r.Height > 0;
				}, null);

                rects.Add(new(item.Position, new Size(item.Width, item.Height)));


                if (find != null)
				{
                    MessageBox.Show("Error");
					Debug.WriteLine("");
                }
            }
			maxX += pageWidth;
		}
		CanvasHeight = 500;
		CanvasWidth = maxX;
		canvas.InvalidateVisual();
	}

}
