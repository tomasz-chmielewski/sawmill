using System;
using System.IO;

namespace Sawmill.Common.IO
{
    public class LineReader : StreamReader
    {
        public LineReader(Stream stream, int maxLineLength) : base(stream)
        {
            this.MaxLineLength = maxLineLength > 0
                ? maxLineLength
                : throw new ArgumentException($"{nameof(maxLineLength)} must be greater then zero.", nameof(maxLineLength));

            this.Buffer = new char[maxLineLength + 1];
        }

        private int MaxLineLength { get; }
        private char[] Buffer { get; }
        private int DataLength { get; set; }
        private int Position { get; set; }
        private int LineStartPosition { get; set; }
        private bool AcceptLine { get; set; } = true;

        public override string ReadLine()
        {
            do
            {
                while(this.Position < this.DataLength)
                {
                    var c = this.Buffer[this.Position];
                    if (c == '\n' || c == '\r')
                    {
                        if((this.LineStartPosition == this.Position) || !this.AcceptLine)
                        {
                            this.Position++;
                            this.LineStartPosition = this.Position;
                            this.AcceptLine = true;
                            continue;
                        }

                        var line = new string(this.Buffer, this.LineStartPosition, this.Position - this.LineStartPosition);

                        this.Position++;
                        this.LineStartPosition = this.Position;

                        return line;
                    }

                    this.Position++;
                }
            }
            while (this.FeedBuffer() > 0);

            return null;
        }

        private int FeedBuffer()
        {
            if(this.Position == this.Buffer.Length)
            {
                if (this.LineStartPosition > 0)
                {
                    this.AcceptLine = true;
                    this.ShiftData();
                }
                else
                {
                    this.AcceptLine = false;
                    this.ResetBuffer();

                    throw new InvalidDataException($"Line is longer then {this.MaxLineLength} characters.");
                }
            }

            var charCount = base.Read(this.Buffer, this.Position, this.Buffer.Length - this.Position);
            this.DataLength += charCount;
            return charCount;
        }

        private void ShiftData()
        {
            Array.Copy(this.Buffer, this.LineStartPosition, this.Buffer, 0, this.Position - this.LineStartPosition);
            this.Position -= this.LineStartPosition;
            this.LineStartPosition = 0;
            this.DataLength = this.Position;
        }

        private void ResetBuffer()
        {
            this.Position = 0;
            this.LineStartPosition = 0;
            this.DataLength = 0;
        }
    }
}
