grammar EfeuGrammar;

line
    : stat EOF
    ;

stat
    : assignmentStat      
    | forStat             
    | callStat             
    | exprStat             
    ;

assignmentStat
    : SET traversal TO expr
    ;

input
    : INPUT expr
    ;

forStat
    : FOR VAR IN expr
    ;

callStat
    : CALL ID
    | CALL ID WITH input+
    ;

exprStat
    : expr
    ;

traversal
    : VAR member*
    ;

member
    : '.' ID
    | '[' expr ']'
    ;

expr
    : atom
    | traversal
    ;

atom
    : VAR             #varExpr
    | INT             #intExpr
    | STRING          #stringExpr
    | funcCall        #funcExpr
    ;

funcCall
    : ID LPAREN (expr (COMMA expr)*)? RPAREN
    ;

// Lexer rules (keywords first!)
SET     : 'set';
FOR     : 'for';
CALL    : 'call';
IN      : 'in';
TO      : 'to';
WITH    : 'with';
INPUT   : ID ':' ;
VAR     : '@' [a-zA-Z_][a-zA-Z0-9_]* ;
ID      : [a-zA-Z_][a-zA-Z0-9_]* ;
DOT     : '.';
INT     : [0-9]+ ;
STRING  : '"' (~["\r\n])* '"' ;
LPAREN  : '(' ;
RPAREN  : ')' ;
COMMA   : ',' ;
WS      : [ \t\r\n]+ -> skip ;