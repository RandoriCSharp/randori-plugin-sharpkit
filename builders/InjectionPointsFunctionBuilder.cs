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

using System;
using System.Collections.Generic;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.TypeSystem;
using SharpKit.Compiler;
using randori.compiler.constants;
using randori.compiler.utils;

namespace randori.compiler.builders
{
    class InjectionPointsFunctionBuilder
    {

        protected DefaultResolvedTypeDefinition cSharpDef;
        protected JsUnit jsEntity;

        public InjectionPointsFunctionBuilder(DefaultResolvedTypeDefinition arg1, JsUnit arg2)
        {
            this.cSharpDef = arg1;
            this.jsEntity = arg2;
        }

        public JsExpressionStatement getInjectionPointsExpression()
        {
            if (cSharpDef != null)
            {
                JsAssignmentExpression newAssignment = new JsAssignmentExpression();

                // since this is static, prepend the class path.
                JsMemberExpression leftPref = AstUtils.getNewMemberExpression(cSharpDef.FullName);

                //shouldExcludeBasedOnNamespace;
                newAssignment.Left = AstUtils.getNewMemberExpression(OutputNameConstants.PROPERTY_INJECTION_POINTS, leftPref);
                newAssignment.Right = getInjectionPointsFunction( cSharpDef );

                JsExpressionStatement newStatement = new JsExpressionStatement();
                newStatement.Expression = newAssignment;
                return newStatement;
            }

            return null;
        }

        JsFunction getInjectionPointsFunction(DefaultResolvedTypeDefinition arg1)
        {
            IList<IMethod> constructors = IEntityUtils.getClassConstructors(arg1);
            IList<IField> fields = IEntityUtils.getFieldsByAttribute( arg1, RandoriClassNames.metadataInject );
            IList<IProperty> properties = IEntityUtils.getPropertiesByAttribute( arg1, RandoriClassNames.metadataInject );
            IList<IField> views = IEntityUtils.getFieldsByAttribute( arg1, RandoriClassNames.metadataView );
            IList<IMethod> methods = IEntityUtils.getClassMethodsByAttribute( arg1, RandoriClassNames.metadataInject );

            JsFunction result = new JsFunction();
            result.Parameters = new List<string>();
            result.Parameters.Add(InjectionPointVariableConstants.INJECTION_POINT_PARAMETER_NAME);

            // function contents
            JsBlock resultsBlock = new JsBlock();
            resultsBlock.Statements = new List<JsStatement>();
            
            // add the block
            result.Block = resultsBlock;

            // Determine what the super class is in the event we need to call the super class's injectionPoints method
            string superClassPath = null;
            foreach (IType type in arg1.DirectBaseTypes)
            {
                // Careful, type.FullName could be all sorts of stuff, like interfaces
                if (type.Kind == TypeKind.Class)
                {
                    bool initArray = GuiceUtils.shouldExcludeBasedOnNamespace(type.Namespace);
                    if (!initArray)
                    {
                        superClassPath = type.FullName;
                    }
                }
            }

            JsSwitchStatement injectionPoints = getInjectionSwitchStatement( InjectionPointVariableConstants.INJECTION_POINT_PARAMETER_NAME, constructors, fields, methods, views, properties, superClassPath );
            if (injectionPoints != null)
            {
                resultsBlock.Statements.Add( AstUtils.getJsVariableDeclarationStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ) );
                resultsBlock.Statements.Add(injectionPoints);
                // return the result variable
                resultsBlock.Statements.Add( AstUtils.getJsReturnStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ) );
            }
            else
            {
                JsInvocationExpression initer = AstUtils.getEmptyArrayInvocationExpression();
                resultsBlock.Statements.Add(AstUtils.getJsReturnStatement(initer));
            }

