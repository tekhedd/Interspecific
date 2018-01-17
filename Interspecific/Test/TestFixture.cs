// System libs
using System;
using System.IO;
using SysNet = System.Net;
using System.Text.RegularExpressions;

// third party
using NUnit.Framework;
using SocketHttpListener.Net;

// Us.
using Interspecific;
using Interspecific.Server;


[TestFixture]
public class TestFixture
{
   readonly string root = "http://localhost:8123";
   
   // Assumes project output is bin/$(Configuration). CWD is 
   // always the same dir as the DLL with NUnit.
   // 
   readonly Config config = new Config {
      Protocol = "http",
      Host = "localhost",
      Port = "8123",
      // Path to our web test dir relative to Test/bin/Debug. Will get 404 errors if this is wrong
      WebRoot = TestContext.CurrentContext.TestDirectory + "/../../Data/web",
      DirIndex = "myindex.html",
      MaxThreads = 3,
      AutoLoadRestResources = false,
      ServerHeader = "MyTestServer/2.3"
   };

   
   RESTServer _host;
   
   [OneTimeSetUp]
   public void SetUp() 
   {
      _host = new RESTServer( config );
      _host.AddResource( new TestResource() );
      _host.Start();
   }

   [OneTimeTearDown]
   public void TearDown()
   {
      _host.Stop();
   }
   
   [Test]
   public void DefaultIndexTest()
   {
      string body = _HttpGet( root ); // get index
      Assert.AreEqual( "dummy", body );
   }

   [Test]
   public void StaticSubdirTest()
   {
      string body = _HttpGet( root + "/subdir/file1.txt" );
      Assert.AreEqual( "file1 content", body );
   }
   
   [Test]
   public void StaticWithSpacesTest()
   {
      string body = _HttpGet( root + "/file with spaces.txt" );
      Assert.AreEqual( "file contents", body );
   }

   [Test]
   public void SimpleRouteTest()
   {
      string body = _HttpGet( root + "/test/hello" );
      Assert.AreEqual( "hello, world", body );
   }

   [Test]
   public void SpaceRouteTest()
   {
      string body = _HttpGet( root + "/test/route with spaces" );
      Assert.AreEqual( "hello with spaces", body );
   }

   [Test]
   public void QueryPathPatternTest()
   {
      string body = _HttpGet( root + "/test/query/one/two" );
      Assert.AreEqual( "one,two", body );
   }
   
   // Test Match extensions
   // Test Request QueryString extensions
   
   [Test]
   public void HttpListenerExtensionsTest()
   {
      // The server will put the exception in the response if there is one
      string body = _HttpGet( root 
         + "/hlr_extensions?string=query_string&int=200" );
      
      Assert.AreEqual( "success", body );
   }
   
   [Test]
   public void MatchExtensionsTest()
   {
      // The server will put the exception in the response if there is one
      string body = _HttpGet( root 
         + "/match_extensions/MyString/100" );

      Assert.AreEqual( "success", body );
   }
   
   [Test]
   public void MatchConversionFailTest()
   {
      string body = _HttpGet( root + "/match_extensions/MyString/not_an_int" );
      Assert.IsTrue( body.StartsWith("fail") );
   }
   
   [Test]
   public void ServerHeaderTest()
   {
      string header = _GetServerHeader(root);
      Assert.AreEqual("MyTestServer/2.3", header);
      
      _host.ServerHeader = null;
      header = _GetServerHeader(root);
      Assert.AreEqual("Mono-HTTPAPI/1.0", header);
      
      _host.ServerHeader = "Ponies/1.0";
      header = _GetServerHeader(root);
      Assert.AreEqual("Ponies/1.0", header);
   }
   
   private string _GetServerHeader(string url)
   {
      SysNet.HttpWebRequest request = (SysNet.HttpWebRequest)SysNet.WebRequest.Create( url );
      using (SysNet.HttpWebResponse response = (SysNet.HttpWebResponse)request.GetResponse())
      {
         return response.Headers["Server"];
      }
   }
   
   // gets a url and assumes it's text
   string _HttpGet( string url )
   {
       SysNet.HttpWebRequest request = (SysNet.HttpWebRequest)SysNet.WebRequest.Create( url );
       using (SysNet.HttpWebResponse response = (SysNet.HttpWebResponse)request.GetResponse())
       {
          using (StreamReader reader = new StreamReader( response.GetResponseStream() ))
          {
              return reader.ReadToEnd();
          }
       }
   }

   // We are not auto loading it, so it does not have to be sealed. 
   // In fact, since we are setting autoload to false, we want attempts
   // to autoload this to show up as a test failure.
   public class TestResource : RESTResource
   {
      [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/test/hello$")]
      public void HelloWorld( HttpListenerContext ctx )
      {
         SendTextResponse( ctx, "hello, world" );
      }

      [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/test/route with spaces$")]
      public void Space( HttpListenerContext ctx )
      {
         SendTextResponse( ctx, "hello with spaces" );
      }
      
      [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/test/query/(?<param1>.+)/(?<param2>.+)$")]
      public void Space( HttpListenerContext ctx, Match match )
      {
         string param1 = match.Groups["param1"].Value;
         string param2 = match.Groups["param2"].Value;
         
         SendTextResponse( ctx, param1 + "," + param2 );
      }
      
      [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/hlr_extensions$")]
      public void HttpListenerRequestExtensions( HttpListenerContext ctx, Match match )
      {
         // Expect two url parameters, "string" and "int", as "MyString" and "100"
         try
         {
            string strVal = ctx.Request.GetQueryString("string");
            Assert.AreEqual("query_string", strVal);
            
            int intVal = ctx.Request.GetQueryParameter<int>("int");
            Assert.AreEqual(200, intVal);
            
            string defaultStr = ctx.Request.GetQueryParameter<string>("does_not_exist", "default_val");
            Assert.AreEqual("default_val", defaultStr, "found a nonexistent parameter?");
            
            SendTextResponse( ctx, "success" );
         }
         catch (Exception ex)
         {
            // On fail, send a response that starts with "fail"
            SendTextResponse( ctx, "fail: " + ex.ToString() );
         }
      }
      
      [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/match_extensions/(?<string>.+)/(?<int>.+)$")]
      public void MatchExtensions( HttpListenerContext ctx, Match match )
      {
         // Expect two query parameters, "string=query_string" and "int=200"
         try
         {
            string strVal = match.GetValue("string");
            Assert.AreEqual("MyString", strVal);

            int intVal = match.GetValue<int>("int");
            Assert.AreEqual(100, intVal);

            SendTextResponse( ctx, "success" );
         }
         catch (Exception ex)
         {
            SendTextResponse( ctx, "fail: " + ex.ToString() );
         }
      }
   }
}