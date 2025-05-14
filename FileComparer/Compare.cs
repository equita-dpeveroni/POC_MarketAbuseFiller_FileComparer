namespace FileComparer
{
	internal class Compare
	{
		public Compare(Line? lineNEW, Line line)
		{
			this.LineNEW = lineNEW;
			this.LineOLD = line;
		}

		public bool Match => this.LineNEW != null && this.LineOLD != null;
		
		public Line? LineNEW { get; }
		
		public Line LineOLD { get; }
	}
}
