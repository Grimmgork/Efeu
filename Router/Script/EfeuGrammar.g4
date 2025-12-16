grammar EfeuGrammar;

script 
    : scope EOF
    ;

scope
    : assignment* expression
    ;

expression
    : expression operator expression # BinaryExpr
    | LPAREN expression RPAREN # GroupExpr
    | expression with_method_call # MethodExpr
    | expression with_struct_modification # StructModExpr
    | expression with_array_modification # ArrayModExpr
    | expression then # ThenExpr
    | expression unless # UnlessExpr
    | array_constructor # ArrayExpr
    | struct_constructor # StructExpr
    | NIL # NilExpr
    | DECIMAL # DecimalEpr
    | FLOAT # FloatExpr
    | INT # IntegerExpr
    | STRING # StringExpr
    | TRUE # TrueExpr
    | FALSE # FalseExpr
    | traversal # TraversalExpr
    ;

operator
    : PLUS
    | MINUS
    | MUL
    | DIV
    | MOD
    | EQUALS
    ;

then
    : THEN expression ELSE expression
    ;

unless
    : UNLESS expression ELSE expression
    ;

array_constructor
    : SLPAREN (expression (COMMA expression)*)? SRPAREN
    ;

struct_constructor
    : CLPAREN (CONST expression (COMMA CONST expression))? CRPAREN
    ;

with_method_call
    : WITH ID (LPAREN expression RPAREN)? (DO CONST* scope END)?
    ;

with_struct_modification
    : WITH CLPAREN (CONST (with_struct_modification|with_array_modification|expression))+ CRPAREN
    ;

with_array_modification
    : WITH SLPAREN (LPAREN expression RPAREN ':' (with_struct_modification|with_array_modification|expression))+ SRPAREN
    ;

assignment
    : LET CONST expression NL
    ;

traversal
    : ID
    | ID '.' traversal
    | '(' expression ')'
    ;

// Lexer rules (keywords first!)
END     : 'end';
LET     : 'let';
DO      : 'do';
WITH    : '$';
NIL     : 'nil';
TRUE    : 'true';
FALSE   : 'false'; 

DECIMAL : [0-9]+ '.' [0-9]+ 'd' ;
FLOAT   : [0-9]+ '.' [0-9]+ ;
INT     : [0-9]+ ;
STRING  : '"' (~["\r\n])* '"' ;

COMMENT : '#' ~[\r\n]* -> skip ;

PLUS  : '+' ;
MINUS : '-' ;
MUL   : '*' ;
DIV   : '/' ;
MOD   : '%' ;
EQUALS: '=' ;
THEN  : 'then' ;
UNLESS: 'unless' ;
ELSE  : 'else' ;

CONST   : [a-zA-Z_][a-zA-Z0-9_]*':' ;
ID      : [a-zA-Z_][a-zA-Z0-9_]* ;
LPAREN  : '(' ;
RPAREN  : ')' ;
CLPAREN : '{' ;
CRPAREN : '}' ;
SLPAREN : '[' ;
SRPAREN : ']' ;
COMMA   : ',' ;
NL      : '\r\n'|'\n'|'\r' ;
WS      : [ \t]+ -> skip ;