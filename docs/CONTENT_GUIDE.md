# Nova Website Content Guide

This file records the working rules for the Nova website. It is the authoritative
reference. If something here is wrong or out of date, edit this file, because it is
what future work will be checked against.

This file is excluded from the Jekyll build. The `exclude` list in `_config.yml`
contains `docs`, so nothing here is published.

## Scope

The website lives on the `gh-pages` branch of the Nova repository. All website work
happens on that branch and nowhere else.

Other branches may be read. The `main` branch is the source of truth for the Nova
library itself, so documentation work reads from it. No website work ever writes to
`main` or to any other branch.

## What counts as an official release

Only official releases may be linked from the website.

Official releases carry a tag matching `v*`, for example `v1.0.0.18`. The very first
release predates that convention and is tagged `Nova-release`. Those are the only
tags the website may reference.

Development builds carry tags matching `dev-*`, including `dev-latest`. These are
pre-release and unvetted. They are never linked, never mentioned, and never used as
a documentation source. Anyone who wants them can find them in the repository.

Work that has been merged to `main` but not yet tagged is treated as unreleased.
It stays off the website until a release tag goes live, no matter how finished it
looks.

## Documentation currency

API documentation always describes the most recent official release. When the
library and the documentation disagree, the release tag wins, not the tip of `main`.

To document a release, read the source at that release tag rather than at `main`,
because `main` may contain work that has not shipped yet.

**Document the public surface only.** A type declared `internal` is not part of the
API, cannot be constructed by anyone using the library, and gets no page. When a type
changes access between releases, its page follows: a type that becomes public gets a
page, and a type that becomes internal loses one.

This is not always obvious from the folder layout. At v1.0.0.18 `ThermoRawReader` is
public while `MzMLReader`, `MzXMLReader` and `MGFReader`, sitting beside it, are
internal. Check the declaration, never the location.

One exception, by the owner's decision: `FileReader.CheckFile` is public in 1.1.0 but
should have been private, and becomes private in the next release. It is left off the
site without comment. Do not restore it.

**Verify every documented member against the source.** Do not carry members forward
on trust, and do not copy them from a sibling page. Read the type at the release tag
and check each name, each signature and each return type against it.

The first review found five pages documenting `FirstScanNumber` and `LastScanNumber`,
properties that have never existed in the library under those names, and one page
documenting a `Close()` method that is not a public member. Those errors had been
published for a year. A name that is almost right is worse than a missing page,
because it fails at compile time in the reader's editor rather than here.

The namespaces page states which release the documentation describes. That line is
generated, not typed: `_layouts/namespaces.html` reads `site.downloads | last` and
prints its `version` field. Adding a release entry updates it automatically.

It is guarded by `{% if latest_release.version %}`, so a download entry missing its
`version` field makes the line disappear silently rather than print something wrong.
That is the safer failure, but it does mean the field is not optional in practice.

**Inherited members are shown on the inheriting page.** A type's page lists every
member a reader can use on it, grouped under "Inherited from ..." headings. Only
members inherited from Nova types are listed, never those of .NET base classes.

Those tables are written once, in `_data/members/<DeclaringType>.yml`, and rendered by
`_includes/members.html` on every page that has them, so the copies cannot drift. `@T`
in a type is replaced by the include's `t` parameter (e.g. `SpecDataPointEx`).

A method page belongs to the declaring type: `TSpectrum.GetMz`, not `SpectrumEx.GetMz`.
Its breadcrumb therefore shows the declaring type, and its Remarks carry one line
naming the types that inherit it ("Inherited by Spectrum and SpectrumEx.").

## Release news

Each official release gets a news post and a download entry.

Source material comes from the commits between the previous official tag and the new
one, plus the GitHub release body when there is one. Read the commits, then write
for someone who uses the library and does not read commit logs.

Style rules:

- Plain, straightforward sentences. Short is better.
- No em dashes. This applies to all website content and to code comments.
  Quoted material from outside sources keeps its own punctuation, so citations
  that correctly use an en dash in a page range stay as they are.
- American English spelling.
- Say what changed and who it affects. A bug fix that only matters to users in
  certain regions should say so.
- Credit people by name when the commit credits them.
- No marketing language and no filler.

Every news item is approved before it goes on the site.

Date a post earlier than the time it will be built, with the Pacific offset in effect
on that date (`-0700` in summer, `-0800` in winter). A post dated in the future is
silently left out of the build until a later one. News5 uses 09:00 on release day.

## Download entries

Each official release gets one file in `_downloads`. The front matter carries four
links: `zip`, `tar`, `gh`, and `nupkg`, plus a `version` field holding the bare
version number, for example `1.0.0.18`.

The `version` field is required. The namespaces page uses the newest entry's version
to state which release the documentation describes, and leaving it out makes that
line vanish. The entry titles are not consistently formatted, which is why the
version is carried in its own field rather than parsed out of the title.

Check every link resolves before publishing. Check that the `nupkg` link points at
the correct release tag directory, because that is the one most easily copied from a
previous entry and left wrong.

The release history is append-only. Old official releases are never removed.

## Frozen decisions

These are settled. Changing any of them starts with a conversation, not a commit.

**Chirpy theme version.** Pinned to 7.5.0 in the `Gemfile`, which is the version that
built the deployed site. `Gemfile.lock` is committed so the version cannot drift. The
lockfile includes both the Windows and Linux platforms because builds happen on
Windows locally and on Ubuntu in CI.

**Local forks of theme files.** Four files in this repository override a file of the
same name in the Chirpy gem:

