using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HswDreamer;


public class Document() : Dictionary<int, List<RectItem>> { }
public class RectPacker
{
	private record class CanInsertItem(RectItem Item, bool IsRotate);
	private record class IsIncludeItem(bool IsInclude, bool IsRoate);
	private List<Rect> Boards { get; set; } = new();
	private double Width, Height, MinLenth;
	private List<RectItem> Targets = null!;
	private void UpdateMinLenth() => MinLenth = Targets.Any() ? Targets.Min(x => Math.Min(x.Width, Width)) : double.MaxValue;
	public RectPacker(double width, double height)
	{
		Width = width;
		Height = height;
	}
	public Document Packing(List<RectItem> items)
	{
		items = Init(items);
		Document ret = new();

		ret.Add(0, items.Where(x => x.IsFinish == true).ToList());
		Targets = items.Where(x => x.IsFinish == false).ToList();
		UpdateMinLenth();
		while (Targets.Count > 0)
		{
			var result = PagePacking();
			if (result.Count == 0)   // 무한루프 방지 예외처리. 들어오면 안됨
				break;
			ret.Add(ret.Count, result);
		}
		return ret;
	}
	private List<RectItem> Init(List<RectItem> items)
	{
		List<RectItem> ret = new List<RectItem>();
		/*
         * 좌측 하단이 0,0 이고 (1사분면)
         * 우로 X축(Width), 위로 Y축(Height)
         */
		var zeros = items.Where(x => x.Width == 0 || x.Height == 0).ToList();
		foreach (var zero in zeros)
		{
			zero.IsZeroSize = true;
			zero.IsFinish = true;
			ret.Add(zero);
			items.Remove(zero);
		}

		var overs = items.Where(x => (x.Width > Width && x.Width > Height) || (x.Height > Width && x.Height > Height) ||
									(x.Width > Width && x.Height > Width) || (x.Width > Height && x.Height > Height)).ToList();
		foreach (var over in overs) // 돌려도 들어 올수 없는 객체들
		{
			over.IsOverSize = true;
			over.IsFinish = true;  // 처리 안함
			ret.Add(over);
			items.Remove(over);
		}

		if (items.Count == 0)
			return ret;

		if (Width > Height) // 페이지의 가로가 더 길 때
		{
			var noStands = items.Where(x => (x.Width > Height || x.Height > Height)).OrderByDescending(x => x.Size).ToList();
			foreach (var item in noStands)   // 눕혀서 밖에 넣을수 없는 것들
			{
				if (item.Height > item.Width)   // 눕힌다.
					item.Rotate();

				item.IsNoStand = true;

				ret.Add(item);           // 누워야 하는 것들을 우선 처리 한다.
				items.Remove(item);
			}
		}
		var other = items.OrderByDescending(x => x.Size).ToList();

		foreach (var item in other)
		{
			if (item.Width > item.Height)   // 세운다. 그래야 가로가 짧아짐
				item.Rotate();

			ret.Add(item);
		}

		return ret;
	}
	private List<RectItem> PagePacking()
	{
		var ret = new List<RectItem>();
		Boards.Clear();
		Boards.Add(new Rect(new Point(0, 0), new Size(Width, Height)));   // 페이지만한 보드를 만든다
		double maxWidth = 0;
		for (int i = 0; i < Boards.Count; i++)           // 보드 전체를 순회
		{
			Rect? board = Boards[i];

			while (board != null)
			{
				if (Targets.Count == 0)
					return ret;
				var canInsert = CanInsert(board.Value);  // 선택된 보드에 들어갈 수 있는 item을 가져 온다
				if (canInsert.Count == 0)
					break;
				var old = board;
				foreach (var insert in canInsert)
				{
					if (insert.IsRotate == true && insert.Item.IsNoStand == false)     // 해당 보드에 눕혀서만 들어갈 수 있는 아이템이고 세울수 있다면
					{
						double xPos = board.Value.Left() + insert.Item.Height; // 보드 시작 + 아이템 높이 (회전해서 들어갈 것이므로)
						if ((maxWidth + insert.Item.Width) < xPos)    // 회전한 후 길어질 폭이 현재 최대 폭 + 아이템 폭(세운상태에서 폭) 보다 더 높아지면 
							continue;                                // 안눕히고 다음에 찾는다
						insert.Item.Rotate();
					}
					if (insert.Item.Width == MinLenth || insert.Item.Height == MinLenth)
						UpdateMinLenth();
					insert.Item.Position = board.Value.LeftBottom();
					insert.Item.IsFinish = true;
					Targets.Remove(insert.Item);
					ret.Add(insert.Item);
					maxWidth = Math.Max(maxWidth, insert.Item.Position.X + insert.Item.Width);

					BoardDevide(board.Value, insert.Item.Width, insert.Item.Height);        // 아이템이 들어간 박스를 쪼갠다. 쪼개서 위쪽에 남는 공간이 있으면 리턴 된다
					if (Boards.Count == 0)
					{
						board = null;
						break;
					}
					i = -1;
					Boards = Boards.OrderBy(x => x.Left()).ToList();
					board = Boards[0];
					break;
				}

				if (board == old)         // 보드가 안바뀐건 넣을게 없다는 뜻. 다음 보드로 이동
					break;
			}
		}

		return ret;
	}
	private void BoardDevide(Rect board, double width, double height)
	{
		Boards.Remove(board);

		Rect rightBoard = new Rect(new Point(board.Left() + width, board.Bottom()), new Size(board.Width - width, board.Height));
		Rect topBoard = new Rect(new Point(board.Left(), board.Bottom() + height), new Size(board.Width, board.Height - height));

		BoardUpdate(new Rect(board.LeftBottom(), new Size(width, height)));

		Add(rightBoard, topBoard);
	}
	private void Add(params Rect[] board)
	{
		List<Rect> list = new();
		foreach (var item in board)
		{
			if (item.Width < MinLenth || item.Height < MinLenth)
				continue;
			if (CheckInclude(item))
				list.Add(item);
		}
		foreach (var item in list)
		{
			Boards.Add(item);
		}
	}
	private void BoardUpdate(Rect board)
	{
		Rect old;
		for (int i = 0; i < Boards.Count; i++)
		{
			var result = IsContains(Boards[i], board);
			if (result == 0)
				continue;

			old = Boards[i];
			Boards.RemoveAt(i);
			i--;
			switch (result)
			{
				case 1:
					Add(Row1(old, board), Column1(old, board), Column2(old, board));
					break;
				case 2:
					Add(Row1(old, board), Column1(old, board), Row2(old, board));
					break;
				case 3:
					Add(Column1(old, board), Row2(old, board), Column2(old, board));
					break;
				case 4:
					Add(Row1(old, board), Column2(old, board), Row2(old, board));
					break;
				case 5:
					Add(Row1(old, board), Column2(old, board));
					break;
				case 6:
					Add(Row1(old, board), Column1(old, board));
					break;
				case 7:
					Add(Column1(old, board), Row2(old, board));
					break;
				case 8:
					Add(Row2(old, board), Column2(old, board));
					break;
				case 9:
					Add(Row1(old, board));
					break;
				case 10:
					Add(Column1(old, board));
					break;
				case 11:
					Add(Row2(old, board));
					break;
				case 12:
					Add(Column2(old, board));
					break;
				case 13:
					Add(Column1(old, board), Column2(old, board));
					break;
				case 14:
					Add(Row1(old, board), Row2(old, board));
					break;
			}
		}
	}
	private Rect Row1(Rect b1, Rect b2) => new Rect(b1.LeftBottom(), new Size(b1.Width, b2.Bottom() - b1.Bottom()));
	private Rect Row2(Rect b1, Rect b2) => new Rect(new Point(b1.Left(), b2.Top()), new Size(b1.Width, b1.Top() - b2.Top()));
	private Rect Column1(Rect b1, Rect b2) => new Rect(b1.LeftBottom(), new Size(b2.Left() - b1.Left(), b1.Height));
	private Rect Column2(Rect b1, Rect b2) => new Rect(new Point(b2.Right(), b1.Bottom()), new Size(b1.Right() - b2.Right(), b1.Height));
	private bool CheckInclude(Rect board)
	{
		for (int i = 0; i < Boards.Count; i++)
		{
			if (board.Contains(Boards[i]))
			{
				Boards.RemoveAt(i);
				i--;
			}
		}
		foreach (var b in Boards)
		{
			if (b.Contains(board))
				return false;
		}
		return true;
	}
	private List<CanInsertItem> CanInsert(Rect bounds)
	{
		var ret = new List<CanInsertItem>();
		foreach (var item in Targets)
		{
			var result = CanInclude(bounds, item);
			if (result.IsInclude)
				ret.Add(new CanInsertItem(item, result.IsRoate));
		}

		return ret;
	}
	private int IsContains(Rect b1, Rect b2)
	{
		if (IsContains(b1, b2.LeftBottom()) && IsContains(b1, b2.RightBottom()) && !IsContains(b1, b2.LeftTop()) && !IsContains(b1, b2.RightTop()))         // 12시방향
			return 1;
		else if (IsContains(b1, b2.LeftTop()) && IsContains(b1, b2.LeftBottom()) && !IsContains(b1, b2.RightTop()) && !IsContains(b1, b2.RightBottom()))    // 3시방향
			return 2;
		else if (IsContains(b1, b2.LeftTop()) && IsContains(b1, b2.RightTop()) && !IsContains(b1, b2.LeftBottom()) && !IsContains(b1, b2.RightBottom()))    // 6시방향
			return 3;
		else if (IsContains(b1, b2.RightTop()) && IsContains(b1, b2.RightBottom()) && !IsContains(b1, b2.LeftTop()) && !IsContains(b1, b2.LeftBottom()))    // 9시방향
			return 4;
		else if (IsContains(b1, b2.RightBottom()) && !IsContains(b1, b2.LeftBottom()) && !IsContains(b1, b2.LeftTop()) && !IsContains(b1, b2.RightTop()))   // 11시방향
			return 5;
		else if (IsContains(b1, b2.LeftBottom()) && !IsContains(b1, b2.RightBottom()) && !IsContains(b1, b2.LeftTop()) && !IsContains(b1, b2.RightTop()))   // 1시방향
			return 6;
		else if (IsContains(b1, b2.LeftTop()) && !IsContains(b1, b2.RightTop()) && !IsContains(b1, b2.LeftBottom()) && !IsContains(b1, b2.RightBottom()))   // 5시방향
			return 7;
		else if (IsContains(b1, b2.RightTop()) && !IsContains(b1, b2.LeftTop()) && !IsContains(b1, b2.LeftBottom()) && !IsContains(b1, b2.RightBottom()))   // 7시방향
			return 8;
		else if (IsContainsTop(b1, b2))                                                                                                                     // 위 덮음
			return 9;
		else if (IsContainsLeft(b1, b2))                                                                                                                    // 왼 덮음
			return 10;
		else if (IsContainsBottom(b1, b2))                                                                                                                  // 아 덮음
			return 11;
		else if (IsContainsRight(b1, b2))                                                                                                                   // 오 덮음
			return 12;
		else if (b1.Left() < b2.Left() && b1.Right() > b2.Right() && b1.Bottom() >= b2.Bottom() && b1.Top() <= b2.Top())                                    // 종 덮음
			return 13;
		else if (b1.Bottom() < b2.Bottom() && b1.Top() > b2.Top() && b1.Left() >= b2.Left() && b1.Right() <= b2.Right())                                    // 횡 덮음
			return 14;

		return 0;
	}
	private bool IsContains(Rect b, Point v) => v.X > b.Left() && v.X < b.Right() && v.Y > b.Bottom() && v.Y < b.Top();
	private bool IsContainsTop(Rect b1, Rect b2) => b2.Top() >= b1.Top() && b2.Bottom() > b1.Bottom() && b2.Bottom() < b1.Top() && b2.Left() <= b1.Left() && b2.Right() >= b1.Right();
	private bool IsContainsRight(Rect b1, Rect b2) => b2.Right() >= b1.Right() && b2.Left() > b1.Left() && b2.Left() < b1.Right() && b2.Bottom() <= b1.Bottom() && b2.Top() >= b1.Top();
	private bool IsContainsLeft(Rect b1, Rect b2) => b2.Left() <= b1.Left() && b2.Right() < b1.Right() && b2.Right() > b1.Left() && b2.Bottom() <= b1.Bottom() && b2.Top() >= b1.Top();
	private bool IsContainsBottom(Rect b1, Rect b2) => b2.Bottom() <= b1.Bottom() && b2.Top() < b1.Top() && b2.Top() > b1.Bottom() && b2.Left() <= b1.Left() && b2.Right() >= b1.Right();
	private IsIncludeItem CanInclude(Rect bounds, RectItem item)
	{
		if (item.IsFinish)
			return new IsIncludeItem(false, false);
		if (item.IsNoStand)
		{
			return new IsIncludeItem(bounds.Width >= item.Width && bounds.Height >= item.Height, false);
		}
		else
		{
			if (bounds.Width >= item.Width && bounds.Height >= item.Height)
				return new IsIncludeItem(true, false);
			if (bounds.Width >= item.Height && bounds.Height >= item.Width)
				return new IsIncludeItem(true, true);
		}
		return new IsIncludeItem(false, false);
	}
}