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
using SharpKit.Compiler;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.Semantics;
using randori.compiler.builders;
using randori.compiler.constants;
using randori.compiler.utils;

namespace randori.compiler
{

    public class RandoriCompilerPlugin : ICompilerPlugin
    {
        
        #region SharpKit Event Sequence/Order

            // 1) event Action BeforeParseCs;
            // 2) event Action AfterParseCs;
            // 3) event Action BeforeApplyExternalMetadata;
            // 4) event Action AfterApplyExternalMetadata;
            // 5) event Action BeforeConvertCsToJs;
            // 6) event Action<IEntity> BeforeConvertCsToJsEntity;
            // 7) event Action<IEntity, JsNode> AfterConvertCsToJsEntity;
            // 8) event Action AfterConvertCsToJs;
            // 9) event Action BeforeMergeJsFiles;
            // 10) event Action AfterMergeJsFiles;
            // 11) event Action BeforeInjectJsCode;
            // 12) event Action AfterInjectJsCode;
            // 13) event Action BeforeOptimizeJsFiles;
            // 14) event Action AfterOptimizeJsFiles;
            // 15) event Action BeforeSaveJsFiles;
            // 16) event Action AfterSaveJsFiles;
            // 17) event Action BeforeEmbedResources;
            // 18) event Action AfterEmbedResources;
            // 19) event Action BeforeSaveNewManifest;
            // 20) event Action AfterSaveNewManifest;
            // 21) event Action BeforeExit;

        #endregion

        public ICompiler compiler
        {
            get;
            set;
        }

        public void Init(ICompiler compiler)
        {
            this.compiler = compiler;

            this.compiler.BeforeConvertCsToJsEntity += Compiler_BeforeConvertCsToJsEntity;
            // Our main Ast Gen...
            this.compiler.AfterConvertCsToJsEntity += Compiler_AfterConvertCsToJsEntity;
            this.compiler.AfterMergeJsFiles += Compiler_AfterMergeJsFiles;
            this.compiler.BeforeSaveJsFiles += Compiler_BeforeSaveJsFiles;

            #region Unused SharpKit Compiler Events
            //Compiler.BeforeParseCs += new Action(Compiler_BeforeParseCs);
            //Compiler.AfterParseCs += new Action(Compiler_AfterParseCs);
            //Compiler.BeforeApplyExternalMetadata += new Action(Compiler_BeforeApplyExternalMetadata);
            //Compiler.AfterApplyExternalMetadata += new Action(Compiler_AfterApplyExternalMetadata);
            //Compiler.BeforeConvertCsToJs += new Action(Compiler_BeforeConvertCsToJs);
            //Compiler.AfterConvertCsToJs += new Action(Compiler_AfterConvertCsToJs);
            //Compiler.BeforeMergeJsFiles += new Action(Compiler_BeforeMergeJsFiles);
            //Compiler.BeforeInjectJsCode += new Action(Compiler_BeforeInjectJsCode);
            //Compiler.AfterInjectJsCode += new Action(Compiler_AfterInjectJsCode);
            //Compiler.BeforeOptimizeJsFiles += new Action(Compiler_BeforeOptimizeJsFiles);
            //Compiler.AfterOptimizeJsFiles += new Action(Compiler_AfterOptimizeJsFiles);
            //Compiler.AfterSaveJsFiles += new Action(Compiler_AfterSaveJsFiles);
            //Compiler.BeforeEmbedResources += new Action(Compiler_BeforeEmbedResources);
            //Compiler.AfterEmbedResources += new Action(Compiler_AfterEmbedResources);
            //Compiler.BeforeSaveNewManifest += new Action(Compiler_BeforeSaveNewManifest);
            //Compiler.AfterSaveNewManifest += new Action(Compiler_AfterSaveNewManifest);
            //Compiler.BeforeExit += new Action(Compiler_BeforeExit);
            #endregion
        }

        void Compiler_BeforeConvertCsToJsEntity(IEntity arg1)
        {
            if (arg1 == null)
            {
                throw new Exception("Incorrect parameters sent from SharpKit.");
            }

            foreach (IAttribute classAttr in arg1.Attributes)
            {
                foreach (ResolveResult posAttr in classAttr.PositionalArguments)
                {
                    if (posAttr is MemberResolveResult)
                    {
                        MemberResolveResult mrr = (MemberResolveResult)posAttr;

                        if (mrr != null && mrr.Member.MemberDefinition.FullName == "SharpKit.JavaScript.JsMode.Global")
                        {
                            return;
                        }
                    }
                }
            }

            switch (arg1.SymbolKind)
            {
                case SymbolKind.TypeDefinition:
                    // TODO: 04/05/2013 - The [Observable] tag is incomplete, commenting out until functionality can be completed
                    // Console.WriteLine(">> Compiler_BeforeConvertCsToJsEntity ( " + arg1.FullName + " )");
                    //DefaultResolvedTypeDefinition cSharpDef = (DefaultResolvedTypeDefinition) arg1;
                    //ObservablePropertiesBuilder bpb = new ObservablePropertiesBuilder(cSharpDef);
                    //List<JsExpressionStatement> propertyStatements = bpb.getEncapsulateBindableProperties();
                    break;
            }
        }

