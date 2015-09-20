using Varus.Paradox.Console.Interpreters.Custom;

namespace Varus.Paradox.Console.Sample.Commands
{
    /// <summary>
    /// Sets the position for <see cref="Cube"/>.
    /// </summary>
    public class SetCubePositionCommand : Command
    {
        private readonly Cube _cube;

        /// <summary>
        /// Constructs a new instance of <see cref="SetCubePositionCommand"/>.
        /// </summary>
        /// <param name="cube">Cube to set rotation speed for.</param>
        public SetCubePositionCommand(Cube cube)
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
