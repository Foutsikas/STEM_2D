namespace STEM2D.Core
{
    /// <summary>
    /// Interface for objects that can be interacted with during experiments.
    /// Implement this on any component that needs to be enabled/disabled
    /// based on the current experiment step.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Enable or disable interaction with this object.
        /// When disabled, the object should not respond to user input.
        /// </summary>
        /// <param name="interactable">True to enable interaction, false to disable</param>
        void SetInteractable(bool interactable);

        /// <summary>
        /// Check if this object can currently be interacted with.
        /// </summary>
        /// <returns>True if the object accepts interaction, false otherwise</returns>
        bool CanInteract();
    }
}