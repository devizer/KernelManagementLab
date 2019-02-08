using System;

namespace Universe.Curses
{
	public class ScreenBuffer
	{
		public int Width { get; set; }
		public int Height { get; set; }

		Character[] _Content;

		public ScreenBuffer (int width, int height) : this(width, height, Character.WhiteSpaceOnBlack.Foreground, Character.WhiteSpaceOnBlack.Background)
		{
//			Width = width;
//			Height = height;
//			int l = width * height;
//			_Content = new Character[l];
//			for (int i = 0; i < l; i++)
//				_Content [i] = Character.WhiteSpaceOnBlack;
		}

		public ScreenBuffer (int width, int height, Color fore, Color back)
		{
			Width = width;
			Height = height;
			var ch = new Character (fore, back, ' ');
			int l = width * height;
			_Content = new Character[l];
			for (int i = 0; i < l; i++)
				_Content [i] = ch;
		}

		public Character this[int x, int y]
		{
			get { return _Content [y * Width + x]; }
			set { _Content [y * Width + x] = value; }
		}

		public void Draw(Position pos, ScreenBuffer another)
		{
			for (int x0 = 0; x0 < another.Width; x0++)
				for (int y0 = 0; y0 < another.Height; y0++) {
					int x = pos.X + x0;
					int y = pos.Y + y0;

					if (x > 0 && x < Width && y > 0 && y < Height)
						this [x, y] = another [x0, y0];
				}
		}
	}
}