        void Compiler_BeforeSaveJsFiles()
        {
            insertGeneratedByComments();
        }

        void Compiler_AfterMergeJsFiles ()
        {
            //Console.WriteLine(">> Compiler_AfterMergeJsFiles");
            processHtmlMergedFiles();
        }

        void Compiler_AfterConvertCsToJsEntity ( IEntity arg1, JsNode arg2 )
        {
            if ( arg1 == null || arg2 == null )
            {
                throw new Exception( "Incorrect parameters sent from SharpKit." );
            }

            foreach ( IAttribute classAttr in arg1.Attributes )
            {
                foreach ( ResolveResult posAttr in classAttr.PositionalArguments )
                {
                    if ( posAttr is MemberResolveResult )
                    {
                        MemberResolveResult mrr = ( MemberResolveResult ) posAttr;

                        // Omit global classes.
                        if ( mrr.Member.MemberDefinition.FullName == "SharpKit.JavaScript.JsMode.Global" )
                        {
                            return;
                        }
                    }
                }
            }

            switch ( arg1.SymbolKind )
            {
                case SymbolKind.TypeDefinition:

                    DefaultResolvedTypeDefinition cSharpDef = ( DefaultResolvedTypeDefinition ) arg1;
                    JsUnit jsDef = ( JsUnit ) arg2;

                    insertClassNameProperty( cSharpDef, jsDef );

                    insertDependencyList( cSharpDef, jsDef );
                    
                    insertBindableMetadata( cSharpDef, jsDef );

                    insertInjectionPoints( cSharpDef, jsDef );
                    break;
                default:
                    //
                    break;
            }
        }

        // inserts static className property in JSFile
        // ex: randori.attributes.HtmlMergedFile.className = "randori.attributes.HtmlMergedFile";
        protected void insertClassNameProperty ( DefaultResolvedTypeDefinition cSharpDef, JsUnit jsDef )
        {
            if ( cSharpDef != null && jsDef != null )
            {
                JsExpressionStatement classNameStatement = AstUtils.getStaticPropertyStatement( OutputNameConstants.PROPERTY_CLASS_NAME, cSharpDef.FullName );
                if ( classNameStatement != null )
                {
                    jsDef.Statements.Add( classNameStatement );
                }
            }
        }

        // inserts a list of other dependencies this class needs in order to run. Guice will load
        protected void insertDependencyList ( DefaultResolvedTypeDefinition cSharpDef, JsUnit jsDef )
        {
            if ( cSharpDef != null && jsDef != null )
            {
                ClassDependencyFunctionBuilder dependencyBuilder = new ClassDependencyFunctionBuilder( cSharpDef, jsDef );
                JsExpressionStatement classDependencyStatement = dependencyBuilder.getDependencyExpression();
                if (classDependencyStatement != null)
                {
                    jsDef.Statements.Add(classDependencyStatement);
                }
            }
        }

        // processes [Bindable] tagged items
        protected void insertBindableMetadata(DefaultResolvedTypeDefinition cSharpDef, JsUnit jsDef)
        {
            if (cSharpDef != null && jsDef != null)
            {
                ObservablePropertiesBuilder bpb = new ObservablePropertiesBuilder(cSharpDef);
                List<JsExpressionStatement> propertyStatements = bpb.getEncapsulateBindableProperties();
                if (propertyStatements != null)
                {
                    foreach (JsExpressionStatement propertyStatement in propertyStatements)
                    {
                        jsDef.Statements.Add(propertyStatement);
                    }
                }
            }
        }

        // creates injectionPoints function
        // ex: randori.formatters.AbstractFormatter.injectionPoints = function(t) { ... }
        protected void insertInjectionPoints ( DefaultResolvedTypeDefinition cSharpDef, JsUnit jsDef )
        {
            if (cSharpDef != null && jsDef != null)
            {
                InjectionPointsFunctionBuilder ipBuilder = new InjectionPointsFunctionBuilder(cSharpDef, jsDef);
                JsExpressionStatement injectionPoints = ipBuilder.getInjectionPointsExpression();
                if (injectionPoints != null)
                {
                    jsDef.Statements.Add(injectionPoints);
                }
            }
        }

        // Iterates over all AST nodes ready to have a JavaScript file generated and insert
        // a generated by tag into the beginning.
        protected void insertGeneratedByComments ()
        {
            JsCommentStatement assemblyComment = AstUtils.getCommentStatement( "Generated by Randori v" + AssemblyUtils.assemblyVersion );

            foreach ( SkJsFile skFile in compiler.SkJsFiles )
            {
                JsFile file = skFile.JsFile;

                if ( file.Units != null && file.Units.Count > 0 )
                {
                    JsUnit unit = ( JsUnit ) file.Units[0];
                    unit.Statements.Insert( 1, assemblyComment );
                }
            }
        }

