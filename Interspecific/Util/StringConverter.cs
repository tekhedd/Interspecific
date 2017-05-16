using System;
using System.ComponentModel;

namespace Interspecific
{
   public static class StringConverter
   {
      /// <summary>
      /// Converts a string to any type, throwing ArgumentException on failure using the 
      /// supplied name as 
      /// </summary>
      /// <param name="value">thing to convert</param>
      /// <param name="name">Name of variable for error reporting</param>
      ///
      public static T FromString<T>( string value, string name )
      {
         var converter = TypeDescriptor.GetConverter(typeof(T));

         if (!converter.CanConvertFrom(typeof(string)))
         {
            string msg = String.Format( 
               "Cannot convert value ({0}) to type {1}", 
               value, typeof(T) );

            throw new RESTArgumentException( msg, name );
         }

         return (T) converter.ConvertFrom( value );
      }
   }
}

