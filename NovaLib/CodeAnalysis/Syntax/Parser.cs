using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Nova.CodeAnalysis.Text;

namespace Nova.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly SourceText text;
        private readonly ImmutableArray<SyntaxToken> tokens;
        private readonly DiagnosticBag diagnostics = new DiagnosticBag();
        private int position;

        public Parser(SourceText text)
        {
            var tokens = new List<SyntaxToken>();

            Lexer lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.WhiteSpaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            this.text = text;
            this.tokens = tokens.ToImmutableArray();
            this.diagnostics.AddRange(lexer.Diagnostics);
        }

        public DiagnosticBag Diagnostics => diagnostics;

        private SyntaxToken Peek(int offset)
        {
            int index = position + offset;
            if (index >= tokens.Length)
                return tokens[tokens.Length - 1];
            
            return tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            StatementSyntax statement = ParseStatement();
            SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(statement, endOfFileToken);
        }

        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.LetKeyword:
                case SyntaxKind.VarKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.DoKeyword:
                    return ParseDoWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            var openBranceToken = MatchToken(SyntaxKind.OpenBraceToken);

            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                SyntaxToken startToken = Current;

                StatementSyntax statement = ParseStatement();
                statements.Add(statement);

                if (Current == startToken)
                    NextToken();
            }

            var closeBranceToken = MatchToken(SyntaxKind.CloseBraceToken);

            return new BlockStatementSyntax(openBranceToken, statements.ToImmutable(), closeBranceToken);
        }

        private StatementSyntax ParseVariableDeclaration()
        {
            SyntaxKind expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            SyntaxToken keyword = MatchToken(expected);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            TypeClauseSyntax typeClause = ParseOptionalTypeClause();
            SyntaxToken equals = MatchToken(SyntaxKind.EqualsToken);
            ExpressionSyntax initializer = ParseExpression();
            return new VariableDeclarationSyntax(keyword, identifier, typeClause, equals, initializer);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (Current.Kind != SyntaxKind.ColonToken)
                return null;

            return ParseTypeClause();
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            SyntaxToken colonToken = MatchToken(SyntaxKind.ColonToken);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            return new TypeClauseSyntax(colonToken, identifier);
        }

        private StatementSyntax ParseIfStatement()
        {
            SyntaxToken keyword = MatchToken(SyntaxKind.IfKeyword);
            ExpressionSyntax condition = ParseExpression();
            StatementSyntax statement = ParseStatement();
            ElseClauseSyntax elseClause = ParseElseClause();
            return new IfStatementSyntax(keyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;
            
            SyntaxToken keyword = NextToken();
            StatementSyntax statement = ParseStatement();
            return new ElseClauseSyntax(keyword, statement);
        }

        private StatementSyntax ParseWhileStatement()
        {
            SyntaxToken keyword = MatchToken(SyntaxKind.WhileKeyword);
            ExpressionSyntax condition = ParseExpression();
            StatementSyntax body = ParseStatement();
            return new WhileStatementSyntax(keyword, condition, body);
        }

        private StatementSyntax ParseDoWhileStatement()
        {
            SyntaxToken doKeyword = MatchToken(SyntaxKind.DoKeyword);
            StatementSyntax body = ParseStatement();
            SyntaxToken whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
            ExpressionSyntax condition = ParseExpression();
            return new DoWhileStatementSyntax(doKeyword, body, whileKeyword, condition);
        }

        private StatementSyntax ParseForStatement()
        {
            SyntaxToken keyword = MatchToken(SyntaxKind.ForKeyword);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken equalsToken = MatchToken(SyntaxKind.EqualsToken);
            ExpressionSyntax lowerBound = ParseExpression();
            SyntaxToken toKeyword = MatchToken(SyntaxKind.ToKeyword);
            ExpressionSyntax upperBound = ParseExpression();
            StatementSyntax body = ParseStatement();
            return new ForStatementSyntax(keyword, identifier, equalsToken, lowerBound, toKeyword, upperBound, body);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            ExpressionSyntax expression = ParseExpression();
            return new ExpressionStatementSyntax(expression);
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                SyntaxToken identifierToken = NextToken();
                SyntaxToken operatorToken = NextToken();
                ExpressionSyntax right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }
            return ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;
                
                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesizedExpression();

                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                    return ParseBooleanLiteral();

                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();
                
                case SyntaxKind.StringToken:
                    return ParseStringLiteral();

                case SyntaxKind.IdentifierToken:                
                default:
                    return ParseNameOrCallExpression();
            }

        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            SyntaxToken left = MatchToken(SyntaxKind.OpenParenthesisToken);
            ExpressionSyntax expression = ParseExpression();
            SyntaxToken right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            bool isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            SyntaxToken keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(keywordToken, isTrue);
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            SyntaxToken numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }

        private ExpressionSyntax ParseStringLiteral()
        {
            SyntaxToken stringToken = MatchToken(SyntaxKind.StringToken);
            return new LiteralExpressionSyntax(stringToken);
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
                return ParseCallExpression();
            
            return ParseNameExpression();
        }

        private ExpressionSyntax ParseCallExpression()
        {
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            SeparatedSyntaxList<ExpressionSyntax> arguments = ParseArguments();
            SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);

            return new CallExpressionSyntax(identifier, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                ExpressionSyntax expression = ParseExpression();
                nodesAndSeparators.Add(expression);

                if (Current.Kind != SyntaxKind.CloseParenthesisToken)
                {
                    SyntaxToken comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
            }

            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(identifierToken);
        }

    }
}