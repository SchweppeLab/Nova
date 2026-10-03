# frozen_string_literal: true

source "https://rubygems.org"

# Pinned to the exact version that built the currently deployed site.
# Do not change without agreeing on a Chirpy upgrade first.
gem "jekyll-theme-chirpy", "7.5.0"

gem "jekyll-github-metadata"

gem "html-proofer", "~> 5.0", group: :test

platforms :mingw, :x64_mingw, :mswin, :jruby do
  gem "tzinfo", ">= 1", "< 3"
  gem "tzinfo-data"
end

gem "wdm", "~> 0.2.0", :platforms => [:mingw, :x64_mingw, :mswin]