| File | Why it is forked |
|---|---|
| `_layouts/post.html` | Adds the "See also" block that resolves the `siblings` front matter across the type collections. Also drops the `post-nav` include, whose older and newer buttons navigate by collection order and mean nothing on API documentation. |
| `_layouts/home.html` | Renders the front page as introduction only, with no post list. |
| `_includes/topbar.html` | Builds API breadcrumbs from the `namespaces` front matter, because the URL cannot express the hierarchy. |
| `_includes/sidebar.html` | Points the GitHub icon at the repository rather than the account, and drops the theme's email and RSS handling. |

A fork stops receiving theme fixes the moment it is created, and it can keep calling
something the theme has since renamed. All four are pinned against Chirpy 7.5.0. If
the theme version ever changes, re-check every one of them against the new release
before anything is published. Step 6 of the review checklist exists for this.

Separately, `_layouts/category.html` is not a fork but it does shadow a theme layout
name. Its contents are this site's namespace listing page. Chirpy's own category
layout is therefore unreachable, which does not matter while category archives are
switched off, but would matter again if they were ever turned back on.

**Public URL shape.** The README on `main` links directly to `/classes/PipesServer.html`,
`/classes/PipesClient.html`, and `/download/`. Those URLs are a public contract. Do not
restructure collections or change permalinks without planning for them. If a change
is ever needed, prepare the exact README edit and hand it over, because website work
does not touch `main`.

**Copyright year.** Rolls forward each year.

## Site character

This is a functional reference for a software library. It is not a blog.

Chirpy brings blog machinery with it: tags, archives, categories, comment systems,
page view counters, and sharing widgets. News posts are genuinely post shaped and
belong here. The rest should be kept only where it earns its place. Suggestions that
make the site leaner and clearer are welcome. Suggestions that make it more blog-like
are not.

## Approval model

Two tiers.

**Fix on sight, report afterward.** Unambiguous bugs and broken links. Things that
are simply wrong, where the correct version is not a matter of taste.

**Propose first, wait for approval.** Everything else. Structural changes, layout
changes, anything touching the look of the site, removals of files or configuration,
all news and documentation text, and anything that is a judgment call.

When in doubt, propose rather than act.

**An answer is not authorization.** When the owner picks an option from a proposal,
that settles what the change should be. It does not say to start making it. Ask
whether to begin, and wait for a yes, before editing any file. The two are separate
steps and they are often separated in time.

## Verification and publishing

**Build locally before committing.** A clean production build is required:

```
bundle exec jekyll build
```

Compare the rendered output before and after a change, not just the source.

**Link checking is manual, by deliberate choice.** Nothing checks this site
automatically. A push that has not been through a review pass has nothing standing
between a broken link and the published site. That is a known and accepted tradeoff.

Each review pass checks links in three ways, as step 3 of the checklist below:

- Internal links, by walking the built site and confirming every target exists.
- Page anchors, by confirming every `href="#..."` matches an element id on that page.
- External links, with `curl`, against the live network.

Images, their alt text, and script references are worth checking the same way when
anything touches a layout.

Some background, so nobody re-opens this by accident. Chirpy ships a tool called
`html-proofer`, which does all of the above automatically. It is declared in the
`Gemfile` and `tools/test.sh` is built around it. It cannot run on this machine,
because it reaches the network through a `libcurl` library that Windows does not
provide, and installing one was declined. Adding it to the publishing workflow
instead was considered and also declined. Both entries stay where they are: the
script references the gem and both work on Linux, so neither is cruft by the
definition used here.

**Local review before any push.** Changes are reviewed on the local server before
they go out:

```
bundle exec jekyll serve -l
```

This serves at `http://127.0.0.1:4000`. Chirpy also provides `tools/run.sh`, which
wraps the same command. Review happens there, so problems get caught before they
reach the published site.

**Pushing.** The repository owner does all pushes, always. Website work stops at a
local commit and never pushes, under any circumstances.

Local review is an additional gate on top of that, not a substitute for it. Changes
are reviewed on the local server first, and then the owner decides whether and when
to push.

Note that a push to `gh-pages` deploys immediately. There is no staging environment,
so the local review above is the last checkpoint.

## Review pass checklist

A review pass runs in this order and reports in one place.

1. **State.** Branch, working tree, sync with origin, what changed on `main` since
   the last pass.
2. **Releases.** Newest official tag against what the downloads and news advertise.
3. **Links, from both ends.** Every outbound URL in content, front matter and
   layouts, checked over the network. Then every internal link in the *built* site,
   checked against what the build actually produced. These find different things.
   Source files alone will not surface a broken link that a layout generates.
4. **Documentation drift.** Public API at the latest official tag against the
   documented types.
5. **Bugs.** Liquid, YAML, front matter, layout logic, configuration.
6. **Theme forks.** Compare every locally overridden theme file against the same
   file in the installed theme. Overrides drift silently, and a stale fork can
   carry a call to something the theme has since renamed.
7. **Site chrome.** Open one built page of each type and read its navigation:
   breadcrumbs, sidebar, topbar. Content checks do not look at the furniture around
   the content, and that is where a whole class of defects hides.
8. **Cruft.** Files and configuration that nothing references and that cannot fire.
9. **Presentation.** Graphics, color, and clarity worth a human look.

Findings are reported in three groups: fixed, proposed and awaiting approval, and
noted with no action suggested. Each finding cites a file and line, shows the current
content and the proposed content, and explains in one sentence why it matters.

## Local environment notes

Two Ruby versions are installed on the development machine. Ruby 4.0 cannot build
this site, because Chirpy requires Ruby `~> 3.1`, which excludes 4.x. Use the 3.3
installation at `C:\Ruby33-x64`.

CI uses Ruby 3.3.6. Local uses 3.3.12. The same minor series, no known issues.
