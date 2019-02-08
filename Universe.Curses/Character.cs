using System;

namespace Universe.Curses
{
	public struct Character
	{
		public Color Foreground { get; private set; }
		public Color Background { get; private set; }
		public Char Char { get; private set; }

		public Character (Color foreground, Color background, char @char) : this()
		{
			Foreground = foreground;
			Background = background;
			Char = @char;
		}


		public static readonly Character WhiteSpaceOnBlack = new Character(Color.White, Color.Black, (char)32);
		
	}
}

