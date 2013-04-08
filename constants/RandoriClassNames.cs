/***
 * Copyright 2012 LTN Consulting, Inc. /dba Digital Primates®
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * @author Ben Schmidtke <bschmidtke@digitalprimates.net>
 */

using randori.attributes;
using randori.content;

namespace randori.compiler.constants
{

    // TODO: The commented code is if we add a reference for the compiler to the Randori project.
    // TODO: However, since the randori project uses the plugin they are intra-dependant on each other and compiling
    // TODO: fails. Either this means we need to bundle a version of the dll to this project (yuck)
    // TODO: or externalize the meta-data classes into a library project to be shared.
 
    public class RandoriClassNames
    {
        public static string contentCache
        {
            get
            {
                return typeof( ContentCache ).FullName;
            }
        }

        public static string metadataInject
        {
            get
            {
                return typeof( Inject ).FullName;
            }
        }

        public static string metadataView
        {
            get
            {
                return typeof( View ).FullName;
            }
        }

        //public static string metadataObservable
        //{
        //    get
        //    {
        //        return typeof( Observable ).FullName;
        //    }
        //}

        public static string metadataHtmlMergedFile
        {
            get
            {
                return typeof( HtmlMergedFile ).FullName;
            }
        }

    }
}
