using Varus.Paradox.Console.CustomInterpreter;

namespace Varus.Paradox.Console.Sample.Commands
{
    /// <summary>
    /// Sets the rotation speed for <see cref="Cube"/>.
    /// </summary>
    public class SetCubeRotationSpeed : Command
    {
        private readonly Cube _cube;

        /// <summary>
        /// Constructs a new instance of <see cref="SetCubeRotationSpeed"/>.
        /// </summary>
        /// <param name="cube">Cube to set rotation speed for.</param>
        public SetCubeRotationSpeed(Cube cube)
        {
            _cube = cube;
        }

        protected override void Try(CommandResult result, string[] args)
        {
            if (args.FailWhenLengthLessThan(3, result,
                "Expected x, y and z floating point numeric components for rotation speed.")) return;

            _cube.RotationSpeed = args.ToVector3();
        }
    }
}
