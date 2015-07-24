using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Newtonsoft.Json.Linq;

namespace WebRequest_JSON
{
    public static class JSONExtensions
    {
        public static string GetJsonType(this string jsonString)
        {
            var jo = JObject.Parse(jsonString);
            return jo["__type"].ToString();
        }

        public static bool IsType( this JObject jobject, string typeStr )
        {
            return (String.Compare(jobject["__type"].ToString(), typeStr, true) == 0);
        }
    }

    class Program
    {
        static string jsonError = "{"
        + "__type: \"ALData_Error\","
        + "errorCode: 0,"
        + "errorMessage: \"Invalid User ID. Supplied Novell user is not valid in PFirst - cponzi\""
        + "}";

        static string jsonUser = 
        @"{"
        + @"__type: ""ALData_User"","
        + @"canApprove: false,"
        + @"canChangeAfterApproval: false,"
        + @"canOverridePaidOut: false,"
        + @"canSelectFornightRest: false,"
        + @"canSelectWeekRest: false,"
        + @"canStatusChange: false,"
        + @"dlaAmount: 1,"
        + @"dlaExpiry: ""/Date(1427760000000)/"","
        + @"negIntMargin: 0,"
        + @"usercode: ""JSM"""
        + @"}";

        static string urlBase = @"http://sbsdtestpet.sbs.local/jaderestservices/jadehttp.dll";
        static string userRequest = @"user";
        static string queryString = @"SBSALRestApp";

        static void DumpType(string jsonString)
        {
            string jsonType = jsonString.GetJsonType();
            Console.WriteLine("Raw JSON: {0}", jsonString);
            Console.WriteLine("jsonType: {0}\r\n", jsonType);
            JObject jobj = JObject.Parse(jsonString);
            if (jobj.IsType("ALData_User"))
            {
                Console.WriteLine("Using .ToString()");
                Console.WriteLine("canApprove: {0}", jobj["canApprove"].ToString());
                Console.WriteLine("canChangeAfterApproval: {0}", jobj["canChangeAfterApproval"].ToString());
                Console.WriteLine("canOverridePaidOut: {0}", jobj["canOverridePaidOut"].ToString());
                Console.WriteLine("canSelectFornightRest: {0}", jobj["canSelectFornightRest"].ToString());
                Console.WriteLine("canSelectWeekRest: {0}", jobj["canSelectWeekRest"].ToString());
                Console.WriteLine("canStatusChange: {0}", jobj["canStatusChange"].ToString());
                Console.WriteLine("dlaAmount: {0}", jobj["dlaAmount"].ToString());
                Console.WriteLine("dlaExpiry: {0}", jobj["dlaExpiry"].ToString());
                Console.WriteLine("negIntMargin: {0}", jobj["negIntMargin"].ToString());
                Console.WriteLine("usercode: {0}", jobj["usercode"].ToString());


                Console.WriteLine("Using casting");
                Console.WriteLine("canApprove: {0}", (bool)jobj["canApprove"]);
                Console.WriteLine("canChangeAfterApproval: {0}", (bool)jobj["canChangeAfterApproval"]);
                Console.WriteLine("canOverridePaidOut: {0}", (bool)jobj["canOverridePaidOut"]);
                Console.WriteLine("canSelectFornightRest: {0}", (bool)jobj["canSelectFornightRest"]);
                Console.WriteLine("canSelectWeekRest: {0}", (bool)jobj["canSelectWeekRest"]);
                Console.WriteLine("canStatusChange: {0}", (bool)jobj["canStatusChange"]);
                Console.WriteLine("dlaAmount: {0}", (decimal)jobj["dlaAmount"]);
                Console.WriteLine("dlaExpiry: {0}", (DateTime)jobj["dlaExpiry"]);
                Console.WriteLine("negIntMargin: {0}", (decimal)jobj["negIntMargin"]);
                Console.WriteLine("usercode: {0}", jobj["usercode"].ToString());
            }
        }

