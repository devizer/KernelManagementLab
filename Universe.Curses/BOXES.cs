using System;

namespace Universe.Curses
{
	public class BOXES
	{
		public BOXES ()
		{
		}

		static string AllThin = @"─│┌└┐┘┴┬";
		static string AllThik = @"━┃┏┗┓┛┻┳";

		public static ScreenBuffer CreateBox(string caption, Size size, bool thin = true, int captionPadding = 2, Color fore = Color.White, Color back = Color.White)
		{
			ScreenBuffer ret = new ScreenBuffer (size.Width, size.Height, fore, back);

			string all = thin ? AllThin : AllThik;

			for (int x = 0; x < size.Width; x++) {
				ret [x, 2] = new Character(fore, back, all [0]); 
				ret [x, size.Height-1] = new Character(fore, back, all [0]); 
			}

			for (int y = 2; y < size.Height; y++) {
				ret [0, y] = new Character(fore, back, all [1]); 
				ret [size.Width - 1, y] = new Character(fore, back, all [1]); 
			}

			ret[0,2] = new Character(fore, back, all [2]); 
			ret[0,size.Height-1] = new Character(fore, back, all [3]); 
			ret[size.Width-1, 2] = new Character(fore, back, all [4]); 
			ret[size.Width-1, size.Height-1] = new Character(fore, back, all [5]);

			ret[captionPadding, 2] = new Character(fore, back, all [6]);
			ret[size.Width - 1 - captionPadding, 2] = new Character(fore, back, all [6]);

			for (int x = captionPadding; x < size.Width - captionPadding; x++) {
				ret [x, 0] = new Character(fore, back, all [0]); 
			}

			ret[captionPadding, 0] = new Character(fore, back, all [2]); 
			ret[size.Width - 1 - captionPadding, 0] = new Character(fore, back, all [4]); 
			ret[captionPadding, 1] = new Character(fore, back, all [1]); 
			ret[size.Width - 1 - captionPadding, 1] = new Character(fore, back, all [1]); 

			for (int i = 0; i < caption.Length; i++) {
				int p = size.Width / 2 - caption.Length / 2 + i;
				if (p > captionPadding && p < size.Width - captionPadding) {
					ret[p, 1] = new Character(fore, back, caption[i]);
				}
			}


			return ret;
		}
	}
}

