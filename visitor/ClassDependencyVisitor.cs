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
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using SharpKit.JavaScript.Ast;

namespace randori.compiler.visitor
{
    /*
     * This purpose of this class is to cycle through a JsNode and find any references to other classes that it is dependent on
     * to later be loaded by the guice dependency loader.
     */
    public class ClassDependencyVisitor : JsNodeVisitorBase
    {
        public List<string> dependencyList { get; set; }

        public ClassDependencyVisitor(DefaultResolvedTypeDefinition arg1, JsNode arg2)
            : base(arg1, arg2)
        {
            dependencyList = new List<string>();
            buildDependencyList();
        }

        protected void buildDependencyList()
        {
            JsUnit jsUnit = (JsUnit) jsEntity;
            if (jsUnit != null)
            {
                foreach (JsNode node in jsUnit.Statements)
                {
                    visit(node);
                }
            }
        }

        protected override void _visit(JsAssignmentExpression node)
        {
            if (node != null)
            {
                visit(node.Left);
                visit(node.Right);
            }
        }

        protected override void _visit(JsBinaryExpression node)
        {
            if (node != null)
            {
                visit(node.Left);
                visit(node.Right);
            }
        }

        protected override void _visit(JsBlock node)
        {
            if (node != null)
            {
                foreach (JsStatement statement in node.Statements)
                {
                    visit(statement);
                }
            }
        }

        protected override void _visit(JsBreakStatement node)
        {
            // Do Nothing
        }

        protected override void _visit ( JsCatchClause node )
        {
            if (node != null)
            {
                // TODO: node.IdentifierName ?
                visit( node.Block );
            }
        }
        
        protected override void _visit(JsCodeExpression node)
        {
            if (node != null)
            {
                // TODO: JsCodeExpression
            }
        }

        protected override void _visit ( JsCommentStatement node )
        {
            if (node != null)
            {
                // Ignore comments
            }
        }
        
        protected override void _visit ( JsConditionalExpression node )
        {
            if (node != null)
            {
                visit( node.Condition );
                visit( node.TrueExpression );
                visit( node.FalseExpression );
            }
        }
        
        protected override void _visit ( JsContinueStatement node )
        {
            if (node != null)
            {
                // Do Nothing
            }
        }
        
        protected override void _visit ( JsDoWhileStatement node )
        {
            if (node != null)
            {
                visit( node.Condition );
                visit( node.Statement );
            }
        }

        protected override void _visit(JsExpression node)
        {
            if (node != null)
            {
                // TODO: JsExpression
            }
        }

        protected override void _visit(JsExpressionStatement node)
        {
            if (node != null)
            {
                visit(node.Expression);
            }
        }

        protected override void _visit ( JsExternalFileUnit node )
        {
            if (node != null)
            {
                // Do Nothing
            }
        }

        protected override void _visit ( JsForInStatement node )
        {
            if (node != null)
            {
                visit( node.Initializer );
                visit( node.Member );
                visit( node.Statement );
            }
        }

        protected override void _visit(JsForStatement node)
        {
            if (node != null)
            {
                visit(node.Condition);
                visit(node.Initializer);
                visit(node.Iterator);
                visit(node.Statement);
            }
        }

        protected override void _visit(JsFunction node)
        {
            if (node != null)
            {
                visit(node.Block);
            }
        }

        protected override void _visit(JsIfStatement node)
        {
            if (node != null)
            {
                visit(node.Condition);
                visit(node.ElseStatement);
                visit(node.IfStatement);
            }
        }

        protected override void _visit(JsIndexerAccessExpression node)
        {
            if (node != null)
            {
                visit(node.Member);

                foreach ( JsExpression arg in node.Arguments )
                {
                    visit( arg );
                }
            }
        }

        protected override void _visit(JsInvocationExpression node)
        {
            if (node != null)
            {
                bool found = false;
                JsExpression mExpression = node.Member;
                if ( mExpression != null && mExpression.NodeType == JsNodeType.MemberExpression )
                {
                    JsMemberExpression member = ( JsMemberExpression ) mExpression;
                    if ( member.Name == "Typeof" )
                    {
                        found = true;
                    }
                }

                // If we find a Typeof, then we need to include the args as a dependency
                if (!found)
                {
                    visit( node.Member );
                    foreach ( JsExpression arg in node.Arguments )
                    {
                        visit(arg);
                    }
                }
                else
                {
                    foreach ( JsExpression arg in node.Arguments )
                    {
                        dependencyList.Add( arg.ToJs() );
                    }
                }
            }
        }

        protected void _visit ( JsInvocationExpression node, bool include )
        {
            if ( node != null )
            {
                visit( node.Member, include );

                foreach ( JsExpression jsExp in node.Arguments )
                {
                    visit( node );
                }
            }
        }

       
        protected override void _visit ( JsJsonArrayExpression node )
        {
            if (node != null)
            {
                foreach ( JsExpression esp in node.Items )
                {
                    visit( esp );
                }
            }
        }

        protected override void _visit ( JsJsonMember node )
        {
            if (node != null)
            {
                // TODO: JsJsonMember -- node.Name?
            }
        }

        protected override void _visit ( JsJsonNameValue node )
        {
            if ( node != null )
            {
                visit( node.Name );
                visit( node.Value );
            }
        }

