using System.IO;

namespace Varus.Paradox.Console.PythonInterpreter
{
    internal class PythonOutputBufferWriter : StreamWriter
    {
        private readonly OutputBuffer _output;

        public PythonOutputBufferWriter(Stream s, OutputBuffer output)
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
