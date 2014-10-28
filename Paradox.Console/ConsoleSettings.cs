using SiliconStudio.Core.Mathematics;

namespace Varus.Paradox.Console
{
    internal class ConsoleSettings
    {            
        public Color BackgroundColor { get; set; }
        public Color FontColor { get; set; }
        public float OpenCloseTransitionSeconds { get; set; }
        public float TimeUntilRepeatingInput { get; set; }
        public float RepeatingInputCooldown { get; set; }
        public float HeightRatio { get; set; }
        public bool Enabled { get; set; }
        public bool Visible { get; set; }
        public string InputPrefix { get; set; }
        public int NumPositionsToMoveWhenOutOfScreen { get; set; }
        public Color InputPrefixColor { get; set; }
        public float Padding { get; set; }
        public string CaretSymbol { get; set; }
        public float CaretBlinkingIntervalSeconds { get; set; }
    }
}
