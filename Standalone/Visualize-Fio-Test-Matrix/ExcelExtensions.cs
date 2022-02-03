using System;
using System.Drawing;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace VisualizeFioTestMatrix
{
    static class ExcelExtensions
    {
        public static ExcelWorksheet GetOrAddByName(this ExcelWorksheets worksheets, string sheetName)
        {
            return
                worksheets.FirstOrDefault(x => x.Name == sheetName)
                ?? worksheets.Add(sheetName);
        }

        public static void SetStyleAndColor(
            this ExcelBorderItem border, 
            ExcelBorderStyle borderStyle,
            Color? color = null)
        {
            if (border == null) throw new ArgumentNullException(nameof(border));
            border.Style = borderStyle;
            if (borderStyle != ExcelBorderStyle.None)
            {
                if (color.HasValue)
                    border.Color.SetColor(color.Value);
                else
                    border.Color.SetAuto();
            }
        }

        public static ExcelRange IgnoreNumberStoredAsText(this ExcelRange cells)
        {
            var ignoreErrors = cells.Worksheet.IgnoredErrors.Add(cells);
            ignoreErrors.NumberStoredAsText = true;
            return cells;
        }
        
        public static ExcelRange SetAllBorders(this ExcelRange cells, ExcelBorderStyle borderStyle, Color? color)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            Border brd = cells.Style.Border;
            foreach (ExcelBorderItem borderSide in new[] { brd.Right, brd.Left, brd.Top, brd.Bottom })
            {
                borderSide.Style = borderStyle;
                if (borderStyle != ExcelBorderStyle.None)
                {
                    if (color.HasValue)
                        borderSide.Color.SetColor(color.Value);
                    else
                        borderSide.Color.SetAuto();
                }
            }

            return cells;
        }
    }
}
