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
using SharpKit.Compiler;
using randori.compiler.constants;
using randori.compiler.utils;

namespace randori.compiler.builders
{

    class HtmlCacheFileBuilder
    {

        public const string FILE_NAME_PARAMETER = "fileName";

        protected string fileNameToCreate;
        protected IEnumerable<string> fileList;

        public HtmlCacheFileBuilder(string fileNameToCreate, IEnumerable<string> fileList)
        {
            this.fileNameToCreate = fileNameToCreate;
            this.fileList = fileList;
        }

        public SkJsFile buildHtmlCacheFile()
        {
            List<JsUnit> fileUnits = new List<JsUnit>();

            fileUnits.Add(getCachedHtmlFiles(RandoriClassNames.contentCache, fileList));

            // load and merge up the stuff.
            JsFile nJsFile = AstUtils.getNewJsFile(fileNameToCreate);
            nJsFile.Units = fileUnits;

            // add units
            SkJsFile nFile = AstUtils.getNewSkJsFile(nJsFile);

            return nFile;
        }

        protected JsUnit getCachedHtmlFiles ( string fileName, IEnumerable<string> fileList )
        {
            JsUnit result = AstUtils.getNewJsUnit();

            foreach (string fileUri in fileList)
            {
                result.Statements.Add(getCachedFileExpressionStatement(OutputNameConstants.PROPERTY_HTML_MERGED_FILES, fileName, fileUri));
            }

            return result;
        }

        protected JsExpressionStatement getCachedFileExpressionStatement( string arrayName, string fileName, string fileUri )
        {
            JsMemberExpression leftPref = AstUtils.getNewMemberExpression(fileName);

            JsAssignmentExpression newAssignment = new JsAssignmentExpression();

            string assoKey = arrayName + "[ '" + fileUri + "' ]";
            newAssignment.Left = AstUtils.getNewMemberExpression(assoKey, leftPref);
            newAssignment.Right = AstUtils.getCachedHtmlExpression(fileUri); ;

            JsExpressionStatement newStatement = new JsExpressionStatement();
            newStatement.Expression = newAssignment;
            return newStatement;
        }

        protected JsUnit getFunctionCachedHtmlForUri(string fileName, List<string> fileList)
        {
            JsUnit result = AstUtils.getNewJsUnit();
            
            // since this is static, prepend the class path.
            JsMemberExpression leftPref = AstUtils.getNewMemberExpression(fileName);

            JsAssignmentExpression newAssignment = new JsAssignmentExpression();
            newAssignment.Left = AstUtils.getNewMemberExpression(OutputNameConstants.FUNCTION_GET_CACHED_HTML_FOR_URI, leftPref);
            newAssignment.Right = getCachedHtmlFileFunction(OutputNameConstants.PROPERTY_HTML_MERGED_FILES);

            JsExpressionStatement newStatement = new JsExpressionStatement();
            newStatement.Expression = newAssignment;
            result.Statements.Add(newStatement);

            return result;
        }

        protected JsUnit getHtmlFileList(string fileName, List<string> fileList)
        {
            JsUnit result = AstUtils.getNewJsUnit();

            JsAssignmentExpression newAssignment = new JsAssignmentExpression();

            // since this is static, prepend the class path.
            JsMemberExpression leftPref = AstUtils.getNewMemberExpression(fileName);

            //shouldExcludeBasedOnNamespace;
            newAssignment.Left = AstUtils.getNewMemberExpression(OutputNameConstants.FUNCTION_GET_CACHED_FILE_LIST, leftPref);
            newAssignment.Right = getHtmlFileListFunction(fileList);

            JsExpressionStatement newStatement = new JsExpressionStatement();
            newStatement.Expression = newAssignment;
            result.Statements.Add(newStatement);

            return result;
        }

        protected JsFunction getCachedHtmlFileFunction(string arrayName)
        {
            JsFunction result = null;
            JsBlock resultsBlock = null;

            if ( arrayName != null )
            {
                JsIfStatement ifArrayNotNull = new JsIfStatement();
                string arrayKeyStr = arrayName + "[ " + FILE_NAME_PARAMETER + " ]";
                ifArrayNotNull.Condition = AstUtils.getJsBinaryExpression(AstUtils.getNewMemberExpression(arrayKeyStr), "!=", new JsNullExpression());

                JsBlock ifBlock = new JsBlock();
                ifBlock.Statements = new List<JsStatement>();
                ifArrayNotNull.IfStatement = ifBlock;
                ifBlock.Statements.Add(AstUtils.getJsReturnStatement(arrayKeyStr));

                // function contents
                resultsBlock = new JsBlock();
                resultsBlock.Statements = new List<JsStatement>();
                resultsBlock.Statements.Add(ifArrayNotNull);
                resultsBlock.Statements.Add(new JsReturnStatement());
            }

            if (resultsBlock != null)
            {
                result = new JsFunction();
                result.Parameters = new List<string>();
                result.Parameters.Add(FILE_NAME_PARAMETER);
                result.Block = resultsBlock;
            }

            return result;
        }

        protected JsFunction getHtmlFileListFunction(List<string> fileList)
        {
            JsFunction result = null;
            JsBlock resultsBlock = null;

            if (fileList != null && fileList.Count > 0)
            {
                // function contents
                resultsBlock = new JsBlock();
                resultsBlock.Statements = new List<JsStatement>();
                resultsBlock.Statements.Add(AstUtils.getJsVariableDeclarationStatement(InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME,
                                                                                       AstUtils.getEmptyArrayInvocationExpression()));
                
                foreach (string fileName in fileList)
                {
                    string fileNameStr = "{n:\'" + fileName + "\'}";
                    resultsBlock.Statements.Add( AstUtils.getArrayInsertStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME, fileNameStr ) );
                }

                resultsBlock.Statements.Add( AstUtils.getJsReturnStatement( InjectionPointVariableConstants.SWITCH_RETURN_VARIABLE_NAME) );
            }

            if (resultsBlock != null)
            {
                result = new JsFunction();
                result.Block = resultsBlock;
            }

            return result;
        }

    }
}
