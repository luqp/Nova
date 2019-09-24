# Nova
# Compiler part 1
## Tokens
[Primera parte :](https://github.com/luqp/Nova/blob/6f8c37eb1deea898529eb5477eb4bedaf0c27a0a/nova/Program.cs)

Esta en esta primera parte detectaremos las palabras (tokens) que tenemos en el texto de entrada.
### 1.0
Crear la clase `Lexer`, esta es la encargada de crear o producir las palabras (tokens).
### 1.1
Crear la clase `SyntaxToken`, representa un Token en nuestro lenguaje, esta clase conoce el tipo, la posicion del token y su valor.

Crear `SyntaxKind enum`, que almacena todos los tipos de palabras de nuestro lenguaje.

### 1.2
Clase `lexer`, agregar un nuevo metodo `NextToken`, este buscara la siguiente palabra y la retornara, de acuerdo a la posicion actual. 
Aqui se define los tokens que queremos encontrar, `ejem: simbolos, numeros, espacios en blanco, etc`.

### 1.3
Clase `lexer`, crear la propiedad `Current`:
para hacer seguimiento al character actual del texto de entrada

## Sentencias
Segunda parte 1:

Basicamente leeremos todo el texto de entrada, y guardaremos todos los tokens reconociendo sentencias.

### 2.0
Crear la clase `Parser`, es el encargado de crear sentencias con los tokens, como si fueran notas sintacticas.
  * Esta clase tambien agrega a la sentencia un ultimo token, que determina el final del texto.

### 2.1
Clase `Parser`, creamos el metodo `Peek`: Nos ayuda a seleccionar un token, o retornar el ultimo si ya no hay tokens en la lista.

Clase `Parser`, creamos la propiedad `Current`: Usa el metodo peek, para seleccionar el token actual.

Se necesita la creacion de nodos para continuar...

## Nodos
[Tercera parte:](https://github.com/luqp/Nova/pull/2/commits/f224cd3ba810b531eb1f89fe2694bd0ebbfd3472)

Empezamos con el diseño del arbol que nos ayudara a interpretar o resolver el texto de entrada.
```
Texto de entrada: 1 + 2 * 3
~~~~~~~~~~~~~~~~~~~~~~~~~~~
Arbol:

    +
   / \
  1   *
     / \
    2   3
```

### 3.0

Crear la clase abstracta `SyntaxNode`, que es la base de todos nodos en esta aplicacion, la clase `ExpresionSyntax` deribara de este:
* Ahora podremos crear los distintos tipos de expresiones en los nodos, ejm: NumberSyntax, BinarySyntax, etc.

Procedimiento para agregar un tipo de expresion (Siguiendo los ejemplos se puede agregar cualquier tipo de ExpresionSyntax en el futuro follow the link). Ejemplos:

* 3.1
        Crear la clase `NumberExpresionSyntax`, que define los tokens de numeros del texto, cada objeto es un Numbertoken.

* 3.2
        Crear la clase `BinaryExpresionSyntax`, representa una operacion, cada objeto tiene una expresion a la izquierda, un operador y una expresion a la derecha.
## Sentencias en los Nodos
[Segunda parte 2:](https://github.com/luqp/Nova/pull/2/commits/f224cd3ba810b531eb1f89fe2694bd0ebbfd3472)

Continuacion de la segunda parte...

Se define la logica para unir e interpretar los nodos segun el orden de operacion de realizan.

### 2.2
Clase `Parser`, crear el metodo `Match`, para comprobar el correcto tipo del token.

Clase `Parser`, crear los metodos `Parse` y `ParsePrimaryExpression`, para analizar la expresion en un nodo, y descomponerla si es necesario.

## Imprimir
[Cuarta parte:](https://github.com/luqp/Nova/pull/2/commits/f224cd3ba810b531eb1f89fe2694bd0ebbfd3472):

### 4.1
En la clase `Program`, crear el metodo estatido `PrettyPrint`, esta parte es solo para mostrar la estructura de los nodos en forma de arbol que el programa esta siguiendo.

## Errores
[Quinta parte :](https://github.com/luqp/Nova/pull/2/commits/5892ef4f443285cdc751d43c9e2c0cd48e9498d5)

Implementa el diagnostico de errores, Identifica los tokens malos.

### 5.1
Crear la clase `SyntaxTree`, esta sera un tipo que representa todas las entradas una forma de limpiar el codigo.

Con esto se modificando el metodo `Parse`, en la clase `Parser`.
Ahora el metodo `Parse` revisa que la expresion se haya terminado de leer.

## Evaluacion
[Sexta parte 1:](https://github.com/luqp/Nova/pull/2/commits/b042a7bbad726568a54033aa5ad5a75b4690fe0e)

Evalua las expresiones en cada nodo del arbol

### 6.0
Create Evaluator class.

### 6.1

Clase `Evaluator`, crear los metodos `Evaluate`y `EvaluateExpression`, que trabajan con los tipos de expresiones de nuestra aplicacion, ejm: BinaryExpressionSyntax, NumberExpressionSyntax, etc.

### Triki tip [Sexta parte 2:](https://github.com/luqp/Nova/pull/2/commits/70b52840847954c372f29c274e0db88d47b078ac)

* Corrige el error, para evaluar los operadores `*, /` con mayor prioridad que los operadores `+, -`
### 6.2
Dividimos `ParseExpression` method en `PaseFactor` y `ParserTerme`.

## Parentesis
[Septima parte :](https://github.com/luqp/Nova/pull/2/commits/05e5adc85f8160f4c5f39194f2f9599c6a82003a)

Add Parenthesis expresion.

### 7.1
Crear la clase `ParenthesizedExpressionSyntax`.

### 7.2
Modificar el metodo `ParsePrimayExpression`, en la clase `Parser` y Agregar el metodo `ParseExpresion`.

### 7.3
Modificar el metodo `EvaluateExpression`


# Compiler part 2:

After clean the code:
## 1.1 Operators precendence
`Parser` class, add the `GetBinaryOperatorPrecendence` method, to defice the priority of operatores. This method simplifies the addition of other priority operators.

## 2.0 Unary Operators
```
examples:
+2
-1 * -3
-(4 + 7)

Term: -(1 + 2)
~~~~~~~~~~~~~~
Tree:
    -
    |
    +
   / \
  1   2
```
### 2.1
Create de `UnaryExpressionSyntax` class
    * Contains an operator (Plus or Minus)
    * Contains an operand (Expression to evaluate)

### 2.2
Add a way to evaluate the unary expression on `Evaluate` class.

### 2.3
`SyntaxFacts` class, add the unary precedence, with longer priority to avoid bugs.
```
Term: -1 + 2
~~~~~~~~~~~~
Bad precedence:
    -
    |
    +
   / \
  1   2

Correct precedence:
     +
    / \
   -   2
   |
   1
```
### 2.4
`Parser` class, parser the unary expression in `ParseExpression` method.

## 3.0 Type-Checking
Give the capacity to the compiler to know what type are each token.

### 3.1
Create some class about node representation.

### 3.2
Create the `Binder` class, this class handler the logic what the type-checker would do.

### 3.3
Add the handler errors in the Binder class, to avoid cascade errors for now.

## 4.0 Evaluator evolution
Change the `Evaluator` class, to work with nodes representation.

## 5.0 Apply changes
Class `Program`, modify the `Main` method and separate the code from the `binder` file into several files.

## 6.0 Add Booleans
App recognize `true` and `false` keywords

### 6.1
Class `lexer`, modify the `Lex` method to recognize letters.
Class `Parser`, convert `true` and `false` keywords into `LiteralExpressionSyntax`.
Class `Evaluator`, change the return of `Evaluate, EvaluateExpression` methods to `object` type.
The app is `case sensitive` yet.
There are a bug:
```
> false
False
> true
False

```

## 7.0 Add more operators

Class `Lexer`, and new operators '!, &&, ||, ==, !='.
Class `SyntaxFacts`, Add precedence to new operators.
Class `Binder`, handler Booleans types.
Class `Evaluator`, can resolve expressions with new operators.

The bug later was fixed.

### 7.1 Replace switch with opertor lookup
Class `Binder`, deleted `BindBinaryOperatorKind` and `BindUnaryOperatorKind` methods, to add new classes that handler bound operators.

### 7.2 == and != with int types

Modify `BoundBinaryOperator` class to add new items of `==, !=` to work with `int` types.
Fix the Bug in the `Bilder` class, that returned a bad type.

### 7.3 Fix more bugs
Class `Program`, Fix diagnostics error, binder's Diagnostics weren't concatenated.
Fix 'BoundBinaryOperator' class. Remove duplicate code.
Class `Binder`, Change <> characters by ''.
Add parentheses to the new tree structure in the 'binder' class.
Fix the invalid 'BoundNodeKind.UnaryExpression' into a binary expression

# Compiler part3

## 1.0 Extract compiler into a library
Create a new library project, and move the `CodeAnalysis` folder to here.

## 2.0 Public way to run

Create a public class to compile de code, and show evaluation results.

### 2.1 Apply library
Use library in `Program` class.

## 3.0 Improve diagnostics
Centralizing it into a single type, and report diagnostic spans.

### 3.1
Create a `TextSpan` class, to handler length info.
Create a new `Diagnostics` and `DiagnosticsBag` classes, to create a list of different types of diagnostics to apply into the app.

## 4.0
Fix the error to report positions in `operant tokens` of 2 characters.

## 5.0
`Program` class, render the diagnostics and spans in a nice way.

# Variables

## 6.0 New Expression Syntax
Create the `NameExpressionSyntax ` class: represents the identifier of variable
Create the `AssignmentExpressionSyntax` class: it is a way to represent a expression of assignment, contain:
  *  `IdentifierToken`: will be the identifier
  *  `EqualsToken
  *  `Expression`: will be the expression to assignment

## 7.0 Consisted in Binder
Update `BindExpression` method, add new method for parenthesized expression.

## 8.0 Add single = operator
`Lexer` class, add the assignment operator '='.

## 9.0 Parser assignment
`Parser` class, add new method to `parse` assignment expressions.

## 10.0 Variable into Dictionary
`Compilation` class, use dictionary concept to place variables, and request their values.

## 11.0 Add variables to semantic Tree
Create new bound classes for assignment and variable.
  * `BoundAssignmentExpression` handler name, and expression to be assignment
  * `BoundVariableExpression ` handler name and types

## 12.0 Binder

Implement the new logic for variables.

Select the bound expression to variables, and add diagnostic. when doesn't exist a variable
In this fase the assignment is worked only to int type.

# 13.0
App can run and, use a new way to work with int and bool types

# 14.0 VariableSymbol
Use a class to place and request variables. To look similar to and API

# 15.0
Replace binding logic with proper symbols, of variables.

# Compiler part 4 - Tests

## 1.0 Test Lexer
### 1.1
Add new Test project, and Create `LexerTest` file.

### 1.2
Expose method.
`SyntaxTree` class, add the `ParseTokens` method, to accesses the `Lexer` methods, since the most part of them are private.

### 1.3
First approach, need to convert entry into correct tokens.
```
input:
==
==!
=!=
```
`LexerTest` class, add the `GetToken` method. this method tests a single token.
  * Define all simple text that you hope that will be a token plus their syntax kind.
  * Add `Theories` and `memberData` decorators, to loop the `GetToken` method with another method.

### 1.4

`LexerTest` class, add the `GetTokenPairs` method, that tests a pair of tokens
  * Use the `GetTokens` method to obtains tokens
  * To work need a condition, and define the `RequiresSeparator` method.

### 1.5
We test white spaces, add the `GetSeparators` methods
  * Concatenate this enumerable result with `GetTokens` result, to tests them like singles tokens

### 1.6
Add test to pipe-line

## 2.0 Test SyntaxFacts
Convert to public the `SyntaxFacts` class.

### 2.1
Add a new method to map from keyword, in `SyntaxFacts` class, the `GetText` method, to return the text value of a operand token's syntax kind.
  * This is because we have many operators with a fixed text.

### 2.2
Create `SyntaxFactsTest`, to test the `GetText` method.

## 3.0 Test Parser
To test the correct structure of the tree

### 3.1
Create the `Parser` class.
Add other methods to `SyntaxFacts` class, to get a list of operator kinds.

### 3.2
Test binary expression, analyzing the operators precedence that there are in an expression:

### 3.3
`SyntaxFacts` class, add methods `GetUnaryOperatorKinds` and `GetBinaryOperatorKinds`
These methods take the precedence to return kinds.

### 3.4
to make that, add a new class `AssertingEnumerator`. This tests the way to build the tree and test the different Node and tokens that it has.
handler assert each node and token

### 3.5
`ParserTests` class, add the test to be honor at binary precedence `ParserBinaryExpressionHonornsPrecedences` method.
Test the building of a binary expression tree

```
Example, if have :
  - op1 = precedence operator 1
  - op2 = precedence operator 2

if (op1 >= op2)

          op2
        /    \
      op1     c
    /    \
  a       b

else

      op1
    /    \
  a      op2
       /    \
      c       b

```

### 3.6
`ParserTests` class, Add tests to ensure we honor unary and binary precedences, add `ParserUnaryExpressionHonornsPrecedences` method.
Test the building of a unary expression tree
```
Example, if have :
  - unary = precedence operator 1
  - binary = precedence operator 2

if (unary >= binary)

       binary
      /      \
  unary       b
    |   
    a   

else

        unary
          |
        binary
       /     \
      a       b

```

## 4.0 Test Evaluator

### 4.1
Test unary and binary evaluations, with numbers and booleans.
and variables assignation.

#Compiler part 5
# Clean-Up

## 1.0 Clean-up Lexer class

### 1.1
Optimize the `Lex` method, to match faster to dispatch, jumping to the corresponding case label.

### 1.2
Add white space more common like cases, but maintain a method to match other types of white spaces
Order the default case by the most common occurrences first.

## 2.0 Clean-up LexerTests

### 2.1
Refactor the `GetTokens` method, divide the large list into `fixedTokens`, these are defined in the `SyntaxFacts` class, and `dynamicTokens` that could have any value.

### 2.1
Create `LexerTestsAllTokens` method, to test we text all token kinds

## 3.0 Clean-up Evaluator class
Separate the 'Evaluate expression' method to read more easily and substantially
  * Chance if - else conditions by switch cases

## 4.0 Clean-up Parser class
Separate the `ParsePrimaryExpression` method, cleaning the switch case.

## 5.0 Immutable Collections
To improve the API

### 5.1
Install `System.Collections.Immutable` package, in the library project.

### 5.2
Change the `IReadOnlyList` to `ImmutableArray` for `diagnostic` parameters, because nobody wanted to `cast` all time this parameter and modify the data parameter.

### 5.3
Use ImmutableArray<T> in Parser class.

## 6.0 SyntaxNode more efficient
Get SyntaxNode.GetChildren non-virtual.

Make more efficient way to get elements. and avoid some of the downfalls when you do a evaluation of things and end up in recursion in parts that you're not supposed to be into.

### 6.1
Refactor the `GetChildren` method, using Reflection to obtain all class's properties that call this method
  * Delete the other methods that overwrite this, in the child classes.
  * The order of reflection return the elements is defined by the order of properties are declared into the class.

## 7.0 Span in Nodes
Construct the span over the first and last children.

### 7.1
Create `TextSpan.FromBounds()` helper method, to return a new `TextSpan` class.

### 7.2
(Not super efficient but it will work) `SyntaxNode` class, add virtual `Span` property, implement `TextSpan.FromBounds()` method, using the span of children.

### 7.3
Change `Span` property of `SyntaxToken` to override.

# Line Numbers
Change the app that is based in positions, that to reports error shows a bunch numbers of length of error, instead we want to show the line and column where is happened the error.

We will remember effectively how long the lines are.
And given a position we can compute the line number.
## 8.0 Text namespace
An new concept to API

### 8.1
Create 'Text' namespace and move 'TextSpan' into.

### 8.2 Exposes line number information
Create `SourceText` class, that handles information.
Create `TextLine` class, that is the definition of a line.

### 8.3
`SourceText` class
  * Create `ParseLines` method. that parse each line form the text.
  * Create `GetLineBreakWidth` method, that controls when a line is on the end.
  * Create `AddLine` method. Add each parse line to source result.

## 9.0 Traditional Binary Search
Apply this concept to look line index

### 9.1
`SourceText` class, create `GetLineIndex` method, that look for an line index into `Lines` immutable Array.

## 10.0 Show Text
Override `ToString` method.

### 10.1
`SourceText` class
  * return string of text
  * return string of text passing the star position and the length
  * return string of text passing span parameters

### 10.2
`TextLine` class
  * return text passing span parameters

### 10.3
Implement `SourceText` into the API
  * Fix a bug in `SourceText.GetLineIndex` method.

## 11.0 Diagnostics with more details
`Program` class, show line and character when an error occurs.
  * Fix a bug of null reference for cases where the token was inserted, `SyntaxToken` class.

## 12.0 Support for multiple Lines
`Program` class, add `textBuilder` parameter to handles multiple lines on console.
  * Make sure that the last line is added to `SourceText`

# Compiler part 6

# Changes

## 1.0 Test Text
Create new name space to test Text

### 1.1
Add Test: To check that the correct number of lines is added to the 'SourceText' class.

## 2.0 Color in console
Add Colorization to REPL

## 3.0 Represents entirety of the input
Add compilation unit:
  * The problem is when we need to pass multiples declarations or statements we don't have the singular root object
  * There isn't an array of expression
  * To pass a expression we have to pass the whole SyntaxTree first, and SyntaxTree isn't a node.

### 3.1
(in C++ each source file is effectively compiled to its own obj file)
Create root node:
  * Create `CompilationUnitSyntax` class: Represent the entire file.

### 3.2
 * Modify `SyntaxTree` constructor.
 * Add `ParseCompilationUnit` method to `Parser` class
 * Use the correct expression in `Compilation` class and tests.

# Scope
Things to change:
 * The currency way to map variables is with dictionary in the binder.
 * Binder should not even care about the values it just cares about the variables themselves
 * Reassignment variables with different types.

## 4.0 Handler variable symbols
  * Support nesting scopes
  * Support shadowing scopes

### 4.1
Create `BoundScope` class
  * With a private dictionary, to look up variables names fast.
  * Add `parent` parameter to handler scopes nested.
  * Add `TryDeclare` method to report error when a variable already exist in the same scope or.
  * and `TryLookup` method look for the variable on all scopes.

### 4.2
Create `BoundGlobalScope` class, catches the root of the program.
  * This has access to diagnostics, variables, expression in the program.

### 4.3
`Binder` class, now return `BoundGlobalScope` with all data.
Handles variables into the scope.

### 4.4
`Compilation` class is modify to work with `BoundGlobalScope`.
  * There isn't a scope yet.

## 5.0 Add scope

### 5.1
`Compilation` class, convert the global scope local variable to parameter
  * It's not a good idea for thread safely

### 5.2
Global scope will give the same result
  * Make thread safely
  * Use a pattern in `GlobalScope` property

## 6.0 Chain scopes
```
Chain previous submissions

submissions 3 -> submissions 2 -> submissions 1

```

### 6.1
`Binder` class, create `CreateParentScopes` method to map all scopes previous.
  * It's a nested scope

## 7.0 Chain the Compilation

### 7.1
Compilation is saved in previous variable to use it next time

### 7.2
  * Chain the diagnostics.
  * Add `#reset` command to reset scopes.

### 7.3
  * Improve the behavior when declare variables.
  * Add diagnostic to raise the error "cannot convert the types"

### 7.4
  * Doesn't have to declare a variable with different types in the same scope.
  * Extract 'ParseExpression' method into tests.

## 8.0 Add statements to the context
Can chain one into the other.
A single statement can be a block in which you nesting

### 8.1
Create the `StatementSyntax` class, and `BlockStatementSyntax` class, that represent a statements

### 8.2
Create the 'ExpressionStatementSyntax' class, in which can define the roles to valid statements.
or what expression  will be considered valid.
```
Is valid:
  a++ ,  a = 10 , a = a ,  M()
Is not valid:
  a + 1
```
### 8.3
Add to `Lexer` class and `SyntaxFact` class the brace symbols '{}' to match them.

### 8.4
`CompilationUnitSyntax` class, apply the `StatementSyntax` concept instead of `ExpressionSyntax`.

### 8.5
`ParserTest`, in `ParseExpression` method add a new assert before return the expression.

### 8.6
Update `Parser` class, to parse statement expression.

### 8.7
Add `BoundStatement`, `BoundBlockStatement` and `BoundExpressionStatement` concept into semantic logic.

### 8.8
`Binder` class, Add `BindStatement` method, to match with statements concepts.
  * Update `BoundGlobalScope` to receive  statements instead expressions.

### 8.9
`Evaluator` class
  * Add `EvaluateStatement` method, to match with statements concepts to evaluate internal expressions.
  * Create `lastValue` property map for the statements.

Can not reassignment a variable with another type of data.

## 9.0 Add variable declaration statement
people not implicitly creating them on first assignment.

### 9.1
Create `VariableDeclarationSyntax`  class, where define how will be the variables
```
var x = 10
let x = 10
```

### 9.2
Add 'let' and 'var' keywords to `SyntaxFact` class.

### 9.3
`Parser` class, add `ParseVariableDeclaration` method that handles declaration variables concept.
  * convert `if` conditional into `switch`, `ParseStatement` method.

### 9.4
Add `VariableDeclarationSyntax` class

### 9.5
`binder` class, add `BindVariableDeclaration` methos to analyze if is keyword of variable is `let`  or `var`.
  * Add `isReadOnly` parameter to `VariableSymbol` class.

### 9.6
Create `BoundVariableDeclaration` class

### 9.7
Add new reports to diagnostic class.

### 9.8
`Evaluator` class, add `EvaluateVariableDeclaration` method to handler variables.

# Compiler part 7
# Improve test

## 1.0 Helper type
case:
`[]` show the location of expected errors.
```
{
    let = [100]
    x [=] 10    <- a variable `let` is read-only
}
```
### 1.1
Create `AnnotatedText` class into Texts project.

### 1.2
`AnnotatedText` class, add `Parse` method, to analyze the input text without worry for whitespaces.

### 1.3
Create the `Unindent` method to take care of unindenting.
```
Delete extra whitespaces:
      var textToTest = @"
~~~~~~~~~~{
~~~~~~~~~~    var x = 10
~~~~~~~~~~    let x = 2
~~~~~~~~~~}
~~~~~~";
```
  * Provide `ToString` method to `TextSpan` class
  * Mark error messages consistent, `DiagnosticBag` class

### 1.4
`EvaluationTests` class, add test cases to report error for span in the text, Extract 'AssertValue' method.
  * Fix 'IdentifierToken' to 'EqualsToken' to report error in read-only variables, `Binder` class.

# Conditional operators

## 2.0 Add Conditional operators
` <  >  <=  >=`

### 2.1
`Lexer` class, add operators symbols.

### 2.2
`SyntaxFacts` class, add precedence and text to conditional operators.

### 2.3
Add them to operator table, `BoundBinaryOperator` class.
  * Compare between `int`

### 2.4
Add operators to 'Evaluator' class

### 2.5
Add tests cases to conditional operators

## 3.0 Conditionals `if` and `else`
### 3.1
  * Create `IfStatementSyntax` class like IfStatementSyntax
  * Create `ElseClauseSyntax` class like a node, because not need an expression or values.

### 3.2
`SyntaxFacts` class, add keywords

### 3.3
`Parser` class, add `ParseIfStatement` method to create new `if` statement, this calls `ParseElseClause` method

### 3.4
`Binder` class, add `BindIfStatement` method and override the `BindExpression` method to check the type of condition.
  * Create `BoundIfStatement` class.

### 3.5
`Evaluator` class, add `EvaluateIfStatement` that evaluates `ThenStatement` or `ElseStatement`.

### 3.6
`EvaluationTests` class, Add if-else cases to test.

## 4.0 While Statement
  * Add `while` keyword to `SyntaxFacts` class.
  * Add `ParseWhileStatement` method to `Parser` class.
  * Create `WhileStatementSyntax` class.
  * Create `BoundWhileStatement`class.
  * Add `BindWhileStatement` method to `Binder` class.
  * Add `EvaluateWhileStatement` method to `Evaluator` class.
  * Add test cases.

## 5.0 For statement
```
for i = 0 to 10
{
}
```
  * Add `for` keyword to `SyntaxFacts` class.
  * Create `ForStatementSyntax` class.
  * Add `ParseForStatement` method to `Parser` class.
  * Add `BindForStatement` method to `Binder` class.
  * Create `BoundForStatement` class
  * Add `EvaluateForStatement` method to `Evaluator` class.
  * Add tests.

## 6.0 Consume Tokens
  Ensure binder doesn't crash when binding fabricated identifiers
    * Modify `ParseBlockStatement` method in `Parser` class, to skip to next position when parse statement parse void.
    * Add test to control this.

## 7.0 Fix infinite loop
### 7.1
Modify `ParseBlockStatement` method in `Parser` class:
  * If `ParseStatement()`  did not consume any tokens, need to skip the current token and continue in order to avoid an infinite loop.
  * Do not need to report an error, because already tried to parse an expression statement and reported one.

### 7.2
Modify `BindNameExpression` method in `Binder` class:
  * This means the token was inserted by the parser.
  * We already reported error so we can just return an error expression.

# Compiler part 8

# Add new behaviors

## 1.0 Bitwise operators
Support for bitwise operators
  * Create test cases
  * Add `~`, `&`, `|` and `^` Bitwise operators to `Lexer` class.
  * Add  precedence of bitwise operators and get text cases in `SyntaxFacts` class.
  * Add operators to unary operators table in `BoundUnaryOperator` class.
  * Add operators to binary operators table in `BoundBinaryOperator` class.
  * Add evaluate cases in `Evaluator` class.

## 2.0 Rewrite Bound Tree
A way to get the children in `BoundNode`, 'Copy all of `SyntaxNode`'.
 * Add `WriteNode` method.

### 2.1
Add a new command `#showProgram` to Enabled bound trees to show.
  * Create `EmitTree` to write trees.

### 2.2
Change color by each type of bound node.
  * Add `GetColor` method in `BoundNode` class.

### 2.3
Handle binary and unary expressions
  * Add `GetText` method in `BoundNode` class.

### 2.4
`BoundNode` class. Add more properties to show.

  * Add `GetProperties` method that return an IEnumerable
  * Delete `WriteNode` method, to implement it into `PrettyPrint` method.
  * Loop for properties and write them with colors.

# Lowering

## 3.0 Bound tree rewriter
Add methods to rewrite Statements and Expressions.

## 4.0 Lowerer class
Create `Lower` method to rewrite a statement.

## 5.0 Invoke Lowering
`Compilation` class
  * Create `GetStatement` method that will Lower the global scope statement.
  * Change all `GlobalScope.Statement` for `GetStatement()`.

# Lowering For - While

## 6.0 Lower `for` statement into `while` statement
```
want to convert this:

for <var> = <lower> to <upper>
{
    <body>
}

 to:
------------------->

{
    var <var> = <lower>
    let <upperBound> = <upper>
    while (<var> <= <upperBound>)
    {
        <body>
        <var> = <var + 1>
    }
}

```

  * `Lowerer` class, override `RewriteForStatement` method.
  * `Evaluator` class, delete `EvaluateForStatement` method because we don't use it anymore.

# Gotos
"We don't add them to Syntax, because we just want to have it in internal representation."

## 7.0 Add Label Symbol
Create `LabelSymbol` class in `Nova.CodeAnalysis` namespace.

## 8.0
  *  `LabelStatement` declare the target.
  *  `GotoStatement` jumps to the target.

### 8.1
Create `BoundGotoStatement` class, `BoundConditionalGotoStatement` class and `BoundLabelStatement` class.

### 8.2
Add `LabelStatement`, `GotoStatement` and `ConditionalGotoStatement` cases to rewriter concepts.


## 9.0 Create Labels
`Lowerer` class, create `GenerateLabel` method, to return numerated labels.

# Lower `if` statement
with Gotos

```
We want to convert this:

- simple case:

  if <condition>
      <then>

  to:
  --------------->

  gotoFalse <condition> end
      <then>
  end:

==============================
- complex case:

  if <condition>
      <then>
  else
      <else>

  to:
  --------------->

  gotoFalse <condition> else
      <then>
  goto end
  else:
      <else>
  end:

```
## 10.0 Rewrite If Statement
`Lowerer` class, override `RewriteIfStatement` method.

# lower `while` statement
```
We want to convert this:

  while  <condition>
    <body>

to:
------------------->

goto check
continue:
    <body>
check:
gotoTrue <condition> continue
end:

```
## 11.0 Rewrite while statement
`Lowerer` class, override `RewriteWhileStatement` method.

## 12.0 Apply evaluate

### 12.1
`Evaluator` class, modify `Evaluate` method to add a way to select evaluate the things.
  * Delete `EvaluateStatement` method because we don't use evaluate if, while, or block statement anymore.

# Compiler part 9
# Better REPL
Read-Eval-Print-Loop
Keeping track to variables or language

* Support multi-line editing, syntax coloration and history.

## 1.0 Create REPL
Create `Repl` class that contains all code to run the program.

## 2.0 Create specific REPL
Create `NovaRepl` class that handles with the language specific stuff.

## 3.0 Editing a document
Change the control flow current in program, to return at a variable and you can update it in the input.
  * Return to a particular line
  * Typing or deleting  redraw

### 3.1
`Repl` class, convert the document input to local.

## 4.0 Model Editing behavior.
### 4.1
Create `SubmissionView` class that handler the input like a document.
  * `Render` method help to display document and set the cursor into right position.
  * `EditingSubmission` class was modify to handler the inserted keys
    * the `enter` or `controlEnter` key return all document.

### 4.2
Fix unexpected behavior that continue running with empty text.

### 4.3
Handle commands.
  * Fix the length covered by the right key
  * Deleted unusable method
### 4.4
Handle key modifiers, covered:
  * Backspace key
  * Delete key
  * Home key
  * End key
  * Tab key
  * Escape Key
  * Fix the new line error
  * Fix the adding whites space in a new line

## 5.0 History
Handle page up - page down.

### 5.1
Create a list of string that carry all input data

### 5.2
Crate `ClearHistory` method that clean history.

### 5.3
Handle `pageUp` and `pageDown` key to go through the history.
  * Create `UpdateDocumentFromHistory` method.

Fix errors:
  * Solved exception when 'pageDown' key is clicked
  * Fix cursor position and extra data is cleaned

## 6.0 Dynamic color
  * Create `RenderLine` method in `Repl` class.
  * Override `RenderLine` method in `NovaRepl` class, add color to specific tokens;

## 7.0 Fix evaluation result that overwrite code from history
  * Fix output, correcting evaluation result that overwrite code from history
  * When use 'pageUp' or 'pageDown'

## 8.0 Inserting a blank line between code
  * Use 'controlEnter' key to insert a new blank line below

## 9.0 Improve delete
Add 'delete between lines' behavior with 'Backspace' key

## 10.0 Fix errors
  * Avoid exception when accessing empty history
    - When use `pageUp` or `pageDown` keys without history.
  * Check whether the last token is missing
    - Fix infinite loops when a bad typing occur

## 11.0 Improve delete
Add 'delete between lines' behavior with 'Delete' key

# Compiler part 10

## 1.0 Right place
  * Move GetLastToken() to 'SyntaxNode' class.
  * Prettify the output
    - add coloration to Identifier
    - add coloration to numbers

## 2.0 Allow REPL to force evaluation
Sometimes our heuristic for detecting whether the submission is complete
doesn't work, in which case we want the user to have the ability to force
the completion. We do this by adding two blank lines.

* `NovaRepl` class, modify `IsCompleteSubmission` method to end the program when detect two blank lines.

## 3.0 Add ability to get lexer diagnostics
When parsing tokens only, we currently have no way to get access to the
lexer's diagnostics. This adds an optional out parameter.

The real fix, of course, would be to mirror what Roslyn is doing, which is
exposing the diagnostics on the actual tokens, which makes the output neatly
self-contained.

* `SyntaxTree` class, overload the `ParseTokens` method and change the return type.

## 4.0 Add support for string literals
To catch the quote symbol into a string, use helpers:
```
the input:
---------
  Test " asdsd

the output:
---------
one way:
  "Test \" asdsd"

second way:
  "Test "" asdsd"  <--
```

### 4.1
`Lexer` class, match `"` quote symbol, and add `ReadString` method.
  * Return report when a string is incomplete

### 4.2
`DiagnosticBag` class, add a report to `ReportUnterminatedString`

### 4.3
`Parser` class, add `ParserStringLiteral` method.

### 4.4
`LexerTests` class, add `LexerLexesUnterminatedString` method to test a specific case:
  * `"\"Test"`

## 5.0 Rename `LabelSymbol` to `BoundLabel`
That is because `LabelSymbol` was used only for binder to represent all loops.
  * Also, this class is an internal helper.
Move `BoundLabel` class to Binding namespace.

## 6.0 Create `Symbols` namespace
  * Move 'VariableSymbol' to a separate namespace
  * Enables to add more Symbols.

### 6.1
  * Create `Symbol` abstract class.
  * Create `SymbolKind` enum, that has all type of symbols

## 7.0 Type Symbols
Create `TypeSymbol` class to replace 'System.Type'

## 8.0 Cascading errors
Add concept of error, Add 'TypeSymbol.Error' to represent unbindable expressions
This avoids cascading errors when part of an expression cannot be bound.
Higher nodes, such as binary expressions, can detect this case and bail early.

### 8.1
Add `Error` type in `TypeSymbol` class.

### 8.2
Create `BoundErrorExpression` class, to return it into:
  * `BindUnaryExpression` method, when the type  is incorrect
  * `BindBinaryExpression` method, when the one side's type  is incorrect or operator is missing
  * `BindNameExpression` method, if we don't know what name it is or when we can't lookup the name

Not return it in:
  * `BindAssignmentExpression` method, is not useful, because assignments are right associative and need more information.
  * `BindLiteralExpression` are always define.

### 8.3
`BoundTreeRewriter`, add 'ErrorExpression' to 'RewriteExpression' method.

## 9.0 Fix exception when evaluating token sequences
  * <ArbitraryToken> <IdentifierToken>
  * Or incomplete Assignation statements
