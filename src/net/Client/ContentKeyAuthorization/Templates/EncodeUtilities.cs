﻿//-----------------------------------------------------------------------
// <copyright file="EncodeUtilities.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
   

   
        public class EncodeUtilities
        {
            public static string Base64UrlEncode(byte[] arg)
            {
                string s = Convert.ToBase64String(arg); // Regular base64 encoder
                s = UrlEncode(s);               
                return s;
            }

            private static string UrlEncode(string s)
            {
                s = s.Split('=')[0]; // Remove any trailing '='s
                s = s.Replace('+', '-'); // 62nd char of encoding
                s = s.Replace('/', '_'); // 63rd char of encoding
                return s;
            }

            public static byte[] Base64UrlDecode(string arg)
            {
                var s = UrlDecode(arg);
                return Convert.FromBase64String(s); // Standard base64 decoder
            }

            private static string UrlDecode(string arg)
            {
                string s = arg;
                s = s.Replace('-', '+'); // 62nd char of encoding
                s = s.Replace('_', '/'); // 63rd char of encoding
                switch (s.Length % 4) // Pad with trailing '='s
                {
                    case 0:
                        break; // No pad chars in this case
                    case 2:
                        s += "==";
                        break; // Two pad chars
                    case 3:
                        s += "=";
                        break; // One pad char
                    default:
                        throw new Exception(
                            "Illegal base64url string!");
                }
                return s;
            }
        }
    
}