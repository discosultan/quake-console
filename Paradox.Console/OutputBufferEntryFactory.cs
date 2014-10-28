using Varus.Paradox.Console.Utilities;

namespace Varus.Paradox.Console
{
    internal class OutputBufferEntryFactory : IFactory<OutputBufferEntry>
    {
        private readonly OutputBuffer _viewBuffer;

        public OutputBufferEntryFactory(OutputBuffer viewBuffer)
        {
            _viewBuffer = viewBuffer;
        }

        public OutputBufferEntry New()
        {
            return new OutputBufferEntry(_viewBuffer);
        }
    }
}
