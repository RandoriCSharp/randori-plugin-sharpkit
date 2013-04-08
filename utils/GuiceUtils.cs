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

using System.Collections.Generic;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Semantics;

namespace randori.compiler.utils
{
    class GuiceUtils
    {

        public static int notRequiredFlag = 0;

        public static int requiredFlag = 1;

        public static bool shouldExcludeBasedOnNamespace(string ns)
        {
            // exculude C# SharpKit and System packages
            if (ns != null && (ns.StartsWith("SharpKit") || ns.StartsWith("System")))
            {
                return true;
            }

            return false;
        }

        public static string getInjectionPointString(IParameter parm)
        {
            bool isRequired = false;
            string defaultValue = null;

            if (parm.IsOptional == false)
            {
                isRequired = true;
            }
            else if (parm.ConstantValue != null)
            {
                // if a default value is defined, set it.
                defaultValue = parm.ConstantValue.ToString();
            }

            string exportClassName = parm.Type.FullName;
            string exportNamespace = parm.Type.Namespace;
            
            // We need to find out if this class this parameter extends is actually being exported, if not, we need to change the string to reflect that
            if (parm.Type is ITypeDefinition)
            {
                ITypeDefinition classType = (ITypeDefinition) parm.Type;
                bool exportClass = true;


                if (classType.Attributes.Count > 0)
                {
                    foreach (IAttribute attr in classType.Attributes)
                    {
                        // dumb
                        if (attr.AttributeType.FullName == "SharpKit.JavaScript.JsTypeAttribute")
                        {
                            foreach (KeyValuePair<IMember, ResolveResult> namedPair in attr.NamedArguments)
                            {
                                IMember namedKey = (IMember)namedPair.Key;

                                if (namedKey.Name == "Name")
                                {
                                    exportClassName = (string)namedPair.Value.ConstantValue;
                                }

                                if (namedKey.Name == "Export")
                                {
                                    exportClass = (bool)namedPair.Value.ConstantValue;
                                    exportNamespace = null;
                                }
                            }
                        }

                        if (!exportClass)
                        {
                            return GuiceUtils.getInjectonPointString(parm.Name, exportClassName, exportNamespace, defaultValue, isRequired);
                        }
                    }
                }
            }
            return GuiceUtils.getInjectonPointString(parm.Name, exportClassName, exportNamespace, defaultValue, isRequired);
        }

        // n: item name
        // t: item type
        // v item value
        // r: is required
        // a: attributes
        // p: parameters (used in methods)

        public static string getInjectonPointString(string itemName, string itemFullName, string itemNS, string itemValue=null, bool required = false, string annotation = null, string parameters = null)
        {
            // probably a better way to do this, but for now..
            string itemType = itemFullName;
            string results = "{n:\'" + itemName + "\'";

            string skHtmlPackage = "SharpKit.Html.";

            // We always want to excluded based on certain namespace packages.
            if (GuiceUtils.shouldExcludeBasedOnNamespace(itemNS))
            {
                itemType = null;
            }
            // 12/11/2012 - This should be Temporary. SharpKit has a few issues with the SharpKit.HTML package.
            // As a result we are checking the full name of the classes and if they are from the SharpKit.HTML package, stripping off the text
            // so it is not exported as such. 
            else if (itemType.Contains( skHtmlPackage ))
            {
                itemType = itemType.Replace(skHtmlPackage, "");
            }

            if( itemType != null )
            {
                results += ", t:\'" + itemType + "\'";
            }

            // required..
            if (!required)
            {
                results += ", r:" + notRequiredFlag;
            }

            // In the situation where the default value is not null
            // OR no matter what, if the item is not required, we have to
            // set a default value.
            if (itemValue != null)
            {
                results += ", v:\'" + itemValue + "\'";
            }
            else if (!required)
            {
                results += ", v:null";
            }

            if (annotation != null)
            {
                results += ", a:\'" + annotation + "\'";
            }

            if(parameters != null)
            {
                results += ", p:[" + parameters + "]";
            }

            results += "}";

            return results;
        }

    }
}
