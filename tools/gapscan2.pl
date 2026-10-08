#!/usr/bin/perl
# Reports which public members of a C# file lack XML doc comments.
# Tracks type scope by brace depth, so interface members (no modifier) and
# enum values are scanned. Block comments are blanked first, so a commented-out
# class is not counted. <inheritdoc/> counts as complete.
# Output, tab separated: type-access, type, member-kind, member, missing
use strict;
use warnings;

my $file = shift or die "usage: gapscan2.pl <file>\n";
open(my $fh, '<', $file) or die "cannot open $file\n";
my $text = do { local $/; <$fh> };
close($fh);

# blank /* */ comments, keeping newlines so line numbers hold
$text =~ s{/\*.*?\*/}{ my $c = $&; $c =~ s/[^\n]//g; $c }gse;
my @lines = split /\n/, $text, -1;

my @stack;       # [kind, name, access, body-depth]
my $depth = 0;
my $pending;     # type declared, waiting for its opening brace

for my $i (0 .. $#lines) {
  my $code = $lines[$i];
  $code =~ s{//.*$}{};                  # drop line comments, including ///
  $code =~ s/"(?:[^"\\]|\\.)*"/""/g;    # neutralize string contents
  $code =~ s/'(?:[^'\\]|\\.)'/''/g;

  my $top = @stack ? $stack[-1] : undef;
  my $at_body = $top && $depth == $top->[3];

  if ($code =~ /^\s*(public|internal|private|protected)?\s*(?:(?:sealed|abstract|static|partial|readonly)\s+)*(class|struct|interface|enum)\s+(\w+)/) {
    $pending = [$2, $3, $1 // 'internal'];
  }
  elsif ($at_body && $code =~ /\S/) {
    member($i, $code, @$top);
  }

  for my $ch (split //, $code) {
    if ($ch eq '{') {
      $depth++;
      if ($pending) { push @stack, [@$pending, $depth]; $pending = undef; }
    }
    elsif ($ch eq '}') {
      pop @stack if @stack && $depth == $stack[-1][3];
      $depth--;
    }
  }
}

sub member {
  my ($i, $code, $kind, $tname, $tacc) = @_;
  $code =~ s/^\s+|\s+$//g;
  return if $code eq '' || $code =~ /^[\[#]/;

  my ($name, $mkind, $nparams, $needs_ret) = ('', '', 0, 0);

  if ($kind eq 'enum') {
    return unless $code =~ /^(\w+)\s*(?:=|,|$)/;
    ($name, $mkind) = ($1, 'value');
  }
  else {
    if ($kind ne 'interface') {
      return unless $code =~ /^public\s/;
      return if $code =~ /\b(class|struct|interface|enum|delegate)\b/;
    }
    my $sig = $code;
    $sig =~ s/=>.*$//;
    $sig =~ s/\{.*$//;
    $sig =~ s/;\s*$//;

    if ($sig =~ /\bevent\b/) {
      ($name) = $sig =~ /(\w+)\s*$/;
      $mkind = 'event';
    }
    elsif ($sig =~ /(\w+)\s*(?:<[^>()]*>)?\s*\(([^)]*)/) {
      $name = $1;
      my $plist = $2;
      $plist =~ s/^\s+|\s+$//g;
      $nparams = $plist eq '' ? 0 : scalar(split /,/, $plist);
      my $is_ctor = ($name eq $tname);
      my $is_void = ($sig =~ /\bvoid\s+\Q$name\E\b/);
      $mkind = $is_ctor ? 'ctor' : 'method';
      $needs_ret = (!$is_ctor && !$is_void) ? 1 : 0;
    }
    else {
      ($name) = $sig =~ /(\w+)\s*$/;
      $mkind = ($code =~ /\{/) ? 'property' : 'field';
    }
  }
  return unless $name;

  my ($sum, $par, $ret, $inh) = (0, 0, 0, 0);
  for (my $j = $i - 1; $j >= 0; $j--) {
    my $p = $lines[$j];
    if ($p =~ m{^\s*///}) {
      $sum++ if $p =~ /<summary>/;
      $par++ if $p =~ /<param\s+name=/;
      $ret++ if $p =~ /<returns>/;
      $inh++ if $p =~ /<inheritdoc/;
    }
    elsif ($p =~ /^\s*\[/) { next; }
    else { last; }
  }

  my @missing;
  unless ($inh) {
    push @missing, 'summary' unless $sum;
    push @missing, 'param x' . ($nparams - $par) if $nparams > $par;
    push @missing, 'returns' if $needs_ret && !$ret;
  }
  printf("%s\t%s\t%s\t%s\t%s\n", $tacc, $tname, $mkind, $name, @missing ? join(', ', @missing) : 'ok');
}
