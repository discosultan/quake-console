using System.IO;

namespace QuakeConsole
{
    internal class OutputBufferWriter : StreamWriter
    {
        private readonly IConsoleOutput _output;

        public OutputBufferWriter(Stream s, IConsoleOutput output)
            : base(s)
        {
            _output = output;            
        }        

        public override void Write(string value)
        {            
            if (value != "\r\n")
                _output.Append(value);            
        }
    }    
}
