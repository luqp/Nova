# Nova

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
[Second part 2:](https://github.com/luqp/Nova/pull/2/commits/f224cd3ba810b531eb1f89fe2694bd0ebbfd3472)

Continuacion de la segunda parte...

Se define la logica para unir e interpretar los nodos segun el orden de operacion de realizan.

### 2.2
Clase `Parser`, crear el metodo `Match`, para comprobar el correcto tipo del token.

Clase `Parser`, crear los metodos `Parse` y `ParsePrimaryExpression`, para analizar la expresion en un nodo, y descomponerla si es necesario.

## Imprimir
[Fourt part:](https://github.com/luqp/Nova/pull/2/commits/f224cd3ba810b531eb1f89fe2694bd0ebbfd3472):

### 4.1
En la clase `Program`, crear el metodo estatido `PrettyPrint`, esta parte es solo para mostrar la estructura de los nodos en forma de arbol que el programa esta siguiendo.

## Errores
[Five part :](https://github.com/luqp/Nova/pull/2/commits/5892ef4f443285cdc751d43c9e2c0cd48e9498d5)

Implementa el diagnostico de errores, Identifica los tokens malos.

### 5.1
Crear la clase `SyntaxTree`, esta sera un tipo que representa todas las entradas una forma de limpiar el codigo.

Con esto se modificando el metodo `Parse`, en la clase `Parser`.
Ahora el metodo `Parse` revisa que la expresion se haya terminado de leer.

## Evaluacion
[Six part 1:](https://github.com/luqp/Nova/pull/2/commits/b042a7bbad726568a54033aa5ad5a75b4690fe0e)

Evalua las expresiones en cada nodo del arbol

### 6.0
Create Evaluator class.

### 6.1

Clase `Evaluator`, crear los metodos `Evaluate`y `EvaluateExpression`, que trabajan con los tipos de expresiones de nuestra aplicacion, ejm: BinaryExpressionSyntax, NumberExpressionSyntax, etc.

### Triki tip [Six part 2:](https://github.com/luqp/Nova/pull/2/commits/70b52840847954c372f29c274e0db88d47b078ac)

* Corrige el error, para evaluar los operadores `*, /` con mayor prioridad que los operadores `+, -`
### 6.2
Dividimos `ParseExpression` method en `PaseFactor` y `ParserTerme`.

## Parentesis
[Seven part :](https://github.com/luqp/Nova/pull/2/commits/05e5adc85f8160f4c5f39194f2f9599c6a82003a)

Add Parenthesis expresion.

### 7.1
Crear la clase `ParenthesizedExpressionSyntax`.

### 7.2
Modificar el metodo `ParsePrimayExpression`, en la clase `Parser` y Agregar el metodo `ParseExpresion`.

### 7.3
Modificar el metodo `EvaluateExpression`
