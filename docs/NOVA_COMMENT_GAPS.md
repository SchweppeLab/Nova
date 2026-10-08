# Nova documentation comment gaps

Measured against the `v1.1.0` tag. Excluded from the Jekyll build.

## Result

Every public member of Nova 1.1.0 carries an XML doc comment: 215 of 215.

The same measure found 86 of 216 at `v1.0.0.18`.

Every API page on the website can therefore quote the library. No page needs to be
written from the implementation.

## What was counted

Public members of public types: properties, methods, constructors, events, fields,
interface members and enum values.

A member is complete when it has a `<summary>`, a `<param>` for each parameter, and a
`<returns>` if it returns a value. `<inheritdoc/>` counts as complete.

The four public delegates in `IPC.Pipes` are declared outside any type, so the scan does
not count them. They were checked by hand and are documented.

Excluded:

- Internal types, which the website does not document
- Code inside block comments, such as the unused `MetaItem` class
- `NovaApp`, which is not part of either package

The scan checks that the tags are present. It does not judge whether a comment is good.

## Next release

Rerun against the new tag. Any member listed has a page that must be written from the
implementation until Nova documents it, and those pages carry a one-line note saying so.

The scan is `tools/gapscan2.pl`. It takes one C# file and prints one line per public
member, ending in its missing tags or "ok". Run it on every `.cs` file at the tag, leaving out
`NovaApp`, `Examples` and `Test`:

```
git -C <Nova clone> archive v1.1.0 | tar -x -C <empty folder>
find <empty folder> -name '*.cs' ! -path '*NovaApp*' ! -path '*Examples*' ! -path '*Test*' \
  -exec perl tools/gapscan2.pl {} \;
```
