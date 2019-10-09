# Nova

[![Build Status](https://windyriey.visualstudio.com/Nova/_apis/build/status/luqp.Nova?branchName=master)](https://windyriey.visualstudio.com/Nova/_build/latest?definitionId=2&branchName=master) 

> Tests :  3540 pased 


Nova is a handwritten compiler in C#, in which you can solve expressions, write function and variables, and execute them.

<p align="center">
  <img src=".\Images\Gifs\01_Initial.gif">
</p>
Nova is base on Roslyn Compiler architecture.

The main structure is divided in: Syntax Trees, Symbols, Binder and Flow Analysis that are showed by the Emit.

<p align="center">
  <img src=".\Images\Readme\NovaStructure.png">
</p>

Nova Compiler has a REPL that handle the compilation of the code, and add some characteristics like:

#### Render Line with colors
The input source code is represented by tokens, so the document can identify each token and render it by colors.

#### Document
The input source code is handler by a `document` that allow:
  * Multiline
  * Back to edit
  * Add new line between the code
  * Browse history

<p align="center">
  <img src=".\Images\Gifs\02_Document.gif">
</p>

#### Basic command
  * `#showTree` - Render the syntax tree representation of input.
  * `#showProgram` - Render the structure created by semantic analysis.
  * `#cls` - Clean the screen.
  * `#reset` - Clean the variables declared in the scope.

<p align="center">
  <img src=".\Images\Gifs\03_Commands.gif">
</p>


# Syntax

## Trees
The Syntax Tree represent the lexical and syntactic structure of source code.
  * Hold all the source information in full fidelity.
  * Divide the source in lexical tokens.
  * Parse tokens for grammatical construct.
  * Represent errors in source code when the program is incomplete or malformed.
  * Syntax trees can be used as a way to construct and edit source text.

<p align="center">
  <img src=".\Images\Gifs\04_Show_Tree.gif">
</p>

Each syntax tree is made up of nodes, tokens, and trivia.
  * Nodes: Represent syntactic constructs (Declarations, Statements, and Expressions)
  * Tokens: Represent the smallest syntactic fragments of the code (Keywords, Identifiers, Literals, and Punctuation)
  * Trivia: Source text insignificant (Whitespace)

# Semantics

## Binding
Checks the semantic consistency of the code. It uses the syntax tree of the previous phase along with the symbol table to verify that the given source code is semantically consistent.

Functions of semantic analysis:

* Helps you to store type information gathered and save it in symbol table or syntax tree.
* In the case of type mismatch a semantic error is shown.
* Collects type information and checks for type compatibility.
* Checks if the source language permits the operands or not.
* Identify the unary and binary expressions.

## Symbol Table
A symbol table contains a record for each identifier with fields for the attributes of the identifier. This component makes it easier for the compiler to search the identifier record and retrieve it quickly.

### Symbols
Every namespace, type, method, property, field, parameter, or local variable is represented by a symbol.


<p align="center">
  <img src=".\Images\Gifs\05_Show_Program.gif">
</p>

## Control Flow
Nova compiler create a graph that represents the control flow of the function or expressions executed.

  * All nodes in the graph are called basic blocks.
  * Basic block is a list of statements that are executed in sequence.

<p align="center">
  <img src=".\Images\Gifs\07_graphs.gif">
</p>

# Diagnostics
In each phases of the compiler errors are catch by a diagnostic bag that return the fauld details.

<p align="center">
  <img src=".\Images\Gifs\06_Diagnostics.gif">
</p>