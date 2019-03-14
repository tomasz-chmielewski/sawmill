using System;
using System.IO;

namespace Sawmill.Common.IO
{
    public class LineReader : StreamReader
    {
        public LineReader(Stream stream) : base(stream)
        {
        }

        private int position = 0;
        private int lineStartPosition = 0;
        private bool acceptLine = true;

        private char[] Buffer { get; } = new char[1024 + 1];
        private int DataLength { get; set; } = 0;

        public override string ReadLine()
        {
            do
            {
                while(this.position < this.DataLength)
                {
                    var c = this.Buffer[this.position];
                    if (c == '\n' || c == '\r')
                    {
                        if((this.lineStartPosition == this.position) || !this.acceptLine)
                        {
                            this.position++;
                            this.lineStartPosition = this.position;
                            this.acceptLine = true;
                            continue;
                        }

                        var line = new string(this.Buffer, this.lineStartPosition, this.position - this.lineStartPosition);

                        this.position++;
                        this.lineStartPosition = this.position;

                        return line;
                    }

                    this.position++;
                }
            }
            while (this.FeedBuffer() > 0);

            return null;
        }

        private int FeedBuffer()
        {
            if(this.position == this.Buffer.Length)
            {
                if (this.lineStartPosition > 0)
                {
                    Array.Copy(this.Buffer, this.lineStartPosition, this.Buffer, 0, this.position - this.lineStartPosition);
                    this.acceptLine = true;
                    this.position -= this.lineStartPosition;
                    this.lineStartPosition = 0;
                    this.DataLength = this.position;
                }
                else
                {
                    this.acceptLine = false;
                    this.position = 0;
                    this.lineStartPosition = 0;
                    this.DataLength = 0;
                }
            }

            var charCount = base.Read(this.Buffer, this.position, this.Buffer.Length - this.position);
            this.DataLength += charCount;
            return charCount;
        }
    }
}
