# FarmOrganizer
.NET MAUI Android app in MVVM architecture, utilizing EntityFramework Core with SQLite as the database provider. Made for farmers and growers to digitalize and simplify the organization of their daily agricultural work and crop sales.
# "Duck tape" solutions
List of workaroudns that currently replace officially adopted solutions due to their bugged functionality. Once the official solutions are fixes, these workarounds should be removed.

### `DataTemplateSelector` doesn't work at all
See the [#28](https://github.com/MaxKolac/FarmOrganizer/pull/28).
It is currently replaced by a simple `Converter`. Workaround used in a `DataTemplate` of `BalanceLedger` model class in `LedgerPage` to dynamically change the `TextColor` property, according to the current `BalanceChange` property.

### App is deliberately downgraded to target Android API 31, instead of 33
See related issues: [#29](https://github.com/MaxKolac/FarmOrganizer/issues/29), [#15](https://github.com/MaxKolac/FarmOrganizer/issues/15) and [#32](https://github.com/MaxKolac/FarmOrganizer/pull/32). Android API 32 has deprecated the `WRITE_EXTERNAL_STORAGE` and `READ_EXTERNAL_STORAGE`, which are essential in `DatabaseFile` and `PdfBuilder`. New permissions in Android API 33 aren't fully supported by .NET MAUI. CommunityToolkitMaui's `FilePicker` and `FolderPicker` work partially when reading a file, however writing is yet to work correctly. On every permission request, `RequestPermissionsAsync()` always returns `PermissionStatus.Denied` regardless of whether or not it is a first request. The current workaround is targeting lower version of Android API.

### SwipeViewItems have their `Mode` property set to `Execute`
See the [#36](https://github.com/MaxKolac/FarmOrganizer/issues/36) and [#40](https://github.com/MaxKolac/FarmOrganizer/pull/40). In the current version of .NET MAUI, swiping more than one `SwipeViewItem` in the same direction when it's used in a `DataTemplate` of a `CollectionView` throws a `Java.Lang.IllegalStateException`. Current workaround is enforcing that each `SwipeViewItem` has the property `Mode` set to `Execute`, forcing the item to swipe back and lowering the chance that the user accidentally has more than two items swipped at once.

### \[Not merged yet\] `PdfBuilder` throws exceptions when rendering in a Release build, due to trimming and AOT issues