        public void processHtmlMergedFiles ()
        {
            if ( compiler.CsCompilation.MainAssembly != null )
            {
                IAssembly projAssembly = compiler.CsCompilation.MainAssembly;
                if ( projAssembly != null )
                {
                    // Search the AssemblyInfo for  
                    foreach ( IAttribute attribute in projAssembly.AssemblyAttributes )
                    {
                        if ( attribute.AttributeType.ReflectionName == RandoriClassNames.metadataHtmlMergedFile )
                        {
                            List<string> fileList = new List<string>();
                            string fileName = null;

                            foreach ( KeyValuePair<IMember, ResolveResult> arg in attribute.NamedArguments )
                            {
                                if ( arg.Key.Name == "Filename" )
                                {
                                    fileName = arg.Value.ConstantValue.ToString();
                                }

                                if ( arg.Key.Name == "Sources" )
                                {
                                    ArrayCreateResolveResult resolveResult = ( ArrayCreateResolveResult ) arg.Value;
                                    foreach ( ConversionResolveResult crr in resolveResult.InitializerElements )
                                    {
                                        fileList.Add( crr.Input.ConstantValue.ToString() );
                                    }
                                }
                            }

                            if ( fileName != null && fileList.Count > 0 )
                            {
                                insertHtmlCachedFile( fileName, fileList );
                            }
                        }
                    }
                }
            }
        }

        public void insertHtmlCachedFile ( string fileName, List<string> fileList )
        {
            if ( fileName != null && fileList.Count > 0 )
            {
                Console.WriteLine(">> Generating HTML Merged File Cache ( " + fileName + " )");

                HtmlCacheFileBuilder htmlCFBuilder = new HtmlCacheFileBuilder(fileName, fileList);
                SkJsFile nFile = htmlCFBuilder.buildHtmlCacheFile();

                if (nFile != null)
                {
                    addJsFile(nFile);
                }
            }
        }

        // Adds a generated SharpKit JsFile to the list of files to generate
        // For when SharpKit writes the JS files to the file system
        public void addJsFile ( SkJsFile file )
        {
            compiler.SkJsFiles.Add( file );
        }

        #region Unused SharpKit Compiler Event Handlers
        void Compiler_BeforeParseCs ()
        {
            //Console.WriteLine(">> Compiler_BeforeParseCs");
        }

        void Compiler_BeforeConvertCsToJs()
        {
            //Console.WriteLine(">> Compiler_BeforeConvertCsToJs");
        }

        void Compiler_BeforeApplyExternalMetadata()
        {
            //Console.WriteLine(">> Compiler_BeforeApplyExternalMetadata");
        }

        void Compiler_AfterApplyExternalMetadata()
        {
            //Console.WriteLine(">> Compiler_AfterApplyExternalMetadata");
        }

        void Compiler_AfterSaveNewManifest()
        {
            //Console.WriteLine(">> Compiler_AfterSaveNewManifest");
        }

        void Compiler_AfterSaveJsFiles()
        {
            //Console.WriteLine(">> Compiler_AfterSaveJsFiles");
        }

        void Compiler_AfterOptimizeJsFiles()
        {
            //Console.WriteLine(">> Compiler_AfterOptimizeJsFiles");
        }

        void Compiler_AfterInjectJsCode()
        {
            //Console.WriteLine(">> Compiler_AfterInjectJsCode");
        }

        void Compiler_BeforeSaveNewManifest()
        {
            //Console.WriteLine(">> Compiler_BeforeSaveNewManifest");
        }

        void Compiler_BeforeOptimizeJsFiles()
        {
            //Console.WriteLine(">> Compiler_BeforeOptimizeJsFiles");
        }

        void Compiler_BeforeMergeJsFiles()
        {
            //Console.WriteLine(">> Compiler_BeforeMergeJsFiles");
        }

        void Compiler_BeforeExit()
        {
            //Console.WriteLine(">> Compiler_BeforeExit");
        }

        void Compiler_BeforeInjectJsCode()
        {
            //Console.WriteLine(">> Compiler_BeforeInjectJsCode");
        }

        void Compiler_BeforeEmbedResources()
        {
            //Console.WriteLine(">> Compiler_BeforeEmbedResources");
        }

        void Compiler_AfterEmbedResources()
        {
            //Console.WriteLine(">> Compiler_AfterEmbedResources");
        }

        void Compiler_AfterConvertCsToJs()
        {
            // assembly hasn't been added yet.
            //Console.WriteLine(">> Compiler_AfterConvertCsToJs");
        }
        
        void Compiler_AfterParseCs()
        {
            //Console.WriteLine(">> Compiler_AfterParseCs");
        }
        #endregion
    }
}