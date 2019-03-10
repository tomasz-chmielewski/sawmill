using System.IO;
using System.Text;

namespace Sawmill.Common.IO
{
    public class LineReader : StreamReader
    {
        public LineReader(Stream stream) : base(stream, Encoding.ASCII)
        {
        }

        public LineReader(Stream stream, int bufferSize) : base(stream, Encoding.ASCII, false, bufferSize)
        {
        }

        private string data = string.Empty;
        private int position = 0;
        private int lineStartPosition = 0;
        private bool skipNewLineChars = true;

        private char[] Buffer { get; } = new char[1024];
        private int BufferPosition { get; set; } = 0;

        public override string ReadLine()
        {
            do
            {
                for (; this.position < this.data.Length; this.position++)
                {
                    var c = this.data[this.position];
                    if (c == '\n' || c == '\r')
                    {
                        if(this.skipNewLineChars)
                        {
                            this.lineStartPosition = this.position + 1;
                            continue;
                        }

                        var line = this.data.Substring(this.lineStartPosition, this.position - this.lineStartPosition);

                        this.data = this.data.Substring(this.position + 1);
                        this.skipNewLineChars = true;
                        this.lineStartPosition = 0;
                        this.position = 0;

                        return line;
                    }
                    else
                    {
                        this.skipNewLineChars = false;
                    }
                }
            }
            while (this.FeedData() > 0);

            return null;
        }

        private int FeedData()
        {
            var charCount = base.Read(this.Buffer, 0, this.Buffer.Length);
            if(charCount > 0)
            {
                this.data += new string(this.Buffer, 0, charCount);
            }

            return charCount;
        }
    }
}
