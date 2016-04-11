using System;
using System.Text;

using SocketHttpListener.Net;

namespace Grapevine
{
   /// <summary>
   /// Extension methods to assist in parsing URL strings.
   /// </summary>
   public static class HttpListenerRequestExtensions
   {
      /// <summary>
      /// gets an expected query parameter, and throws if it is not found.
      /// No conversion is performed, value is returnerd as a string.
      /// </summary>
      public static string GetQueryString( this HttpListenerRequest request, string name )
      {
         string value = request.QueryString[name];
         if (value == null)
            throw new RESTArgumentException( "Query parameter not found", name );

         return value;
      }

      /// <summary>
      /// Retrieves a required query parameter like GetQueryString, and attempts
      /// to convert it to any typed value.
      /// </summary>
      public static T GetQueryParameter<T>( this HttpListenerRequest request, string name )
      {
         string value = GetQueryString( request, name );
         return StringConverter.FromString<T>( value, name ) ;
      }

      /// <summary>
      /// Same as GetQueryParameter, but returns a default if the query parameter was not supplied.
      /// This can still throw an exception if the query parameter can not be converted
      /// to the required type.
      /// </summary>
      public static T GetQueryParameter<T>( 
         this HttpListenerRequest request, 
         string name,
         T defaultValue )
      {
         string value = request.QueryString[name];
         if (value == null)
            return defaultValue;

         return StringConverter.FromString<T>( value, name ) ;
      }
   }
}