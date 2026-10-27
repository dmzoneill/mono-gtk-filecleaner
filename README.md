# FileCleaner

A Mono GTK# tool that cleans up filenames in a folder.

## What it does

You pick a folder and enter a pattern and a replacement. The tool walks the
folder and its subfolders and renames each file whose name matches, logging what
it did and any files it could not rename.

## Tech

- C# with Mono and GTK#
- Autotools-style `configure` and `Makefile`

## Building

```sh
./configure
make
```

You can also open `FileCleaner.sln` in MonoDevelop.

## License

Licensed under the Apache License 2.0. See [LICENSE](LICENSE).
