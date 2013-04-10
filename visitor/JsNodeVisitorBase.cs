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
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using SharpKit.JavaScript.Ast;

namespace randori.compiler.visitor
{
    /*
    * Since SharpKit doesn't offer a Visitor pattern concept, here's a inverted one to spider a JsNode.
    */
    public class JsNodeVisitorBase
    {

        protected DefaultResolvedTypeDefinition cSharpDef;
        protected JsNode jsEntity;

        public JsNodeVisitorBase ( DefaultResolvedTypeDefinition arg1, JsNode arg2 )
        {
            this.cSharpDef = arg1;
            this.jsEntity = arg2;
        }
        
        protected void visit( JsNode node )
        {
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case JsNodeType.AssignmentExpression:
                        _visit((JsAssignmentExpression) node);
                        break;
                    case JsNodeType.BinaryExpression:
                        _visit( ( JsBinaryExpression ) node );
                        break;
                    case JsNodeType.Block:
                        _visit( ( JsBlock ) node );
                        break;
                    case JsNodeType.BreakStatement:
                        _visit((JsBreakStatement) node);
                        break;
                    case JsNodeType.CatchClause:
                        _visit((JsCatchClause) node);
                        break;
                    case JsNodeType.CodeExpression:
                        _visit((JsCodeExpression) node);
                        break;
                    case JsNodeType.CommentStatement:
                        _visit((JsCommentStatement) node);
                        break;
                    case JsNodeType.ConditionalExpression:
                        _visit((JsConditionalExpression) node);
                        break;
                    case JsNodeType.ContinueStatement:
                        _visit((JsContinueStatement) node);
                        break;
                    case JsNodeType.DoWhileStatement:
                        _visit((JsDoWhileStatement) node);
                        break;
                    case JsNodeType.Expression:
                        _visit((JsExpression) node);
                        break;
                    case JsNodeType.ExpressionStatement:
                        _visit((JsExpressionStatement) node);
                        break;
                    case JsNodeType.ExternalFileUnit:
                        _visit((JsExternalFileUnit) node);
                        break;
                    case JsNodeType.ForInStatement:
                        _visit((JsForInStatement) node);
                        break;
                    case JsNodeType.ForStatement:
                        _visit((JsForStatement) node);
                        break;
                    case JsNodeType.Function:
                        _visit((JsFunction) node);
                        break;
                    case JsNodeType.IfStatement:
                        _visit((JsIfStatement) node);
                        break;
                    case JsNodeType.IndexerAccessExpression:
                        _visit((JsIndexerAccessExpression) node);
                        break;
                    case JsNodeType.InvocationExpression:
                        _visit((JsInvocationExpression) node);
                        break;
                    case JsNodeType.JsonArrayExpression:
                        _visit((JsJsonArrayExpression) node);
                        break;
                    case JsNodeType.JsonMember:
                        _visit((JsJsonMember) node);
                        break;
                    case JsNodeType.JsonNameValue:
                        _visit((JsJsonNameValue) node);
                        break;
                    case JsNodeType.JsonObjectExpression:
                        _visit((JsJsonObjectExpression) node);
                        break;
                    case JsNodeType.MemberExpression:
                        _visit((JsMemberExpression) node);
                        break;
                    case JsNodeType.NewObjectExpression:
                        _visit((JsNewObjectExpression) node);
                        break;
                    case JsNodeType.NodeList:
                        _visit((JsNodeList) node);
                        break;
                    case JsNodeType.NullExpression:
                        _visit((JsNullExpression) node);
                        break;
                    case JsNodeType.NumberExpression:
                        _visit((JsNumberExpression) node);
                        break;
                    case JsNodeType.ParenthesizedExpression:
                        _visit((JsParenthesizedExpression) node);
                        break;
                    case JsNodeType.PostUnaryExpression:
                        _visit((JsPostUnaryExpression) node);
                        break;
                    case JsNodeType.PreUnaryExpression:
                        _visit((JsPreUnaryExpression) node);
                        break;
                    case JsNodeType.RegexExpression:
                        _visit((JsRegexExpression) node);
                        break;
                    case JsNodeType.ReturnStatement:
                        _visit((JsReturnStatement) node);
                        break;
                    case JsNodeType.Statement:
                        _visit((JsStatement) node);
                        break;
                    case JsNodeType.StatementExpressionList:
                        _visit((JsStatementExpressionList) node);
                        break;
                    case JsNodeType.StringExpression:
                        _visit((JsStringExpression) node);
                        break;
                    case JsNodeType.SwitchLabel:
                        _visit((JsSwitchLabel) node);
                        break;
                    case JsNodeType.SwitchSection:
                        _visit((JsSwitchSection) node);
                        break;
                    case JsNodeType.SwitchStatement:
                        _visit((JsSwitchStatement) node);
                        break;
                    case JsNodeType.This:
                        _visit((JsThis) node);
                        break;
                    case JsNodeType.ThrowStatement:
                        _visit((JsThrowStatement) node);
                        break;
                    case JsNodeType.TryStatement:
                        _visit((JsTryStatement) node);
                        break;
                    case JsNodeType.Unit:
                        _visit((JsUnit) node);
                        break;
                    case JsNodeType.VariableDeclarationExpression:
                        _visit((JsVariableDeclarationExpression) node);
                        break;
                    case JsNodeType.VariableDeclarationStatement:
                        _visit((JsVariableDeclarationStatement) node);
                        break;
                    case JsNodeType.VariableDeclarator:
                        _visit((JsVariableDeclarator) node);
                        break;
                    case JsNodeType.WhileStatement:
                        _visit((JsWhileStatement) node);
                        break;
                    case JsNodeType.UseStrictStatement:
                        _visit((JsUseStrictStatement) node);
                        break;
                }
            }

        }

        protected virtual void _visit ( JsAssignmentExpression node )
        {
            throw new NotImplementedException( "JsAssignmentExpression" );
        }

        protected virtual void _visit ( JsBinaryExpression node )
        {
            throw new NotImplementedException( "JsBinaryExpression" );
        }

        protected virtual void _visit ( JsBlock node )
        {
            throw new NotImplementedException( "JsBlock" );
        }

        protected virtual void _visit ( JsBreakStatement node )
        {
            throw new NotImplementedException( "JsBreakStatement" );
        }

        protected virtual void _visit ( JsCatchClause node )
        {
            throw new NotImplementedException( "JsCatchClause" );
        }

        protected virtual void _visit ( JsCodeExpression node )
        {
            throw new NotImplementedException( "JsCodeExpression" );
        }

        protected virtual void _visit ( JsCommentStatement node )
        {
            throw new NotImplementedException( "JsCommentStatement" );
        }

        protected virtual void _visit ( JsConditionalExpression node )
        {
            throw new NotImplementedException( "JsConditionalExpression" );
        }

        protected virtual void _visit ( JsContinueStatement node )
        {
            throw new NotImplementedException( "JsContinueStatement" );
        }

        protected virtual void _visit ( JsDoWhileStatement node )
        {
            throw new NotImplementedException( "JsDoWhileStatement" );
        }

        protected virtual void _visit ( JsExpression node )
        {
            throw new NotImplementedException( "JsExpression" );
        }

        protected virtual void _visit ( JsExpressionStatement node )
        {
            throw new NotImplementedException( "JsExpressionStatement" );
        }

        protected virtual void _visit ( JsExternalFileUnit node )
        {
            throw new NotImplementedException( "JsExternalFileUnit" );
        }

        protected virtual void _visit ( JsForInStatement node )
        {
            throw new NotImplementedException( "JsForInStatement" );
        }

        protected virtual void _visit ( JsForStatement node )
        {
            throw new NotImplementedException( "JsForStatement" );
        }

        protected virtual void _visit ( JsFunction node )
        {
            throw new NotImplementedException( "JsFunction" );
        }

        protected virtual void _visit ( JsIfStatement node )
        {
            throw new NotImplementedException( "JsIfStatement" );
        }

        protected virtual void _visit ( JsIndexerAccessExpression node )
        {
            throw new NotImplementedException( "JsIndexerAccessExpression" );
        }

        protected virtual void _visit ( JsInvocationExpression node )
        {
            throw new NotImplementedException( "JsInvocationExpression" );
        }

        protected virtual void _visit ( JsJsonArrayExpression node )
        {
            throw new NotImplementedException( "JsJsonArrayExpression" );
        }

        protected virtual void _visit ( JsJsonMember node )
        {
            throw new NotImplementedException( "JsJsonMember" );
        }

        protected virtual void _visit ( JsJsonNameValue node )
        {
            throw new NotImplementedException( "JsJsonNameValue" );
        }

        protected virtual void _visit ( JsJsonObjectExpression node )
        {
            throw new NotImplementedException( "JsJsonObjectExpression" );
        }

        protected virtual void _visit ( JsMemberExpression node )
        {
            throw new NotImplementedException( "JsMemberExpression" );
        }

        protected virtual void _visit ( JsNewObjectExpression node )
        {
            throw new NotImplementedException( "JsNewObjectExpression" );
        }

        protected virtual void _visit ( JsNodeList node )
        {
            throw new NotImplementedException( "JsNodeList" );
        }

        protected virtual void _visit ( JsNullExpression node )
        {
            throw new NotImplementedException( "JsNullExpression" );
        }

        protected virtual void _visit ( JsNumberExpression node )
        {
            throw new NotImplementedException( "JsNumberExpression" );
        }

        protected virtual void _visit ( JsParenthesizedExpression node )
        {
            throw new NotImplementedException( "JsParenthesizedExpression" );
        }

        protected virtual void _visit ( JsPostUnaryExpression node )
        {
            throw new NotImplementedException( "JsPostUnaryExpression" );
        }

        protected virtual void _visit ( JsPreUnaryExpression node )
        {
            throw new NotImplementedException( "JsPreUnaryExpression" );
        }

        protected virtual void _visit ( JsRegexExpression node )
        {
            throw new NotImplementedException( "JsRegexExpression" );
        }

        protected virtual void _visit ( JsReturnStatement node )
        {
            throw new NotImplementedException( "JsReturnStatement" );
        }

        protected virtual void _visit ( JsStatement node )
        {
            throw new NotImplementedException( "JsStatement" );
        }

        protected virtual void _visit ( JsStatementExpressionList node )
        {
            throw new NotImplementedException( "JsStatementExpressionList" );
        }

        protected virtual void _visit ( JsStringExpression node )
        {
            throw new NotImplementedException( "JsStringExpression" );
        }

        protected virtual void _visit ( JsSwitchLabel node )
        {
            throw new NotImplementedException( "JsSwitchLabel" );
        }

        protected virtual void _visit ( JsSwitchSection node )
        {
            throw new NotImplementedException( "JsSwitchSection" );
        }

        protected virtual void _visit ( JsSwitchStatement node )
        {
            throw new NotImplementedException( "JsSwitchStatement" );
        }

        protected virtual void _visit ( JsThis node )
        {
            throw new NotImplementedException( "JsThis" );
        }

        protected virtual void _visit ( JsThrowStatement node )
        {
            throw new NotImplementedException( "JsThrowStatement" );
        }

        protected virtual void _visit ( JsTryStatement node )
        {
            throw new NotImplementedException( "JsTryStatement" );
        }

        protected virtual void _visit ( JsUnit node )
        {
            throw new NotImplementedException( "JsUnit" );
        }

        protected virtual void _visit ( JsVariableDeclarationExpression node )
        {
            throw new NotImplementedException( "JsVariableDeclarationExpression" );
        }

        protected virtual void _visit ( JsVariableDeclarationStatement node )
        {
            throw new NotImplementedException( "JsVariableDeclarationStatement" );
        }

        protected virtual void _visit ( JsVariableDeclarator node )
        {
            throw new NotImplementedException( "JsVariableDeclarator" );
        }

        protected virtual void _visit ( JsWhileStatement node )
        {
            throw new NotImplementedException( "JsWhileStatement" );
        }

        protected virtual void _visit( JsUseStrictStatement node )
        {
            throw new NotImplementedException( "JsUseStrictStatement" );
        }
    }
}
