# FarmOrganizer
.NET MAUI Android app in MVVM architecture, utilizing EntityFramework Core with SQLite as the database provider. Made for farmers and growers to digitalize and simplify the organization of their daily agricultural work and crop sales.
# "Duck tape" solutions
List of workaroudns that currently replace officially adopted solutions due to their bugged functionality. Once the official solutions are fixed, these workarounds should be replaced.

### `DataTemplateSelector` doesn't work at all
See the [#28](https://github.com/MaxKolac/FarmOrganizer/pull/28).
It is currently replaced by a simple `Converter`. Workaround used in a `DataTemplate` of `BalanceLedger` model class in `LedgerPage` to dynamically change the `TextColor` property, according to the current `BalanceChange` property.

### App is deliberately downgraded to target Android API 31, instead of 33
See related issues: [#29](https://github.com/MaxKolac/FarmOrganizer/issues/29), [#15](https://github.com/MaxKolac/FarmOrganizer/issues/15) and [#32](https://github.com/MaxKolac/FarmOrganizer/pull/32). Android API 32 has deprecated the `WRITE_EXTERNAL_STORAGE` and `READ_EXTERNAL_STORAGE`, which are essential in `DatabaseFile` and `PdfBuilder`. New permissions in Android API 33 aren't fully supported by .NET MAUI. CommunityToolkitMaui's `FilePicker` and `FolderPicker` work partially when reading a file, however writing is yet to work correctly. On every permission request, `RequestPermissionsAsync()` always returns `PermissionStatus.Denied` regardless of whether or not it is a first request. The current workaround is targeting lower version of Android API.

### \[Not merged yet\] `PdfBuilder` throws exceptions when rendering in a Release build, due to trimming and AOT issues
Question asked [here](https://stackoverflow.com/questions/77344191/how-to-trim-dependencies-in-a-net-maui-android-app), it is yet to be answered... :(