        protected override void _visit(JsJsonObjectExpression node)
        {
            if ( node != null && node.NamesValues != null )
            {
                foreach ( JsJsonNameValue nameValue in node.NamesValues )
                {
                    visit( nameValue );
                }
            }
        }

        protected override void _visit(JsMemberExpression node)
        {
            if (node != null)
            {
                visit( node.PreviousMember );
                
                // bschmidtke : We may need this although I don't think so. Keeping around for node.Name, just in case.

                //string jsStr = node.ToJs();
                //string regStr = @"(?:Typeof\(|new\s)([\w\.]+)(?:\(|\))";
                //MatchCollection match = Regex.Matches( node.Name, regStr, RegexOptions.None );
                //if ( match.Count > 0 )
                //{
                //    visit( node, true );
                //}
                //else
                //{
                //    // end of the line for this guy
                //}
            }
        }

        protected void _visit(JsMemberExpression node, bool include)
        {
            if (node != null)
            {
                if (include)
                {
                    dependencyList.Add(node.Name);
                }
            }
        }

        // new object expression adds the "new" work prior to defining an object.
        protected override void _visit(JsNewObjectExpression node)
        {
            if (node != null)
            {
                visit(node.Invocation, true);
            }
        }

        protected override void _visit ( JsNodeList node )
        {
            if( node != null )
            {
                foreach (JsNode jsnode in node.Nodes)
                {
                    visit( jsnode );
                }
            }
        }

        protected override void _visit(JsNullExpression node)
        {
            if (node != null)
            {
                // Do Nothing
            }
        }

        protected override void _visit ( JsNumberExpression node )
        {
            if (node != null)
            {
                // Do Nothing
            }
        }
        
        protected override void _visit(JsParenthesizedExpression node)
        {
            if (node != null)
            {
                visit(node.Expression);
            }
        }

        protected override void _visit(JsPostUnaryExpression node)
        {
            if (node != null)
            {
                visit(node.Left);
            }
        }

        protected override void _visit(JsPreUnaryExpression node)
        {
            if (node != null)
            {
                visit(node.Right);
            }
        }

        protected override void _visit ( JsRegexExpression node )
        {
            if (node != null)
            {
                // Do Nothing
            }
        }
        
        protected override void _visit(JsReturnStatement node)
        {
            if (node != null)
            {
                visit(node.Expression);
            }
        }

        protected override void _visit(JsStatement node)
        {
            if (node != null)
            {
                // TODO: JsStatement?
            }
        }

        protected override void _visit ( JsStatementExpressionList node )
        {
            if (node != null)
            {
                foreach (JsExpression expression in node.Expressions)
                {
                    visit( expression );
                }
            }
        }

        protected override void _visit(JsStringExpression node)
        {
            if (node != null)
            {
                // TODO: JsStringExpression?
            }
        }

        protected override void _visit ( JsSwitchLabel node )
        {
            if (node != null)
            {
                visit( node.Expression );
            }
        }
        
        protected override void _visit ( JsSwitchSection node )
        {
            if (node != null)
            {
                foreach ( JsSwitchLabel label in node.Labels)
                {
                    visit( label );
                }

                foreach ( JsStatement statement in node.Statements )
                {
                    visit( statement );
                }
            }
        }
        
        protected override void _visit ( JsSwitchStatement node )
        {
            if (node != null)
            {
                visit( node.Expression );
                foreach ( JsSwitchSection section in node.Sections )
                {
                    visit( section );
                }
            }
        }

        protected override void _visit(JsThis node)
        {
            if (node != null)
            {
                // TODO: JsThis?
            }
        }

        protected override void _visit(JsThrowStatement node)
        {
            if (node != null)
            {
                visit(node.Expression);
            }
        }
        
        protected override void _visit ( JsTryStatement node )
        {
            if (node != null)
            {
                visit( node.TryBlock );
                visit( node.CatchClause );
                visit( node.FinallyBlock );
            }
        }

        protected override void _visit ( JsUnit node )
        {
            if (node != null)
            {
                foreach (JsStatement statement in node.Statements)
                {
                    visit( statement );
                }
            }
        }

        protected override void _visit(JsVariableDeclarationExpression node)
        {
            if (node != null)
            {
                foreach (JsVariableDeclarator decNode in node.Declarators)
                {
                    visit(decNode);
                }
            }
        }

        protected override void _visit(JsVariableDeclarationStatement node)
        {
            if (node != null)
            {
                visit(node.Declaration);
            }
        }

        protected override void _visit(JsVariableDeclarator node)
        {
            if (node != null)
            {
                visit(node.Initializer);
            }
        }

        
        protected override void _visit ( JsWhileStatement node )
        {
            if (node != null)
            {
                visit( node.Condition );
                visit( node.Statement );
            }
        }

        protected override void _visit ( JsUseStrictStatement node )
        {
            if (node != null)
            {
                // Do Nothing
            }
        }

        protected void visit(JsNode node, bool include)
        {
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case JsNodeType.InvocationExpression:
                        _visit( ( JsInvocationExpression ) node, include );
                        break;

                    case JsNodeType.MemberExpression:
                        _visit((JsMemberExpression) node, include);
                        break;

                    default:
                        visit(node);
                        break;
                }
            }
        }
    }
}
