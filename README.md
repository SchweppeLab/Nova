# Nova Website

This branch holds the source for the Nova documentation site, published at
<https://schweppelab.github.io/Nova/>.

Nova itself lives on the `main` branch of this repository. This branch has a
separate history and contains no library code.

## What is here

A [Jekyll](https://jekyllrb.com) site built on the
[Chirpy](https://github.com/cotes2020/jekyll-theme-chirpy) theme, pinned to an
exact version in the `Gemfile`.

| Path | Contents |
| --- | --- |
| `_namespaces`, `_classes`, `_structs`, `_interfaces`, `_enums` | API documentation, one file per type |
| `_posts` | Release announcements |
| `_downloads` | Download links, one file per official release |
| `_tabs` | The sidebar tabs |
| `_layouts`, `_includes` | Custom layouts, plus local overrides of four theme files |
| `docs/CONTENT_GUIDE.md` | The working rules for maintaining this site |

## Building locally

Requires Ruby 3.3 and Bundler. Ruby 4.x will not work, because the theme
requires Ruby `~> 3.1`.

```shell
bundle install
bundle exec jekyll serve -l
```

The site is then served at <http://127.0.0.1:4000>.

## Publishing

A push to `gh-pages` builds and deploys immediately. There is no staging step,
so review the site locally before pushing.

Read `docs/CONTENT_GUIDE.md` before making changes. It records the rules this
site is maintained by, including which releases may be linked, how release news
is written, and which decisions are frozen.