            return result;
        }

        // Builds out the injection point JSSwitchStatement for the items passed in for a particiular class
        JsSwitchStatement getInjectionSwitchStatement(string switchVariableStr, IList<IMethod> constructors, IList<IField> fields, IList<IMethod> methods, IList<IField> views, IList<IProperty> properties, string superClassPath = null)
        {
            // Constructors 0
            JsSwitchSection constructorSection = getConstructorSwitchSection(InjectionPointVariableConstants.SWITCH_CONSTRUCTOR_BLOCK, constructors);

            // in the case of fields, properties, methods and views, we will call super if a superClassPath is defined.
            JsInvocationExpression initializer = null;
            if (superClassPath != null)
            {
                JsMemberExpression param = AstUtils.getNewMemberExpression( InjectionPointVariableConstants.INJECTION_POINT_PARAMETER_NAME );
                List<JsExpression> args = new List<JsExpression>();
                args.Add(param);

                initializer = AstUtils.getStaticMethodCallInvocationExpression(OutputNameConstants.PROPERTY_INJECTION_POINTS, superClassPath, args);
            }

            // Fields 1
            JsSwitchSection fieldsSection = getFieldsSwitchSection( InjectionPointVariableConstants.SWITCH_FIELDS_BLOCK, fields, initializer );
            
            // Methods 2
            JsSwitchSection methodsSection = getMethodsSwitchSection( InjectionPointVariableConstants.SWITCH_METHODS_BLOCK, methods );
            
            // Views 3
            JsSwitchSection viewsSection = getFieldsSwitchSection( InjectionPointVariableConstants.SWITCH_VIEWS_BLOCK, views, initializer );
            
            // Properties 4
            JsSwitchSection propertiesSection = getPropertiesSwitchSection( InjectionPointVariableConstants.SWITCH_PROPERTIES_BLOCK, properties, initializer );

            // if everything is null, there is nothing left to do, return null
            if ( constructorSection == null && fieldsSection == null && methodsSection == null && viewsSection == null && propertiesSection == null )
            {
                return null;
            }

            // If we have data, let's build out the JSSwitchStatement
            JsSwitchStatement results = new JsSwitchStatement();
            results.Sections = new List<JsSwitchSection>();
            results.Expression = AstUtils.getNewMemberExpression(switchVariableStr);

            if (constructorSection != null)
            {
                results.Sections.Add(constructorSection);
            }

            if (fieldsSection != null)
            {
                results.Sections.Add(fieldsSection);
            }

            if (methodsSection != null)
            {
                results.Sections.Add(methodsSection);
            }

            if (viewsSection != null)
            {
                results.Sections.Add(viewsSection);
            }

            if (propertiesSection != null)
            {
                results.Sections.Add(propertiesSection);
            }

            if (results != null)
            {
                results.Sections.Add(getSwitchDefaultSection());
            }

            return results;
        }

        // Determines if there are any contructors for a particular class and builds a Switch Case block to be insert into the InjectionPoints Switch Section
        JsSwitchSection getConstructorSwitchSection(string caseIndex, IList<IMethod> constructors)
        {
            // No constructors found
            if (constructors == null || constructors.Count == 0)
            {
                return null;
            }

            // we can only use one.
            if (constructors.Count > 1)
            {
                throw new SystemException("Multiple constructors are not supported.");
            }

            JsSwitchSection results = null;

            // Push in any business we need to do...
            // JSExpressionStatement
            foreach (IMethod method in constructors)
            {
                // IF WE SUPPORT MULTIPLE CONSTRUCTORS, THIS NEEDS TO MOVE.
                IList<JsExpressionStatement> parameters = getParametersForIMethod(method);
                if (parameters != null)
                {
                    results = AstUtils.getJsSwitchSection(caseIndex);
                    results.Statements = new List<JsStatement>();

                    // initalized the result array
                    JsBinaryExpression pExpression = AstUtils.getJsBinaryExpression( AstUtils.getNewMemberExpression( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ),
                                                                                    "=",
                                                                                    AstUtils.getEmptyArrayInvocationExpression());

                    results.Statements.Add(AstUtils.getJsExpressionStatement(pExpression));

                    foreach (JsExpressionStatement parm in parameters)
                    {
                        results.Statements.Add(parm);
                    }

                    results.Statements.Add(new JsBreakStatement());
                }
            }

            return results;
        }

        JsSwitchSection getMethodsSwitchSection(string caseIndex, IList<IMethod> methods)
        {
            // if fields come in, regardless, build it.
            // if no fields, and a override, build it. (i.e., we must create the call to super even if this class does not define any fields
            if (methods == null)
            {
                return null;
            }

            JsSwitchSection results = AstUtils.getJsSwitchSection(caseIndex);
            results.Statements = new List<JsStatement>();

            // initalized the result array
            JsBinaryExpression pExpression = AstUtils.getJsBinaryExpression( AstUtils.getNewMemberExpression( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ),
                                                                            "=",
                                                                            AstUtils.getEmptyArrayInvocationExpression());

            JsExpressionStatement pStatement = AstUtils.getJsExpressionStatement(pExpression);
            results.Statements.Add(pStatement);


            // JSExpressionStatement
            foreach (IMethod method in methods)
            {
                JsExpressionStatement methodStatement = getMethodInjectStatement(method);
                results.Statements.Add( methodStatement );
            }

            // Insert Break
            results.Statements.Add(new JsBreakStatement());

            return results;
        }

        public JsExpressionStatement getMethodInjectStatement(IMethod method)
        {
            string parameterStr = null;
            if (method.Parameters != null && method.Parameters.Count > 0)
            {
                foreach (IParameter param in method.Parameters)
                {
                    if (parameterStr != null)
                    {
                        parameterStr += ", ";
                    }
                    else
                    {
                        parameterStr = "";
                    }
                    parameterStr += GuiceUtils.getInjectionPointString(param);
                }
            }

            string jsName = SkJs.GetEntityJsName(method);
            ITypeDefinition classDef = method.DeclaringTypeDefinition;

            // we will not pass in the classDef.FullName, this is because we don't want the "type" section generated for methods.
            string codeStr = GuiceUtils.getInjectonPointString(jsName, null, method.Namespace, null, true, null, parameterStr);
            return AstUtils.getArrayInsertStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME, codeStr );
        }

        public JsExpressionStatement getMethodInjectStatement(IProperty property, IMethod method)
        {
            string parameterStr = null;
            if (method.Parameters != null && method.Parameters.Count > 0)
            {
                foreach (IParameter param in method.Parameters)
                {
                    if (parameterStr != null)
                    {
                        parameterStr += ", ";
                    }
                    else
                    {
                        parameterStr = "";
                    }
                    parameterStr += GuiceUtils.getInjectionPointString(param);
                }
            }

            string jsName = SkJs.GetEntityJsName(property);
            ITypeDefinition classDef = method.DeclaringTypeDefinition;

            string codeStr = GuiceUtils.getInjectonPointString( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME, jsName, classDef.FullName, method.Namespace, false, null, parameterStr );
            return AstUtils.getArrayInsertStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME, codeStr );
        }

        JsSwitchSection getFieldsSwitchSection(string caseIndex, IEnumerable<IField> fields, JsInvocationExpression initalizerOverride)
        {
            // if fields come in, regardless, build it.
            // if no fields, and a override, build it. (i.e., we must create the call to super even if this class does not define any fields
            if (initalizerOverride == null && fields == null)
            {
                return null;
            }

            JsSwitchSection results = AstUtils.getJsSwitchSection(caseIndex);
            results.Statements = new List<JsStatement>();

            if (initalizerOverride == null)
            {
                initalizerOverride = AstUtils.getEmptyArrayInvocationExpression();
            }

            JsBinaryExpression pExpression = AstUtils.getJsBinaryExpression(AstUtils.getNewMemberExpression( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ),
                                                                            "=",
                                                                            initalizerOverride);

            JsExpressionStatement pDeclaration = AstUtils.getJsExpressionStatement(pExpression);
            results.Statements.Add(pDeclaration);

            if (fields != null)
            {
                foreach (IField field in fields)
                {
                    bool fRequired = IEntityUtils.isFieldRequired(field);
                    string fAnnotation = IEntityUtils.getInjectAnnotation(field);
                    string fDefaultValue = null;

                    if (field.ConstantValue != null)
                    {
                        // if a default value is defined, set it.
                        fDefaultValue = field.ConstantValue.ToString();
                    }

                    string codeStr = GuiceUtils.getInjectonPointString(field.Name, field.Type.FullName, field.Type.Namespace, fDefaultValue, fRequired, fAnnotation);
                    results.Statements.Add( AstUtils.getArrayInsertStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME, codeStr ) );
                }
            }

            // add the break last...
            results.Statements.Add(AstUtils.getJsBreakStatement());

            return results;
        }

        JsSwitchSection getPropertiesSwitchSection(string caseIndex, IEnumerable<IProperty> properties, JsInvocationExpression initalizerOverride)
        {
            // if fields come in, regardless, build it.
            // if no fields, and a override, build it. (i.e., we must create the call to super even if this class does not define any fields
            if (initalizerOverride == null && properties == null)
            {
                return null;
            }

            JsSwitchSection results = AstUtils.getJsSwitchSection(caseIndex);
            results.Statements = new List<JsStatement>();

            if (initalizerOverride == null)
            {
                initalizerOverride = AstUtils.getEmptyArrayInvocationExpression();
            }

            JsBinaryExpression pExpression = AstUtils.getJsBinaryExpression(
                                                                            AstUtils.getNewMemberExpression( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ),
                                                                            "=",
                                                                            initalizerOverride
                                                                            );

            JsExpressionStatement pDeclaration = AstUtils.getJsExpressionStatement(pExpression);
            results.Statements.Add(pDeclaration);

            if (properties != null)
            {
                foreach (IProperty property in properties)
                {
                    if( property.CanSet )
                    {
                        JsExpressionStatement propertyStatement = getMethodInjectStatement(property.Setter);
                        results.Statements.Add(propertyStatement);
                    }
                }
            }

            // add the break last...
            results.Statements.Add(AstUtils.getJsBreakStatement());

            return results;
        }

        JsSwitchSection getSwitchDefaultSection()
        {
            JsSwitchSection results = AstUtils.getJsSwitchSection( InjectionPointVariableConstants.SWITCH_DEFAULT_BLOCK, true );
            results.Statements = new List<JsStatement>();

            // initalized the result array
            JsBinaryExpression pExpression = AstUtils.getJsBinaryExpression( AstUtils.getNewMemberExpression( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME ),
                                                                            "=",
                                                                            AstUtils.getEmptyArrayInvocationExpression());

            JsExpressionStatement pStatement = AstUtils.getJsExpressionStatement(pExpression);
            results.Statements.Add(pStatement);

            // break;
            results.Statements.Add(AstUtils.getJsBreakStatement());
            return results;
        }

        public IList<JsExpressionStatement> getParametersForIMethod(IMethod method)
        {
            IList<JsExpressionStatement> results = null;

            // adding a constructor statement when there are no arguements will crash the sharpkit compiler.
            if (method.Parameters != null && method.Parameters.Count > 0)
            {
                results = new List<JsExpressionStatement>();
                foreach (IParameter mParm in method.Parameters)
                {
                    string codeStr = GuiceUtils.getInjectionPointString(mParm);
                    results.Add( AstUtils.getArrayInsertStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME, codeStr ) );
                }
            }

            return results;
        }
    }
}
