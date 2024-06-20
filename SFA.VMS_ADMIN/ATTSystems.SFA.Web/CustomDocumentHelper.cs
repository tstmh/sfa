using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
namespace ATTSystems.SFA.Web
{
    public static class CustomDocumentHelper
    {
        public static Cell CreateTextCell(string columnName, UInt32 rowIndex, string cellValue, UInt32 styleIndex)
        {
            Cell cell = new Cell
            {
                DataType = CellValues.InlineString,
                StyleIndex = styleIndex,
                CellReference = columnName + rowIndex
            };

            InlineString inlineString = new InlineString();
            Text t = new Text { Text = cellValue };
            inlineString.AppendChild(t);

            cell.AppendChild(inlineString);
            return cell;
        }
        public static Stylesheet GenerateStylesheet()
        {
            Stylesheet? styleSheet = null;


            //Fonts fonts = new Fonts(
            //    new Font( // Index 0 - default
            //        new FontSize() { Val = 11 }

            //    ),
            //    new Font( // Index 1 - header
            //        new FontSize() { Val = 11 },
            //        new Bold(),
            //        new Color() { Rgb = "FFFFFF" }

            //    ));

            Fonts fonts = new Fonts(
                new Font(                                                               // Index 0 - The default font.
                    new FontSize() { Val = 11 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Calibri" }),

                new Font(                                                               // Index 1 - The bold font.
                    new Bold(),
                    new FontSize() { Val = 11 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Calibri" }),

                new Font(                                                               // Index 2 - The white font.
                                                                                        //new Italic(),
                    new FontSize() { Val = 11 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "FFFFFF" } },
                    new FontName() { Val = "Calibri" }),

                new Font(                                                               // Index 3 - The Times Roman font. with 16 size
                    new FontSize() { Val = 16 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Times New Roman" }),

                new Font(
                    new Bold(),                                                         // add by sanj
                    new FontSize() { Val = 14 },                                        // index 4 - Title Font
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Calibri" }),


                new Font(
                    new Bold(),
                    new FontSize() { Val = 13 },                                        // index 5 - Title Font
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Calibri" })


                );




            Fills fills = new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0 - default
                    new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1 - default
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "001f67" } })
                    { PatternType = PatternValues.Solid }) // Index 2 - header

                );



            Borders borders = new Borders(
                    new Border(), // index 0 default
                    new Border( // index 1 black border
                        new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder()),
                    new Border(// index 2 black border
                        new LeftBorder(new Color() { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder()),
                    new Border( // index 3 black border
                        new LeftBorder(new Color() { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder()),
                    new Border( // index 4 black border
                        new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                );

            Border thinBorder = new Border(
         new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
         new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
         new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
         new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
         new DiagonalBorder());

            borders.Append(thinBorder);


            CellFormats cellFormats = new CellFormats(
                    new CellFormat(), // default
                    new CellFormat { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 1 - default 
                    new CellFormat { FontId = 4, FillId = 0, BorderId = 0, ApplyFont = true }, // index 2 - Title
                    new CellFormat { FontId = 2, FillId = 2, BorderId = 2, ApplyFont = true, ApplyFill = true, ApplyBorder = true, Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center } }, // index 3 - header1
                    new CellFormat { FontId = 2, FillId = 2, BorderId = 2, ApplyFont = true, ApplyFill = true, ApplyBorder = true }, // index 4 - header2
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }, // index 5 - body
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 3, ApplyBorder = true }, // index 6 - body  - no left border
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, Alignment = new Alignment { Vertical = VerticalAlignmentValues.Top } }, // index 7 - body - align top
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 4, ApplyBorder = true }, // index 8 - body  - no Right border
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Left } }, // index9  - body
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, Alignment = new Alignment { WrapText = true } }, // index 10 - body
                    new CellFormat { FontId = 2, FillId = 2, BorderId = 2, ApplyFont = true, ApplyFill = true, ApplyBorder = true, Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center } }, // index 11 - header1, // index 4 - header2
                    new CellFormat { FontId = 5, FillId = 0, BorderId = 1, ApplyBorder = true, Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Right } } // index 12 - body
                );


            styleSheet = new Stylesheet(fonts, fills, borders, cellFormats);

            return styleSheet;
        }
        public static string ColumnLetter(int intCol)
        {
            var intFirstLetter = ((intCol) / 676) + 64;
            var intSecondLetter = ((intCol % 676) / 26) + 64;
            var intThirdLetter = (intCol % 26) + 65;

            var firstLetter = (intFirstLetter > 64) ? (char)intFirstLetter : ' ';
            var secondLetter = (intSecondLetter > 64) ? (char)intSecondLetter : ' ';
            var thirdLetter = (char)intThirdLetter;

            return string.Concat(firstLetter, secondLetter, thirdLetter).Trim();
        }
    }
}
