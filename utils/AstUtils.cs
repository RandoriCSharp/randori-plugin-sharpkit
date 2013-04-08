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

using SharpKit.JavaScript.Ast;
using System.Collections.Generic;
using SharpKit.Compiler;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace randori.compiler.utils
{
    class AstUtils
    {

        public static JsMemberExpression getNewMemberExpression(string name, JsMemberExpression previousMember = null)
        {
            JsMemberExpression result = new JsMemberExpression();
            result.Name = name;
            result.PreviousMember = previousMember;
            return result;
        }

        public static JsVariableDeclarator getJsVariableDeclarator(string variableName, JsInvocationExpression initer = null)
        {
            JsVariableDeclarator result = new JsVariableDeclarator();
            result.Name = variableName;

            // should the variable be initialized?
            if (initer != null)
                result.Initializer = initer;

            return result;
        }

        public static JsCodeExpression getJsCodeExpression(string code)
        {
            JsCodeExpression result = new JsCodeExpression();
            result.Code = code;
            return result;
        }

        public static JsVariableDeclarationStatement getJsVariableDeclarationStatement(string variableName, JsInvocationExpression initer = null)
        {
            // declare function return object
            JsVariableDeclarationStatement pResult = new JsVariableDeclarationStatement();
            JsVariableDeclarationExpression pExpression = new JsVariableDeclarationExpression();
            pResult.Declaration = pExpression;

            // add variable declarations
            pExpression.Declarators = new List<JsVariableDeclarator>();
            pExpression.Declarators.Add(getJsVariableDeclarator(variableName, initer));
            return pResult;
        }

        public static JsReturnStatement getJsReturnStatement(string variableName)
        {
            JsReturnStatement pReturn = new JsReturnStatement();
            pReturn.Expression = getNewMemberExpression( variableName );
            return pReturn;
        }

        public static JsReturnStatement getJsReturnStatement(JsExpression exp)
        {
            JsReturnStatement pReturn = new JsReturnStatement();
            pReturn.Expression = exp;
            return pReturn;
        }

        public static JsExpressionStatement getJsExpressionStatement( JsExpression exp )
        {
            JsExpressionStatement result = new JsExpressionStatement();
            result.Expression = exp;
            return result;
        }

        public static JsAssignmentExpression getJsAssignmentStatement(JsExpression leftExp, string operatorStr, JsExpression rightExp)
        {
            JsAssignmentExpression result = new JsAssignmentExpression();
            result.Left = leftExp;
            result.Operator = operatorStr;
            result.Right = rightExp;
            return result;
        }

        public static JsBinaryExpression getJsBinaryExpression( JsExpression leftExp, string operatorStr, JsExpression rightExp )
        {
            JsBinaryExpression result = new JsBinaryExpression();
            result.Left = leftExp;
            result.Operator = operatorStr;
            result.Right = rightExp;
            return result;
        }

        public static JsFile getNewJsFile(string fileName)
        {
            JsFile result = null;

            if (fileName != null)
            {
                result = new JsFile();
                result.Filename = fileName;
            }

            return result;
        }

        public static SkJsFile getNewSkJsFile(JsFile newFile)
        {
            SkJsFile result = null;

            if (newFile != null)
            {
                result = new SkJsFile();
                result.JsFile = newFile;
            }

            return result;
        }

        public static JsUnit getNewJsUnit()
        {
            JsUnit result = new JsUnit();
            result.Statements = new List<JsStatement>();
            return result;
        }

        public static JsCommentStatement getCommentStatement(string commentStr)
        {
            JsCommentStatement result = new JsCommentStatement();
            if (commentStr != null)
            {
                result.Text = commentStr;
            }
            return result;
        }

        /***************************************************************************************************************************/
        /**************************************** Switch Statement Utilities *******************************************************/
        /***************************************************************************************************************************/

        public static JsSwitchSection getJsSwitchSection(string label, bool isDefault = false)
        {
            JsSwitchSection result = new JsSwitchSection();
            result.Labels = new List<JsSwitchLabel>();
            result.Labels.Add(getJsSwitchLabel(label, isDefault));
            return result;
        }

        public static JsSwitchLabel getJsSwitchLabel(string label, bool isDefault = false)
        {
            JsSwitchLabel result = new JsSwitchLabel();
            result.IsDefault = isDefault;
            result.Expression = getJsCodeExpression(label);
            return result;
        }

        // not necessary, but putting it in so someone doesn't have to dig through the call stack to figure out what it is
        public static JsBreakStatement getJsBreakStatement()
        {
            return new JsBreakStatement();
        }


        /***************************************************************************************************************************/
        /**************************************** Array Statement Utilities *******************************************************/
        /***************************************************************************************************************************/

        // example: []
        public static JsInvocationExpression getEmptyArrayInvocationExpression()
        {
            JsInvocationExpression initer = new JsInvocationExpression();
            initer.Arguments = new List<JsExpression>();
            initer.Member = new JsExpression();
            initer.OmitParanthesis = true;

            JsJsonArrayExpression args = new JsJsonArrayExpression();
            args.Items = new List<JsExpression>();
            initer.Arguments.Add(args);

            return initer;
        }

        // need a better name...
        // example: randori.apples.SuperClass.injectionPoints(t);
        public static JsInvocationExpression getStaticMethodCallInvocationExpression(string methodName, string classPath, List<JsExpression> args)
        {
            //randori.apples.SuperClass
            JsMemberExpression classPathExpr = AstUtils.getNewMemberExpression(classPath);

            //methodName
            JsInvocationExpression initer = new JsInvocationExpression();
            initer.Member =  AstUtils.getNewMemberExpression(methodName, classPathExpr);

            // Method Argumenets
            if (args != null)
            {
                initer.Arguments = args;
            }

            return initer;
        }

        public static JsExpressionStatement getArrayInsertStatement(string arrayName, string itemValue)
        {
            JsInvocationExpression constInvoke = new JsInvocationExpression();

            /********************************************************/
            // build add statement to array
            /********************************************************/
            JsMemberExpression arrayMember = AstUtils.getNewMemberExpression(arrayName);
            JsMemberExpression addMember = AstUtils.getNewMemberExpression("push", arrayMember);
            constInvoke.Member = addMember;
            constInvoke.Arguments = new List<JsExpression>();

            constInvoke.Arguments.Add(AstUtils.getJsCodeExpression(itemValue));

            JsExpressionStatement constructorStatement = AstUtils.getJsExpressionStatement(constInvoke);
            return constructorStatement;
        }

        /***************************************************************************************************************************/
        /**************************************** Array Statement Utilities *******************************************************/
        /***************************************************************************************************************************/

        public static JsExpressionStatement getStaticPropertyStatement(string propertyName, string classPath)
        {
            JsBinaryExpression newAssignment = new JsBinaryExpression();
            newAssignment.Operator = "=";

            // since this is static, prepend the class path.
            JsMemberExpression leftPref = AstUtils.getNewMemberExpression(classPath);
            // set the property name
            newAssignment.Left = AstUtils.getNewMemberExpression(propertyName, leftPref);
            // set the desired value for the property
            // only support strings, ref
            newAssignment.Right = AstUtils.getNewMemberExpression("\"" + classPath + "\"");

            JsExpressionStatement newStatement = new JsExpressionStatement();
            newStatement.Expression = newAssignment;
            return newStatement;
        }

        /***************************************************************************************************************************/
        /********************************** Load & Geberate String Expression for a Html File **************************************/
        /***************************************************************************************************************************/

        public static JsExpression getCachedHtmlExpression(string fileUri)
        {
            string fileHtml = "";

            FileStream fs = null;
            StreamReader streamReader = null;

            try
            {
                fs = new FileStream(@fileUri, FileMode.Open, FileAccess.Read);
                streamReader = new StreamReader(fs);
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    string tLine = line.Trim();
                    fileHtml += line;
                }
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }

            string findExp = "\"";
            string replaceExp = "/\"";
            fileHtml = Regex.Replace(fileHtml, findExp, replaceExp);

            return AstUtils.getNewMemberExpression("\""+ fileHtml + "\"");
        }

        /***************************************************************************************************************************/
        /************************************************ For Getters and Setters **************************************************/
        /***************************************************************************************************************************/

        public static JsExpressionStatement getGetterSetterExpression(string propertyName, string returnName)
        {
            if(propertyName == null)
            {
                return null;
            }

            JsMemberExpression leftExp = getNewMemberExpression(propertyName);
            
            //
            JsBlock rightBlock = new JsBlock();
            rightBlock.Statements = new List<JsStatement>();
            rightBlock.Statements.Add(getJsReturnStatement(returnName));
            
            //
            JsFunction rightExp = new JsFunction();
            rightExp.Block = rightBlock;

            JsExpressionStatement results = new JsExpressionStatement();
            results.Expression = getJsAssignmentStatement(leftExp, null, rightExp);

            return results;
        }


    }
}
