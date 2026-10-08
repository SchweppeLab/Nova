// Snippets published on the Io.Meta pages.

using System;
using Nova.Io.Meta;

namespace NovaSnippets.IoMeta
{
  internal static class MetaDictionarySnippets
  {
    // MetaDictionary.FindMeta(string)
    internal static void LookUpATrailerLabel()
    {
      // Several spellings map to the same MetaClass, because the label
      // varies between instruments and file versions.
      MetaClass a = MetaDictionary.FindMeta("Charge State");
      MetaClass b = MetaDictionary.FindMeta("Z");

      // An unrecognized label returns None rather than throwing.
      MetaClass unknown = MetaDictionary.FindMeta("Not A Real Label");

      Console.WriteLine(a + " " + b + " " + unknown);
    }

    // Switching on the result
    internal static void ActOnALabel(string label, string value)
    {
      switch (MetaDictionary.FindMeta(label))
      {
        case MetaClass.ChargeState:
          Console.WriteLine("charge " + value);
          break;

        case MetaClass.FaimsCV:
          Console.WriteLine("FAIMS CV " + value);
          break;

        case MetaClass.None:
          // Nothing recognized this label, so leave it alone.
          break;
      }
    }
  }
}
