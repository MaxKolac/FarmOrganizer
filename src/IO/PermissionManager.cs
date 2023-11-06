using FarmOrganizer.Services;

namespace FarmOrganizer.IO
{
    public static class PermissionManager
    {
        private const string _IOAlertNoPermissions = "Aby aplikacja mogła zresetować bazę danych, potrzebne są odpowiednie uprawnienia.\nJeżeli ten komunikat pokazuje się po zrestartowaniu aplikacji, możliwe że wymagane jest zresetowanie odmówionych uprawnień. Przejdź do ustawień swojego telefonu, a następnie w sekcji 'Aplikacje', odnajdź FarmOrganizer i nadaj mu uprawnienia do zapisu i odczytu plików.";

        /// <summary>
        /// <para>
        /// Checks if the user granted <see cref="Permissions.StorageWrite"/> and <see cref="Permissions.StorageRead"/>. 
        /// If any of those permissions turns out to be <see cref="PermissionStatus.Denied"/>, attempts to request it from the user through a pop-up. If the user denies the permissions, an alert is shown explaining that the app requires those permissions to function properly.
        /// </para>
        /// <para>
        /// <b>Important!</b> The pop-ups do not show on Android API level greater than 31. 
        /// According to the linked StackOverflow question, the <see cref="Permissions.StorageWrite"/> has been discontinued from Android API 33.
        /// The current workaround is purposefuly downgrading the app to API 31.
        /// </para>
        /// <para><see href="https://stackoverflow.com/a/75331176/21342746">Liyun Zhang's answer on StackOverflow</see></para>
        /// <para><see href="https://github.com/dotnet/maui/issues/11275">.NET MAUI GitHub Issue discussion regarding the workaround</see></para>
        /// </summary>
        /// <returns><c>true</c> if both permissions have been granted, <c>false</c> otherwise.</returns>
        public static async Task<bool> RequestPermissionsAsync()
        {
            var storageWritePerm = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            var storageReadPerm = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var writePermissionGranted = storageWritePerm == PermissionStatus.Granted;
            var readPermissionGranted = storageReadPerm == PermissionStatus.Granted;

            if (!writePermissionGranted)
            {
                storageWritePerm = await Permissions.RequestAsync<Permissions.StorageWrite>();
                writePermissionGranted = storageWritePerm == PermissionStatus.Granted;
            }
            if (!readPermissionGranted)
            {
                storageReadPerm = await Permissions.RequestAsync<Permissions.StorageRead>();
                readPermissionGranted = storageReadPerm == PermissionStatus.Granted;
            }

            if (!writePermissionGranted || !readPermissionGranted)
                PopupExtensions.ShowAlert(App.PopupService, "Błąd", _IOAlertNoPermissions);

            return writePermissionGranted && readPermissionGranted;
        }
    }
}