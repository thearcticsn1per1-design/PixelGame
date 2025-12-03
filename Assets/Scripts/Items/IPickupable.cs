namespace PixelGame
{
    /// <summary>
    /// Interface for items that can be picked up by the player
    /// </summary>
    public interface IPickupable
    {
        /// <summary>
        /// Called when player picks up this item
        /// </summary>
        /// <param name="player">The player who picked it up</param>
        void Pickup(PlayerController player);
    }
}