        static string GenUserRequest(string userName)
        {
            return String.Format("{0}/{1}/{2}?SBSALRestApp", urlBase, userRequest, userName);
        }

        // HTTP Status Codes (Windows) <https://msdn.microsoft.com/en-us/library/aa383887.aspx>
        static bool InvokeGetRequest(string webRequest, out HttpStatusCode statusCode, out string responseFromServer)
        {
            WebRequest request = WebRequest.Create(webRequest);
            request.Credentials = CredentialCache.DefaultCredentials; // supports Windows auth

            request.Method = "GET";
            request.ContentLength = 0;
            request.Timeout = 10000;
            try
            {
                WebResponse response = request.GetResponse();

                HttpStatusCode returnStatus = ((HttpWebResponse)response).StatusCode;

                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();

                // Clean up the streams.
                reader.Close();
                dataStream.Close();

                response.Close();

                statusCode = returnStatus;
                return true;
            }
            catch( WebException e )
            {
                if (e.Response != null)
                {
                    using (WebResponse response = e.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        HttpStatusCode returnStatus = ((HttpWebResponse)response).StatusCode;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                        statusCode = returnStatus;
                        return true;
                    }
                }
                else
                {
                    responseFromServer = "";
                    statusCode = (HttpStatusCode)(404); // not the real status code, filling it in
                    return false;
                }
            }
        }

        static void Main(string[] args)
        {
            DumpType(jsonError);
            DumpType(jsonUser);

            HttpStatusCode statusCode;
            string requestUser;
            string jsonResponse;

            requestUser = GenUserRequest("jsmith");
            Console.WriteLine("Request: {0}", requestUser);
            InvokeGetRequest(requestUser, out statusCode, out jsonResponse);
            DumpType(jsonResponse);
            Console.WriteLine("Status code: {0}", statusCode);
            if (statusCode != HttpStatusCode.OK)
                Console.WriteLine("Request: Did not complete");
            else
            {
                Console.WriteLine("Request completed");
                JObject jobj = JObject.Parse(jsonResponse);
                Console.WriteLine("Casting");


            }

            Console.WriteLine("------------");

            requestUser = GenUserRequest("cponzi");
            Console.WriteLine("Request: {0}", requestUser);
            if (InvokeGetRequest(requestUser, out statusCode, out jsonResponse))
            {
                if ( statusCode == HttpStatusCode.OK)
                    DumpType(jsonResponse);
                Console.WriteLine("Status code: {0}", statusCode);
                Console.WriteLine("Request: Completed");
            }
            else
            {
                Console.WriteLine("Request did not complete");
            }

            Console.WriteLine("------------");

            requestUser = "http://sbsdtestpet.sbs.local/jaderestservices/jadehttp.dll/user/cponzi?SBSALRestApp1"; // bad syntax
            Console.WriteLine("Request: {0}", requestUser);
            if (InvokeGetRequest(requestUser, out statusCode, out jsonResponse))
            {
                if (statusCode == HttpStatusCode.OK)
                    DumpType(jsonResponse);
                Console.WriteLine("Status code: {0}", statusCode);
                Console.WriteLine("Request: Completed");
            }
            else
            {
                Console.WriteLine("Request did not complete");
            }

            Console.WriteLine("------------");

            requestUser = "http://deadend.sbs.local/jaderestservices/jadehttp.dll/user/cponzi?SBSALRestApp"; // bad host
            Console.WriteLine("Request: {0}", requestUser);
            if (InvokeGetRequest(requestUser, out statusCode, out jsonResponse))
            {
                if (statusCode == HttpStatusCode.OK)
                    DumpType(jsonResponse);
                Console.WriteLine("Status code: {0}", statusCode);
                Console.WriteLine("Request: Completed");
            }
            else
            {
                Console.WriteLine("Request did not complete");
            }
        }
    }
}
