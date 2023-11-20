using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

namespace Xunit.DependencyInjection.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(XunitDependencyInjectionCodeFixProvider)), Shared]
public class XunitDependencyInjectionCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.CreateRange(Rules.SupportedDiagnostics.Select(d => d.Id));

    // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics[0];
        if (diagnostic.Id != Rules.ReturnTypeAssignableTo.Id &&
            diagnostic.Id != Rules.NoReturnType.Id) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

        if (root?.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is not MethodDeclarationSyntax method) return;

        if (diagnostic.Id == Rules.NoReturnType.Id)
            context.RegisterCodeFix(CodeAction.Create(
                    CodeFixResources.ModifyReturnType,
                    c => ChangeReturnType(context.Document, method, SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.VoidKeyword, "void", "void", SyntaxTriviaList.Create(SyntaxFactory.Whitespace(" ")))), c),
                    nameof(CodeFixResources.ModifyReturnType)),
                diagnostic);
        else if (diagnostic.Id == Rules.ReturnTypeAssignableTo.Id)
            if (method.Identifier.Text == "CreateHostBuilder")
                context.RegisterCodeFix(CodeAction.Create(
                        CodeFixResources.ModifyReturnType,
                        c => ChangeReturnType(
                            context.Document, method, SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxFactory.QualifiedName(
                                        GetIdentifierName("Microsoft"),
                                        SyntaxFactory.Token(SyntaxKind.DotToken),
                                        GetIdentifierName("Extensions")),
                                    SyntaxFactory.Token(SyntaxKind.DotToken),
                                    GetIdentifierName("Hosting")),
                                SyntaxFactory.Token(SyntaxKind.DotToken),
                                GetIdentifierName("IHostBuilder")), c),
                        nameof(CodeFixResources.ModifyReturnType)),
                    diagnostic);
    }

    private static IdentifierNameSyntax GetIdentifierName(string text) =>
        SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(text));

    private static async Task<Document> ChangeReturnType(Document document, MethodDeclarationSyntax node, TypeSyntax returnType, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();

        return document.WithSyntaxRoot(root.ReplaceNode(node, node.WithReturnType(returnType)));
    }
}
