using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nova.CodeAnalysis.Lowering;
using Nova.CodeAnalysis.Symbols;
using Nova.CodeAnalysis.Syntax;
using Nova.CodeAnalysis.Text;

namespace Nova.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag diagnostics = new DiagnosticBag();
        private readonly FunctionSymbol function;
        private BoundScope scope;

        public Binder(BoundScope parent, FunctionSymbol function)
        {
            scope = new BoundScope(parent);
            this.function = function;

            if (function != null)
            {
                foreach (ParameterSymbol p in function.Parameters)
                    scope.TryDeclareVariable(p);
            }
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            BoundScope parentScope = CreateParentScope(previous);
            Binder binder = new Binder(parentScope, function: null);

            foreach (FunctionDeclarationSyntax function in syntax.Members.OfType<FunctionDeclarationSyntax>())
                binder.BindFunctionDeclaration(function);

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            
            foreach (GlobalStatementSyntax globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
            {
                BoundStatement s = binder.BindStatement(globalStatement.Statement);
                statements.Add(s);
            }

            ImmutableArray<FunctionSymbol> functions = binder.scope.GetDeclaredFunctions();
            ImmutableArray<VariableSymbol> variables = binder.scope.GetDeclaredVariables();
            ImmutableArray<Diagnostic> diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, functions, variables, statements.ToImmutable());
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            BoundScope parentScope = CreateParentScope(globalScope);
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            BoundGlobalScope scope = globalScope;

            while (scope != null)
            {
                foreach (FunctionSymbol function in scope.Functions)
                {
                    Binder binder = new Binder(parentScope, function);
                    BoundStatement body = binder.BindStatement(function.Declaration.Body);
                    BoundBlockStatement loweredBody = Lowerer.Lower(body);
                    functionBodies.Add(function, loweredBody);
                    diagnostics.AddRange(binder.Diagnostics);
                }

                scope = scope.Previous;
            }

            BoundBlockStatement statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));
            return new BoundProgram(diagnostics.ToImmutable(), functionBodies.ToImmutable(), statement);
        }

        private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            HashSet<string> seenParameterNames = new HashSet<string>();

            foreach (ParameterSyntax parameterSyntax in syntax.Parameters)
            {
                string parameterName = parameterSyntax.Identifier.Text;
                TypeSymbol parameterType = BindTypeClause(parameterSyntax.Type);

                if (!seenParameterNames.Add(parameterName))
                {
                    diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Span, parameterName);
                }
                else
                {
                    ParameterSymbol parameter = new ParameterSymbol(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            TypeSymbol type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;

            if (type != TypeSymbol.Void)
                diagnostics.XXX_ReportFunctionsAreUnsupported(syntax.Type.Span);

            FunctionSymbol function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);

            if (!scope.TryDeclareFunction(function))
                diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, function.Name);

        }

        public static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            Stack<BoundGlobalScope> stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }
            BoundScope parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                BoundScope scope = new BoundScope(parent);

                foreach (FunctionSymbol f in previous.Functions)
                    scope.TryDeclareFunction(f);

                foreach (VariableSymbol v in previous.Variables)
                    scope.TryDeclareVariable(v);

                parent = scope;
            }
            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            BoundScope result = new BoundScope(null);

            foreach (FunctionSymbol f in BuiltinFunctions.GetALL())
                result.TryDeclareFunction(f);

            return result;
        }

        public DiagnosticBag Diagnostics => diagnostics;

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockStatementSyntax)syntax);
                case SyntaxKind.VariableDeclaration:
                    return BindVariableDeclaration((VariableDeclarationSyntax)syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax)syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax)syntax);
                case SyntaxKind.DoWhileStatement:
                    return BindDoWhileStatement((DoWhileStatementSyntax)syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax)syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);

            foreach (StatementSyntax statementSyntax in syntax.Statements)
            {
                BoundStatement statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            scope = scope.Parent;
            
            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            bool isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            TypeSymbol type = BindTypeClause(syntax.TypeClause);
            BoundExpression initializer = BindExpression(syntax.Initializer);
            TypeSymbol variableType = type ?? initializer.Type;
            VariableSymbol variable = BindVariable(syntax.Identifier, isReadOnly, variableType);
            BoundExpression convertedInitializer = BindConversion(syntax.Initializer.Span, initializer, variableType);
            
            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax == null)
                return null;
            
            TypeSymbol type = LookupType(syntax.Identifier.Text);
            if (type == null)
                diagnostics.ReportUndefinedType(syntax.Identifier.Span, syntax.Identifier.Text);

            return type;
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement thenStatement = BindStatement(syntax.ThenStatement);
            BoundStatement elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement body = BindStatement(syntax.Body);
            return new BoundDoWhileStatement(condition, body);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            TypeSymbol intType = TypeSymbol.Int;
            BoundExpression lowerBound = BindExpression(syntax.LowerBound, intType);
            BoundExpression upperBound = BindExpression(syntax.UpperBound, intType);

            scope = new BoundScope(scope);

            VariableSymbol variable = BindVariable(syntax.Identifier, isReadOnly: true, intType);
            BoundStatement body = BindStatement(syntax.Body);

            scope = scope.Parent;
            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            BoundExpression expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            BoundExpression result = BindExpressionInternal(syntax);

            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;

            if (syntax.IdentifierToken.IsMissing)
            {
                return new BoundErrorExpression();
            }

            if (!scope.TryLookupVariable(name, out var variable))
            {
                diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();                
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;
            BoundExpression boundExpression = BindExpression(syntax.Expression);

            if (!scope.TryLookupVariable(name, out var variable))
            {
                diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
                diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

            BoundExpression convertedExpression = BindConversion(syntax.Expression.Span, boundExpression, variable.Type);

            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            
            if (boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperator == null)
            {
                diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();                
            }

            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);

            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            
            if (boundOperator == null)
            {
                diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
                return BindConversion(syntax.Arguments[0], type, allowExplicit: true);

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (ExpressionSyntax argument in syntax.Arguments)
            {
                BoundExpression boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            if (!scope.TryLookupFunction(syntax.Identifier.Text, out FunctionSymbol function))
            {
                diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                BoundExpression argument = boundArguments[i];
                ParameterSymbol parameter = function.Parameters[i];

                if (argument.Type != parameter.Type)
                {
                    diagnostics.ReportWrongArgumentType(syntax.Arguments[i].Span, parameter.Name, parameter.Type, argument.Type);
                    return new BoundErrorExpression();
                }
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            BoundExpression expression = BindExpression(syntax);
            return BindConversion(syntax.Span, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            Conversion conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                    diagnostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);

                return new BoundErrorExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
                diagnostics.ReportCannotConvertImplicitly(diagnosticSpan, expression.Type, type);

            if (conversion.IsIdentity)
                return expression;

            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            string name = identifier.Text ?? "?";
            bool declare = !identifier.IsMissing;
            VariableSymbol variable = function == null
                            ? (VariableSymbol) new GlobalVariableSymbol(name, isReadOnly, type)
                            : new LocalVariableSymbol(name, isReadOnly, type);

            if (declare && !scope.TryDeclareVariable(variable))
                diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);

            return variable;
        }

        private TypeSymbol LookupType(string name)
        {
            switch (name)
            {
                case "bool":
                    return TypeSymbol.Bool;
                case "int":
                    return TypeSymbol.Int;
                case "string":
                    return TypeSymbol.String;
                default:
                    return null;
            }
        }
    }
}