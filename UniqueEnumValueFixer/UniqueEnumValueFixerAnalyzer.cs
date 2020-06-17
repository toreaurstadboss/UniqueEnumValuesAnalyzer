using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace UniqueEnumValueFixer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UniqueEnumValueFixerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UniqueEnumValueFixer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);

        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            try
            {
                var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
                if (namedTypeSymbol.EnumUnderlyingType != null)
                {
                    var valueListForEnum = new List<Tuple<string, int>>();
                    //Debugger.Launch();
                    //Debugger.Break();
                    var typeResolved = context.Compilation.GetTypeByMetadataName(namedTypeSymbol.MetadataName) ?? context.Compilation.GetTypeByMetadataName(namedTypeSymbol.ToString());
                    if (typeResolved != null)
                    {
                        foreach (var member in typeResolved.GetMembers())
                        {
                            var c = member.GetType().GetRuntimeProperty("ConstantValue");
                            if (c == null)
                            {
                                c = member.GetType().GetRuntimeProperties().FirstOrDefault(prop =>
                                    prop != null && prop.Name != null &&
                                    prop.Name.Contains("IFieldSymbol.ConstantValue"));
                                if (c == null)
                                {
                                    continue;
                                }
                            }

                            var v = c.GetValue(member) as int?;
                            if (v.HasValue)
                            {
                                valueListForEnum.Add(new Tuple<string, int>(member.Name, v.Value));
                            }
                        }
                        if (valueListForEnum.GroupBy(v => v.Item2).Any(g => g.Count() > 1))
                        {
                            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0],
                                namedTypeSymbol.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
            catch (Exception err)
            {


                Console.WriteLine(err);
            }

        }


    }
}
