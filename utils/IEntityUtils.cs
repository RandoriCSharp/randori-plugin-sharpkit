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
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.Semantics;

namespace randori.compiler.utils
{
    class IEntityUtils
    {

        public static IList<IMethod> getClassConstructors(DefaultResolvedTypeDefinition classDef)
        {
            IList<IMethod> results = new List<IMethod>();

            foreach (IMethod methodDef in classDef.Methods)
            {
                //string mName = methodDef.Name;
                if (methodDef.IsConstructor)
                {
                    // Check the class is being exported
                    //ITypeDefinition classDef = getClassDefinition( methodDef.
                    results.Add(methodDef);
                }
            }
            if (results.Count > 0)
            {
                return results;
            }
            return null;
        }

        public static IList<IField> getFieldsByAttribute(DefaultResolvedTypeDefinition classDef, string reflectionName)
        {
            IList<IField> results = new List<IField>();

            foreach (IField field in classDef.Fields)
            {
                IAttribute attribute = getAttributeByName(field.Attributes, reflectionName);

                if (attribute != null)
                {
                    results.Add(field);
                }
            }

            if (results.Count > 0)
            {
                return results;
            }

            return null;
        }

        public static IList<IProperty> getPropertiesByAttribute(DefaultResolvedTypeDefinition classDef, string reflectionName)
        {
            IList<IProperty> results = new List<IProperty>();

            foreach (IProperty property in classDef.Properties)
            {
                IAttribute attribute = getAttributeByName(property.Attributes, reflectionName);

                if (attribute != null)
                {
                    results.Add(property);
                }
            }

            if (results.Count > 0)
            {
                return results;
            }

            return null;
        }

        public static IAttribute getAttributeByName(IList<IAttribute> attributes, string reflectionName)
        {
            IAttribute result = null;

            foreach (IAttribute attr in attributes)
            {
                // there are several ways to do this, but since we don't reference the randori project, we'll stick the string names in for now
                if (attr.AttributeType.ReflectionName == reflectionName)
                {
                    result = attr;
                    break;
                }
            }

            return result;
        }

        public static IField getFieldByName(IEnumerable<IField> fields, string fieldName)
        {
            IField result = null;
            foreach (IField field in fields)
            {
                if (field.Name == fieldName)
                {
                    result = field;
                    break;
                }
            }

            return result;
        }

        public static IList<IMethod> getClassMethodsByAttribute(DefaultResolvedTypeDefinition classDef, string reflectionName)
        {
            IList<IMethod> results = new List<IMethod>();

            foreach (IMethod methodDef in classDef.Methods)
            {
                string mName = methodDef.Name;
                if (!methodDef.IsConstructor)
                {
                    IAttribute attribute = getAttributeByName(methodDef.Attributes, reflectionName);
                    if (attribute != null)
                    {
                        results.Add(methodDef);
                    }
                }
            }
            if (results.Count > 0)
            {
                return results;
            }
            return null;
        }

        public static bool isFieldRequired(IField field)
        {
            bool result = false;

            IAttribute att = IEntityUtils.getAttributeByName(field.Attributes, "randori.attributes.View");
            if (att != null)
            {
                bool found = false;

                // check to see if a user has explicitly set the required flag for the attribute. 
                foreach (KeyValuePair<IMember, ResolveResult> namedPair in att.NamedArguments)
                {
                    IMember namedKey = (IMember) namedPair.Key;
                    // need to find a better way to do this in case property gets renamed
                    if (namedKey.Name == "required")
                    {
                        result = (bool) namedPair.Value.ConstantValue;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // if a user has not explicitly set the required flag, check the default value for the attribute
                    // get the default value from the constructor.
                    IList<IParameter> constructorParams = att.Constructor.Parameters;
                    foreach ( IParameter param in constructorParams )
                    {
                        if (param.Name == "required")
                        {
                            result = (bool) param.ConstantValue;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public static string getInjectAnnotation(IField field)
        {
            string result = null;

            IAttribute att = IEntityUtils.getAttributeByName(field.Attributes, "randori.attributes.Inject");
            if (att != null)
            {
                foreach (KeyValuePair<IMember, ResolveResult> pair in att.NamedArguments)
                {
                    IMember key = (IMember)pair.Key;
                    // need to find a better way to do this in case property gets renamed
                    if (key.Name == "annotation")
                    {
                        result = (string) pair.Value.ConstantValue;
                    }
                }
            }

            return result;
        }

    }
}
