using Varus.Paradox.Console.CustomInterpreter;

namespace Varus.Paradox.Console.Sample.Commands
{
    /// <summary>
    /// Sets the position for <see cref="Cube"/>.
    /// </summary>
    public class SetCubePosition : Command
    {
        private readonly Cube _cube;

        /// <summary>
        /// Constructs a new instance of <see cref="SetCubePosition"/>.
        /// </summary>
        /// <param name="cube">Cube to set rotation speed for.</param>
        public SetCubePosition(Cube cube)
        {
            _cube = cube;
        }

        protected override void Try(CommandResult result, string[] args)
        {
            if (args.FailWhenLengthLessThan(3, result,
                "Expected x, y and z floating point numeric components for position.")) return;

            _cube.Position = args.ToVector3();
        }
    }
}
