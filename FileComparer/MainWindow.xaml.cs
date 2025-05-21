namespace FileComparer
{
	using System;
	using System.IO;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Interop;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Compare_Click(object sender, RoutedEventArgs e)
		{
			var fiTradeNEW = new FileInfo("C:\\Temp\\FTB_TRADE_DB_20250430.txt");
			var fiTradeOLD = new FileInfo("C:\\Temp\\FTB_TRADE_DB_20250430_PROD.txt");
			Compare(fiTradeNEW, fiTradeOLD);

			var fiOrderNEW = new FileInfo("C:\\Temp\\FTB_ORDER_DB_20250430.txt");
			var fiOrderOLD = new FileInfo("C:\\Temp\\FTB_ORDER_DB_20250430_PROD.txt");
			Compare(fiOrderNEW, fiOrderOLD);
		}

		private void Compare(FileInfo fiNEW, FileInfo fiOLD)
		{
			var linesNEW = this.GetDicLines(fiNEW);
			var linesOLD = this.GetDicLines(fiOLD);

			Dictionary<int, Compare> dicCompare = CompareTrade(linesNEW, linesOLD);

			var onlyInNew = dicCompare.Where(x => x.Value.LineOLD == null).ToList();
			var onlyInOld = dicCompare.Where(x => x.Value.LineNEW == null).ToList();
			var inBoth = dicCompare.Where(x => x.Value.Match).ToList();
			var inBothSameCount = inBoth.Where(x => x.Value.LineNEW?.Count == x.Value.LineOLD?.Count).ToList();

			//var oldLinesIT0003497176 = dicCompare.Where(kvp => kvp.Value.LineOLD?.LineText.Contains("IT0003497176") ?? false).ToList();

			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("  *** COMPARE RESULT ***");
			stringBuilder.AppendLine("--------------------------------------");
			stringBuilder.AppendLine($"File NEW: {fiNEW.FullName}");
			stringBuilder.AppendLine($"File OLD: {fiOLD.FullName}");
			stringBuilder.AppendLine("--------------------------------------");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Only in NEW: {onlyInNew.Count}");
			foreach (var kvp in onlyInNew)
				stringBuilder.AppendLine($"  -> ({kvp.Value.LineNEW?.LineNumber}) {kvp.Value.LineNEW?.OriginalLineText}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine($"Only in OLD: {onlyInOld.Count}");
			foreach (var kvp in onlyInOld)
				stringBuilder.AppendLine($"  -> ({kvp.Value.LineOLD?.LineNumber}) {kvp.Value.LineOLD?.OriginalLineText}");

			// Dump the StringBuilder content to a file
			File.WriteAllText($"C:\\Temp\\Compare{fiNEW.Name.Replace(fiNEW.Extension, string.Empty)}_{DateTime.Now:yyyyMMdd-HHmmss}.txt", stringBuilder.ToString());
		}

		private static Dictionary<int, Compare> CompareTrade(Dictionary<int, Line> linesNEW, Dictionary<int, Line> linesOLD)
		{
			var dicCompare = new Dictionary<int, Compare>();
			foreach (var lineHash in linesNEW.Keys)
			{
				Compare? compare = null;
				Line? lineOLD = null;
				if (linesOLD.ContainsKey(lineHash))
					lineOLD = linesOLD[lineHash];
				compare = new Compare(linesNEW[lineHash], lineOLD);
				dicCompare.Add(lineHash, compare);
			}
			foreach (var lineHash in linesOLD.Keys)
				if (dicCompare.ContainsKey(lineHash) == false)
				{
					Line? lineNEW = null;
					if (linesNEW.ContainsKey(lineHash))
						lineNEW = linesNEW[lineHash];
					var compare = new Compare(lineNEW, linesOLD[lineHash]);
					dicCompare.Add(lineHash, compare);
				}

			return dicCompare;
		}

		private Dictionary<int, Line> GetDicLines(FileInfo fileInfo)
		{
			var lines1 = File.ReadAllLines(fileInfo.FullName);

			IEnumerable<Line> lines;
			if(fileInfo.Name.Contains("TRADE"))
				lines = lines1.Select((line, index) => new Line(line, index + 1, fileInfo));
			else
				lines = lines1.Select((line, index) => GetOrderLine(line, index + 1, fileInfo));

			var kvpLinesByHash = lines
														.GroupBy(line => line.Hash)
														.Select(g => new KeyValuePair<int, Line>(g.Key, this.GroupLine(g)));
			return kvpLinesByHash.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		private Line GetOrderLine(string orifinalLine, int index, FileInfo fileInfo)
		{
			string workingLine;
			if (orifinalLine == string.Empty 
				|| orifinalLine.StartsWith("LOADENTITY")
				|| orifinalLine.StartsWith("FIELDSEQ")				
				|| orifinalLine.StartsWith("ENDLOAD")
				)
				workingLine = orifinalLine;
			else
			{
				workingLine = string.Empty;
				var cells = orifinalLine.Split("\t");
				for (int i = 0; i < cells.Length; i++)
				{
					if (i != 18 //Order.ExecutedQty
							&& i != 13 //Order.OrderStatus
						)
						workingLine = string.Concat(workingLine, cells[i]);
				}	
			}

			
			return new Line(orifinalLine, workingLine, index + 1, fileInfo);
		}

		private Line GroupLine(IEnumerable<Line> group)
		{ 
			var line = group.First();
			line.SetCounter(group.Count());
			return line;
		}
	}
}