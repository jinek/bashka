# Bashka plugin packages

Run Bashka and choose **Plugins** from the roots menu (`Alt+F1` for the left
panel or `Alt+F2` for the right panel). The catalog searches nuget.org for the
`bashka-plugin` tag, including prerelease packages. Name shows the package ID,
Extension shows `Installed`, or `Update` when a newer version exists, and is blank
for available entries. Available and Update entries use the Executable category.
Local packages supply their size and file date;
search results do not include size or publication date.

The catalog uses `PathCatalog` for navigation and loading, with a single
`plugins` root. Its `IReadStreamCatalog` and `IWriteStreamCatalog` implementations
provide package downloads and validated package imports.

Feed searches and downloads use
[BaGetter.Protocol](https://github.com/bagetter/BaGetter/tree/main/src/BaGetter.Protocol).
The feed returns `SearchResult` directly; version selection uses its included
versions together with locally stored versions, without additional requests.
NuGet's resolver and packaging libraries handle dependency resolution and extraction.

- **Enter** opens a version picker. Confirming downloads the selected version,
  resolves its runtime dependencies with NuGet, and extracts them into the store.
  Runtime-excluded dependencies remain supplied by Bashka. IOC activation is not
  implemented yet.
- **F3** opens the selected package's metadata in Bashka's read-only text viewer.
  Close it with `Escape` to return to the catalog.
- **F5**, from Plugins to a writable catalog in the other panel, downloads
  the selected version's `.nupkg` through a temporary file deleted when closed.
  Existing destination files are replaced
  only after confirming the copy dialog.
- **F5**, from a readable filesystem/archive catalog to Plugins in the other
  panel, validates and stores `.nupkg` files locally. Packages must carry the
  `bashka-plugin` tag. Copying is not installation.
- **F8** uninstalls the selected installed version and deletes its local package
  after confirmation, or deletes a copied package that has not been installed.
  It does not unlist or delete anything on nuget.org. Published packages remain
  visible as Available after their local copies are deleted.

Mark multiple packages to copy or delete them together. Installed means the selected
version has been extracted and recorded. Update means that version is installed
and a newer version exists. Other entries are Available, including copied but
uninstalled packages.
When browsing offline, locally copied packages remain visible.

Local packages live in `Bashka/PluginPackages` under
`Environment.SpecialFolder.ApplicationData`. Installation always downloads fresh
archives into the system temporary directory and deletes them after success or
failure. It does not reuse local archives or previously extracted packages.
Archives explicitly imported with F5 remain user copies, separate from installation.

Each installed plugin has an `installed/<package-id>/installation.json` record
containing its original NuGet ID, version, framework, runtime identifier,
relative entry assembly path, and resolved dependency identities. Extracted
packages retain their files and `.nuspec` metadata, without archives. Feed and
store entries merge by package ID, with the installed version selected by default.
One version per plugin ID is installed at a time; validation finishes in a staging
directory before replacing an existing installation.
Extracted package folders use IDs without versions (`packages/<package-id>/`);
versions remain in the installation record and `.nuspec` metadata.

`PluginPackageStore.GetInstalledPlugins()` returns the persistent identities.
`GetEntryAssemblyPath(identity)` returns the compatible `<PackageId>.dll` path
for future IOC scanning. These work after reopening the store and do not depend
on the feed or any package archive.
