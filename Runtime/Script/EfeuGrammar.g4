grammar EfeuGrammar;

script 
    : scope EOF
    ;

scope
    : assignment* expression
    ;

expression
    : LPAREN expression RPAREN # GroupExpr
    | expression operator expression # BinaryExpr
    | expression unary_operator # UnaryExpr
    | expression with_method_call # MethodExpr
    | expression with_struct_mod # StructModExpr
    | expression with_array_mod # ArrayModExpr
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

unary_operator
    : INCR
    | DECR
    | NOT
    ;

then
    : THEN expression ELSE expression END?
    ;

unless
    : UNLESS expression ELSE expression END?
    ;

array_constructor
    : SLPAREN (expression (COMMA expression)*)? SRPAREN
    ;

struct_constructor
    : CLPAREN (CONST expression ((COMMA?) CONST expression)*)? CRPAREN
    ;

with_method_call
    : WITH ID (LPAREN expression RPAREN)? (DO CONST* scope END)?
    ;

with_struct_mod
    : WITH CLPAREN with_struct_mod_field+ CRPAREN
    ;

with_struct_mod_field
    : CONST (with_struct_mod|with_array_mod|expression)
    ;

with_array_mod
    : WITH SLPAREN with_array_mod_item+ SRPAREN
    ;

with_array_mod_item
    : LPAREN expression RPAREN ':' (with_struct_mod|with_array_mod|expression)
    ;

assignment
    : LET CONST expression
    ;

traversal
    : ID traversal_tail?
    ;
    
traversal_tail
    : TRAVERSAL_ID traversal_tail?
    | TRAVERSAL_ID_EXPR expression ')' traversal_tail?
    | '(' expression ')' traversal_tail?
    ;

// Lexer rules (keywords first!)
END     : 'end';
LET     : 'let';
DO      : 'do';
WITH    : '$';
NOT     : 'not';
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
INCR  : '++' ;
DECR  : '--' ;
THEN  : 'then' ;
UNLESS: 'unless' ;
ELSE  : 'else' ;

CONST   : [a-zA-Z_][a-zA-Z0-9_]*':' ;
ID      : [a-zA-Z_][a-zA-Z0-9_]* ;
TRAVERSAL_ID_EXPR : '.' [a-zA-Z_][a-zA-Z0-9_]* '(';
TRAVERSAL_ID : '.' [a-zA-Z_][a-zA-Z0-9_]* ;
LPAREN  : '(' ;
RPAREN  : ')' ;
CLPAREN : '{' ;
CRPAREN : '}' ;
SLPAREN : '[' ;
SRPAREN : ']' ;
COMMA   : ',' ;
WS      : [ \t\r\n]+ -> skip ;
LINE_COMMENT : '#' ~[\r\n]* -> skip ;