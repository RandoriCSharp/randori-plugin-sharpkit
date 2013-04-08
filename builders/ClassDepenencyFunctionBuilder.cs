﻿/***
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

using ICSharpCode.NRefactory.TypeSystem.Implementation;
using SharpKit.JavaScript.Ast;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using randori.compiler.constants;
using randori.compiler.utils;

namespace randori.compiler.builders
{
    class ClassDepenencyFunctionBuilder
    {
        
        protected DefaultResolvedTypeDefinition cSharpDef;
        protected JsUnit jsEntity;

        public ClassDepenencyFunctionBuilder(DefaultResolvedTypeDefinition arg1, JsUnit arg2)
        {
            this.cSharpDef = arg1;
            this.jsEntity = arg2;
        }

        public JsExpressionStatement getDependencyExpression()
        {
            if (cSharpDef != null)
            {
                JsAssignmentExpression newAssignment = new JsAssignmentExpression();
                newAssignment.Operator = "=";

                // since this is static, prepend the class path.
                JsMemberExpression leftPref = AstUtils.getNewMemberExpression(cSharpDef.FullName);

                //shouldExcludeBasedOnNamespace;
                newAssignment.Left = AstUtils.getNewMemberExpression(OutputNameConstants.FUNCTION_GET_CLASS_DEPENDENCIES, leftPref);
                newAssignment.Right = getClassDependencyFunction(jsEntity);

                JsExpressionStatement newStatement = new JsExpressionStatement();
                newStatement.Expression = newAssignment;
                return newStatement;
            }

            return null;
        }

        public JsFunction getClassDependencyFunction(JsUnit jsNode)
        {
            // regex for finding: 1) any typeof references 2) any uses of the "new" keyword
            // This does not work 100%, temporary solution until Dan-el Implements a visitor pattern in SharpKit
            string replaceIgnore = @"void\('#RANDORI_IGNORE_BEGIN([^;]*);[\W\w]*?void\('#RANDORI_IGNORE_END([^;]*);";
            //string regStr = @"(?<=Typeof\()(.*?)(?=\))|(?<=new ).*?(?=\()";
            string regStr = @"(?:Typeof\(|new\s)([\w\.]+)(?:\(|\))";

            // get the javascript
            string jsStr = jsEntity.ToJs();
            jsStr = Regex.Replace(jsStr, replaceIgnore, "");
            
            // function contents
            JsBlock resultsBlock = new JsBlock();
            resultsBlock.Statements = new List<JsStatement>();
            
            // Check to see if we need to call super.
            string superClassPath = null;
            
            // 12/04/1012 - Mike doesn't want to call super, commenting out to value remains null. Clean up later once this is confirmed after testing.
            // Leaving the code using superClassPath intact until we decide what route to go with this.

            //bool initArray = true;
            //foreach (IType type in cSharpDef.DirectBaseTypes)
            //{
            //    // careful, type.FullName could be all sorts of stuff, like interfaces
            //    if (type.Kind == TypeKind.Class)
            //    {
            //        initArray = GuiceUtils.shouldExcludeBasedOnNamespace(type.Namespace);
            //        if (!initArray)
            //        {
            //            superClassPath = type.FullName;
            //        }
            //    }
            //}

            JsBinaryExpression pExpression = null;
            JsInvocationExpression initializer = null;

            // If we have to call super...do it
            // in the case of properties methods and views, we will call super if a superClassPath is defined.
            if (superClassPath != null)
            {
                // define return variable and call super
                initializer = AstUtils.getStaticMethodCallInvocationExpression(OutputNameConstants.FUNCTION_GET_CLASS_DEPENDENCIES, superClassPath, null);

                // initalized the result array, by calling super
                pExpression = AstUtils.getJsBinaryExpression(
                                                                AstUtils.getNewMemberExpression(InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME),
                                                                "=",
                                                                initializer
                                                                );
            }
            else
            {
                // initalized empty result array
                pExpression = AstUtils.getJsBinaryExpression( AstUtils.getNewMemberExpression( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ),
                                                                                "=",
                                                                                AstUtils.getEmptyArrayInvocationExpression());
            }

            JsExpressionStatement pStatement = AstUtils.getJsExpressionStatement(pExpression);

            // define return variable.
            resultsBlock.Statements.Add( AstUtils.getJsVariableDeclarationStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ) );

            MatchCollection match = Regex.Matches(jsStr, regStr, RegexOptions.None);
            if (match.Count > 0)
            {
                resultsBlock.Statements.Add(pStatement);

                List<string> dups = new List<string>();

                foreach (Match typeMatch in match)
                {
                    if (typeMatch.Groups != null && typeMatch.Groups.Count > 1)
                    {
                        // based on the regex Mike wrote, we will always want the first capture.
                        // [0] is what we searched for
                        // [1] is what we are wanting to capture
                        string value = "\'" + typeMatch.Groups[1] + "\'";
                        if (!dups.Contains(value) && value.Length > 1)
                        {
                            dups.Add(value);

                            JsExpressionStatement insert = AstUtils.getArrayInsertStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME, value );
                            resultsBlock.Statements.Add(insert);
                        }
                    }
                }

                resultsBlock.Statements.Add( AstUtils.getJsReturnStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ) );
            }
            else
            {
                if( initializer != null )
                {
                    // if the super class path was defined, we have to no matter what make sure we call super.
                    resultsBlock.Statements.Add(pStatement);
                    resultsBlock.Statements.Add( AstUtils.getJsReturnStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ) );
                }
                else
                {
                    JsInvocationExpression initer = AstUtils.getEmptyArrayInvocationExpression();
                    resultsBlock.Statements.Add(AstUtils.getJsReturnStatement(initer));
                }
            }

            // define function
            JsFunction result = new JsFunction();
            // add the function block
            result.Block = resultsBlock;
            return result;
        }          
    }
}
