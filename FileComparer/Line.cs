namespace FileComparer
{
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;

	internal class Line
	{
		public Line(string originalLine, int lineNumber, FileInfo fileInfo)
		{
			this.LineNumber = lineNumber;
			this.WorkingLineText = originalLine;
			this.OriginalLineText = originalLine;
			this.FileInfo = fileInfo;
			this.Hash = GenerateHash(originalLine);
		}

		public Line(string originalLine, string workingLine, int lineNumber, FileInfo fileInfo)
		{
			this.LineNumber = lineNumber;
			this.WorkingLineText = originalLine;
			this.OriginalLineText = originalLine;
			this.FileInfo = fileInfo;
			this.Hash = GenerateHash(workingLine);
		}

		public int LineNumber { get; }
		public string WorkingLineText { get; }
		public string OriginalLineText { get; }
		public FileInfo FileInfo { get; }
		public int Hash { get; }

		public int Count { get; private set; }

		internal void SetCounter(int count)
			=> this.Count = count;

		private int GenerateHash(string input)
		{
			using var sha256 = SHA256.Create();
			var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
			return BitConverter.ToInt32(bytes, 0);
		}
	}
}
