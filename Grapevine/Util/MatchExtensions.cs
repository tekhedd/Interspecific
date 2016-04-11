using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Grapevine
{
   /// <summary>
   /// Extension methods to retrieve values from the URL match patterns.
   /// </summary>
   public static class MatchExtensions
   {
      /// <summary>
      /// Returns a value from the matches or null if not found. Unescapes the URL
      /// data. Throws RESTArgumentException if the parameter is not found or invalid.
      /// </summary>
      public static string GetValue( this Match match, string name )
      {
         System.Text.RegularExpressions.Group group = match.Groups[name];
         if (!group.Success)
            throw new RESTArgumentException( "URL parameter not found", name );

         // Currently, the url is raw, so we manually decode the partz.
         return Uri.UnescapeDataString( group.Value );
      }

      /// <summary>
      /// Like GetValue(), but also casts the data to whatever type you like.
      /// Throws RESTArgumentException if the parameter is not found or invalid.
      /// </summary>
      public static T GetValue<T>( this Match match, string name )
      {
         string value = GetValue( match, name );
         return StringConverter.FromString<T>( value, name );
      }
   }
}