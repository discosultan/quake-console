using System;

namespace Varus.Paradox.Console.CustomInterpreter
{
    /// <summary>
    /// Command which can be executed by <see cref="CustomCommandInterpreter"/>.
    /// </summary>
    public abstract class Command : ICommand
    {
        /// <inheritdoc />
        public CommandResult Execute(string[] args)
        {
            var result = CommandResult.Default;

            try
            {
                result.IsFaulted = false;
                result.Message = null;                
                Try(result, args);
            }
            catch (Exception e)
            {
                result.IsFaulted = true;
                result.Message = e.Message;
                Catch(result, e);
            }
            finally
            {
                Finally(result);
            }

            return result;
        }

        protected abstract void Try(CommandResult result, string[] args);

        protected virtual void Catch(CommandResult result, Exception e) { }

        protected virtual void Finally(CommandResult result) { }
    }
}
