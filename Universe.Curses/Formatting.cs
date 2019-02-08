using System;

namespace Universe.Curses
{
	public enum VerticalAlignment
	{
		Top,
		Middle,
		Bottom
	}

	public enum HorizontalAlignment
	{
		Near,
		Center,
		Far
	}

	public struct Position
	{
		public int X { get; set; }
		public int Y { get; set; }

		public Position (int x, int y) : this()
		{
			this.X = x;
			this.Y = y;
		}
		
	}

	public struct Size
	{
		public int Width { get; set; }
		public int Height { get; set; }

		public Size (int width, int height) : this()
		{
			this.Width = width;
			this.Height = height;
		}
		
	}

	public struct Padding
	{
		public int Left { get; set; }
		public int Top { get; set; }
		public int Right { get; set; }
		public int Bottom { get; set; }
	}

	public enum Color : byte
	{
		Black,
		DarkBlue,
		DarkGreen,
		DarkCyan,
		DarkRed,
		DarkMagenta,
		DarkYellow,
		Gray,
		DarkGray,
		Blue,
		Green,
		Cyan,
		Red,
		Magenta,
		Yellow,
		White,

		None = 127
		
	}
}

