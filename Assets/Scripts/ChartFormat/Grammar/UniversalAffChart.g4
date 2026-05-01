/**
 * Based on Arcade-plus
 *
 * @source https://github.com/yojohanshinwataikei/Arcade-plus/blob/master/Assets/Scripts/Aff/ArcaeaFileFormat.g4
 * @author yojohanshinwataikei
 */

// $antlr-format alignTrailingComments true, columnLimit 150, minEmptyLines 1, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, alignSemicolons hanging, alignColons hanging

grammar UniversalAffChart;

// $antlr-format alignTrailingComments true, columnLimit 150, minEmptyLines 0, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, minEmptyLines 0, alignSemicolons ownLine
// $antlr-format alignColons trailing, singleLineOverrulesHangingColon true, alignLexerCommands true, alignLabels true, alignTrailers true

Whitespace: [\p{White_Space}] -> skip;

// LineComment 
//     : '//' ~[\r\n]*  ('\r'? '\n' | EOF)
//     -> skip
//     ;
// 
// BlockComment 
//     : '/*' .*? '*/'
//     -> skip
//     ;

LParen    : '(';
RParen    : ')';
LBrack    : '[';
RBrack    : ']';
LBrace    : '{';
RBrace    : '}';
LChev     : '<';
RChev     : '>';
Comma     : ',';
Semicolon : ';';
Colon     : ':';
Equal     : '=';

Space    : ' ';
Plus     : '+';
Minus    : '-';
Multiply : '*';
Divide   : '/';
Mod      : '%';
Pow      : '^';

fragment SQUOTE     : '\'';
fragment DQUOTE     : '"';
fragment BQUOTE     : '`';
fragment UNDERLINE  : '_';
fragment SHARP      : '#';
fragment ALPHABET   : [a-zA-Z];
fragment DIGITSTART : [1-9];
fragment ZERO       : '0';
fragment DIGIT      : DIGITSTART | ZERO;
fragment DOT        : '.';
fragment NEGATIVE   : '-';
fragment SPACE      : ' ';
fragment SLASH      : '/';
fragment BSLASH     : '\\';

// $antlr-format alignTrailingComments true, columnLimit 150, minEmptyLines 1, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine false, allowShortBlocksOnASingleLine true, alignSemicolons hanging, alignColons hanging

chart
    : body EOF
    ;

value
    : ExprString
    | (Word Equal value)
    | String
    | Word
    | expr
    | ()
    ;

expr
    : LParen expr RParen
    | Minus expr
    | expr (Multiply | Divide | Mod) expr
    | expr (Plus | Minus) expr
    | expr Pow expr
    | Int
    | Float
    ;

values
    : LParen (value (Comma value)*)? RParen
    ;

event
    : Word? values subEvents? properties? segment?
    ;

item
    : event Semicolon
    ;

property
    : Word (Colon value)?
    ;

properties
    : LChev (property (Comma property)*)? RChev
    ;

subEvents
    : LBrack (event (Comma event)*)? RBrack
    ;

segment
    : LBrace body RBrace
    ;

body
    : item*
    ;

ExprString
    : BQUOTE ~[`]* BQUOTE
    ;

String
    : SQUOTE ~[']* SQUOTE
    | DQUOTE ~["]* DQUOTE
    ;

fragment WORD_HEAD // Word must starts with '#', '_' or an Alpha
    : (SHARP | UNDERLINE | ALPHABET)
    ;

fragment WORD_BODY
    : (
        SHARP
        | UNDERLINE
        | ALPHABET
        | DIGIT
        | DOT
        | SLASH
        | BSLASH
        | NEGATIVE
        | SPACE // space can only be in the middle part of the Word
    )
    ;

fragment WORD_TAIL // space is not allowed as ending
    : (SHARP | UNDERLINE | ALPHABET | DIGIT | DOT | SLASH | BSLASH | NEGATIVE)
    ;

Word
    : WORD_HEAD (WORD_BODY* WORD_TAIL)?
    ;

Int
    : NEGATIVE? (ZERO | DIGITSTART DIGIT*)
    ;

Float
    : Int DOT DIGIT+
    ;