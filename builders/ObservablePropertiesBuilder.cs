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
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

namespace randori.compiler.builders
{

    // The purpose of this class is to create getter & setter's for properties tagged bindable 
    // that don't already have getter & setter's defined.
    class ObservablePropertiesBuilder
    {
        
        protected DefaultResolvedTypeDefinition cSharpEntity;

        public ObservablePropertiesBuilder(DefaultResolvedTypeDefinition arg1)
        {
            this.cSharpEntity = arg1;
        }
 
        public List<JsExpressionStatement> getEncapsulateBindableProperties()
        {
            List<JsExpressionStatement> results = null;

            // 

            // the NrRefactory object will have properties that already contain a getter & setter in the "properties" of the IEntity
            // however, a c# property that does not define a getter/setter will be considered a field in the "Fields" of the IEntity

            // we will need to generate a getter & setter for every field that does not contain a getter & setter in the properties.
            // get all bindable fields
            /* 
             * TODO: [Observable] Support
             * 
            IList<IField> fields = IEntityUtils.getFieldsByAttribute(cSharpEntity, RandoriClassNames.metadataObservable);

            if (fields != null)
            {
                results = new List<JsExpressionStatement>();
                // throw an error on any property that is private and bindable
                foreach (IField field in fields)
                {
                    if (!field.IsPublic)
                    {
                        throw new SystemException("Private field ( " + field.ReflectionName + " ) marked [Observable] is not supported.");
                    }
                    else
                    {
                        // prototype stuff...
                        //results.Add(AstUtils.getGetterSetterExpression(field.Name, field.Name));
                        //DefaultUnresolvedField xx = new DefaultUnresolvedField(field.UnresolvedMember.GetType(), field.Name);

                        string originalName = field.Name;
                        string adjustedName = "_" + originalName;
                        
                        // rename
                        //field.Name = adjustedName;
                    }
                }
            }
            */
            //TODO: checek to see if there is already getter & setters generated.
            if (results != null && results.Count > 0)
            {
                return results;
            }

            return null;
        }


    }
}
