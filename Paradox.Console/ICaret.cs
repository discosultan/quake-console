namespace Varus.Paradox.Console
{
    public interface ICaret
    {
        /// <summary>
        /// Gets or sets the index the cursor is at in the <see cref="InputBuffer"/>.
        /// </summary>
        int Index { get; }
    }
}