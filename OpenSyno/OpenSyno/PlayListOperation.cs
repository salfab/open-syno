namespace OpenSyno
{
    public enum PlayListOperation
    {
        /// <summary>
        /// Replaces the playlist items with the specified items.
        /// </summary>
        ClearAndPlay,

        /// <summary>
        /// Insert the specified items after the current item.
        /// </summary>
        InsertAfterCurrent,

        /// <summary>
        /// Appends the specified items at the end of the playlist.
        /// </summary>
        Append
    }
}