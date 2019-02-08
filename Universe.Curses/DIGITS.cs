using System;
using System.Linq;
using System.Collections.Generic;

namespace Universe.Curses
{
	public class DIGITS
	{
		public DIGITS ()
		{
		}

		public static readonly string L1 = "XXX  #  ###";
		public static readonly string L2 = "X X  #    #";
		public static readonly string L3 = "X X  #    #";
		public static readonly string L4 = "X X ### ";
		public static readonly string L5 = "X X";
		public static readonly string L6 = "XXX ";

		static readonly string ALL_2 = @"
 █  ▀▀█
 █  █▀▀
■█■ █■■
";

		public static readonly string ALL = @"
o █▀█ ▄█  ▀▀█ ▀▀█ █ █ █▀▀ █▀▀ █▀█ █▀█ █▀█      
  █ █  █  █▀▀ ▀▀█ ▀▀█ ▀▀█ █▀█   █ █▀█ ▀▀█ ▀▀▀ ╳
  █▄█ ▄█▄ █▄▄ ▄▄█   █ ▄▄█ █▄█   █ █▄█ ▄▄█      
                             Temperature
";

		public static ScreenBuffer CreateBuffer(string text, Color fore, Color back)
		{
			var all = AllAsArray.Value;
			int w = 0;
			const string digree = "°";
			int[] width = new[] { 1, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1 };
			string chars = digree + "0123456789-*";

			foreach (char c in text) {
				int pos = chars.IndexOf (c.ToString ());
				if (pos < 0)
					continue;

				w += width [pos];
				w += 1;
			}

			if (text.Length > 0)
				w -= 1;

			ScreenBuffer buf = new ScreenBuffer (w, 3);
			int x = 0;
			foreach (char c in text) {
				int pos = chars.IndexOf (c.ToString ());
				if (pos < 0)
					continue;

				int source0 = 0;
				for (int i = 0; i < pos; i++)
					source0 += width [i] + (i > 0 ? 1 : 1);

				for (int ox = 0; ox < width [pos]; ox++)
					for (int oy = 0; oy < buf.Height; oy++) {
						buf [x + ox, oy] = new Character (fore, back, all[oy][source0+ox]);
					}


				x += width [pos] + 1;
			}

			return buf; 


		}


		static Lazy<List<string>> AllAsArray = new Lazy<List<string>>(() => {
			return ALL.Split (new[] { '\r', '\n' }).Select (x => x).Where (x => x.Length > 0).ToList ();
		});


	}
}

