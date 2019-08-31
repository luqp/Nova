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
