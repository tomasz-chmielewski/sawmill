using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sawmill
{
    public class LogStreamReader : StreamReader
    {
        public LogStreamReader(Stream stream) : base(stream, Encoding.ASCII)
        {
        }

        public LogStreamReader(Stream stream, int bufferSize) : base(stream, Encoding.ASCII, false, bufferSize)
        {
        }

        private string reminder = null;

        public override string ReadLine()
        {
            var line = base.ReadLine();
            return ProcessReminder(line);
        }

        public override Task<string> ReadLineAsync()
        {
            return base.ReadLineAsync().ContinueWith(s => this.ProcessReminder(s.Result));
        }

        private string ProcessReminder(string line)
        {
            if (this.EndOfStream)
            {
                this.reminder = string.Concat(this.reminder, line);
                line = null;
            }
            else if (this.reminder != null)
            {
                line = this.reminder + line;
                this.reminder = null;
            }

            return line;
        }
    }
}
