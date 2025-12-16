grammar EfeuGrammar;

line
    : stat EOF
    ;

stat
    : assignmentStat NL      
    | forStat NL         
    | callStat NL          
    | exprStat NL
    | letExprStat NL
    ;

assignmentStat
    : SET traversal TO expr
    ;

input
    : FIELD expr
    ;

forStat
    : FOR variable IN expr
    ;

callStat
    : CALL ID
    | CALL ID WITH input+
    ;

exprStat
    : expr
    ;

letExprStat
    : letExpr+ expr
    ;

letExpr
    : LET ID BE expr
    ;

traversal
    : variable member*
    ;

member
    : '.' ID
    | '[' expr ']'
    ;

variable
    : '@' ID
    ;

arrayLiteral
    : '[' expr (COMMA expr)* ']'
    ;

hashLiteral
    : '{' (FIELD expr)* '}' 
    ;

expr
    : literal postfixOp*        #literalExpr
    | funcCall postfixOp*       #funcExpr
    | traversal postfixOp*      #traversalExpr
    ;

literal
    : NIL             #nilExpr
    | variable        #varExpr
    | INT             #intExpr
    | STRING          #stringExpr
    | arrayLiteral    #arrayExpr
    | hashLiteral     #hashExpr
    ;

funcCall
    : ID LPAREN (expr (COMMA expr)*)? RPAREN
    ;

postfixOp
    : ID block?
    ;

block
    : DO expr END
    ;

const
    : ID ':'
    ;

// Lexer rules (keywords first!)
END     : 'end';
LET     : 'let';
BE      : 'be';
SET     : 'set';
FOR     : 'for';
CALL    : 'call';
AWAIT   : 'await';
IN      : 'in';
TO      : 'to';
DO      : 'do';
NIL     : 'nil';
WITH    : 'with';
FIELD   : ID ':' ;
OP      : [+*/-mod~] ;
INT     : [0-9]+ ;
ID      : [a-zA-Z_][a-zA-Z0-9_]* ;
DOT     : '.';
STRING  : '"' (~["\r\n])* '"' ;
LPAREN  : '(' ;
RPAREN  : ')' ;
COMMA   : ',' ;
NL      : ('\r\n'|'\n'|'\r') ;
WS      : [ \t\r\n]+ -> skip ;