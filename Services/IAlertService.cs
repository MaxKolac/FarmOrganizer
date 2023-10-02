namespace FarmOrganizer.Services
{
    /// <summary>
    /// <para>
    /// Service class usable by viewmodels to display simple alerts and confirmation alerts to the user. 
    /// </para>
    /// <see href="https://stackoverflow.com/questions/72429055/how-to-displayalert-in-a-net-maui-viewmodel"/>
    /// </summary>
    public interface IAlertService
    {
        // ----- async calls (use with "await" - MUST BE ON DISPATCHER THREAD) -----
        /// <summary>
        /// Asynchrous method - Must be used on dispatcher thread! Shows a simple alert to the user with one simple Cancel button.
        /// </summary>
        /// <param name="title">The title to use in the alert.</param>
        /// <param name="message">The message to show to the user.</param>
        /// <param name="cancel">The text of the Cancel button. Default value is "OK".</param>
        Task ShowAlertAsync(string title, string message, string cancel = "OK");

        /// <summary>
        /// Asynchrous method - Must be used on dispatcher thread! Shows an alert to the user with two buttons for Accept and Cancel.
        /// </summary>
        /// <param name="title">The title to use in the alert.</param>
        /// <param name="message">The message to show to the user.</param>
        /// <param name="accept">The text of the Accept button. Default value is "Yes".</param>
        /// <param name="cancel">The text of the Cancel button. Default value is "No".</param>
        /// <returns>A <c>Task</c> that contains the user's choice as a Boolean value.</returns>
        Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No");

        // ----- "Fire and forget" calls -----

        /// <summary>
        /// "Fire and forget" synchrous method. Returns BEFORE showing alert. 
        ///  Shows a simple alert to the user with one simple Cancel button.
        /// </summary>
        /// <inheritdoc cref="ShowAlertAsync(string, string, string)"/>
        void ShowAlert(string title, string message, string cancel = "OK");
        /// <param name="callback">Action to perform afterwards.</param>

        /// <summary>
        /// "Fire and forget" synchrous method. Returns BEFORE showing alert.
        /// Shows an alert to the user with two buttons for Accept and Cancel.
        /// </summary>
        /// <inheritdoc cref="ShowConfirmationAsync(string, string, string, string)"/>
        void ShowConfirmation(string title, string message, Action<bool> callback,
                              string accept = "Yes", string cancel = "No");
    }
}
