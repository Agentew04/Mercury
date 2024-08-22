grammar Mips;

program         : (line | macroDefinition)* EOF;

line            : label? (instruction | directive | macroInvocation)? COMMENT? NEWLINE;

label           : ID ':' ;

directive       : '.' ID (directiveArg ( directiveArg)*)?;

directiveArg    : ID | STRING | INT | FLOAT | register | CHAR;

instruction     : ID (operand (',' operand)*)?;

operand         : register | variable | immediate | label | macroParam;

register        : '$' ID;

variable        : ID;

immediate       : INT | FLOAT;

macroDefinition : '.macro' ID macroParams? NEWLINE
                  (line | macroInvocation)+
                  '.end_macro' NEWLINE;

macroParams     : '(' (macroParam (',' macroParam)*)? ')';

macroInvocation : ID macroArgs?;

macroParam      : '%' ID;

macroArgs       : '(' (macroArg (',' macroArg)*)? ')';

macroArg        : operand;

COMMENT         : '#' ~[\r\n]*;

ID              : [a-zA-Z_][a-zA-Z_0-9]*;

STRING          : '"' (~["\r\n])* '"';
CHAR            : '\'' ~['\r\n] '\'';

INT             : ('0' | [1-9][0-9]*) | '0x' [0-9a-fA-F]+;

FLOAT           : (INT '.' [0-9]+ ([eE][+-]?[0-9]+)?) | ('.' [0-9]+ ([eE][+-]?[0-9]+)?);

NEWLINE         : '\r'? '\n';

WS              : [ \t]+ -> skip;