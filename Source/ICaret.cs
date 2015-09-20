namespace QuakeConsole
{
    public interface ICaret
    {
        /// <summary>
        /// Gets or sets the character index the cursor is at in the <see cref="InputBuffer"/>.
        /// </summary>
        int Index { get; set; }
    }
}