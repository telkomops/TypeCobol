﻿using System.Linq;

namespace TypeCobol.Compiler.Scanner
{
    // WARNING : both enumerations below (families / types) must stay in sync
    // WARNING : make sure to update the tables in TokenUtils if you add one more token family or one more token type

    public enum TokenFamily
    {
        //          0 : Error
        Invalid=0,
        //   1 ->   3 : Whitespace
        // p46: The separator comma and separator semicolon can
        // be used anywhere the separator space is used.
        Whitespace=1,
        //   4 ->   5 : Comments
        Comments=4,
        // 6 ->  11 : Separators - Syntax
        SyntaxSeparator=6,
        //  12 ->  16 : Special character word - Arithmetic operators
        ArithmeticOperator=12,
        //  17 ->  21 : Special character word - Relational operators
        RelationalOperator=17,
        //  22 ->  27 : Literals - Alphanumeric
        AlphanumericLiteral=22,
        //  28 ->  31 : Literals - Numeric 
        NumericLiteral=28,
        //  32 ->  34 : Literals - Syntax tokens
        SyntaxLiteral=32,
        //  35 ->  39 : Symbols
        Symbol=35,
        //  40 ->  58 : Keywords - Compiler directive starting tokens
        CompilerDirectiveStartingKeyword=40,
        //  59 ->  154 : Keywords - Code element starting tokens
        CodeElementStartingKeyword=59,
        // 155 -> 185 : Keywords - Special registers
        SpecialRegisterKeyword=155,
        // 186 -> 199 : Keywords - Figurative constants
        FigurativeConstantKeyword=186, 
        // 200 -> 201 : Keywords - Special object identifiers
        SpecialObjetIdentifierKeyword=200,
        // 202 -> 495 : Keywords - Syntax tokens
        SyntaxKeyword=202,
        // 496 -> 498 : Keywors - Cobol V6
        CobolV6Keyword = 496,
        // 499 -> 500 : Keywords - Cobol 2002
        Cobol2002Keyword = 499,
        // 501 -> 505 : Keywords - TypeCobol
        TypeCobolKeyword = 501,

        // 506-> 506 : Operators - TypeCobol
        TypeCobolOperators= 506, 

        // 507 -> 509 : Compiler directives
        CompilerDirective = 507,
        // 510 -> 510 : Internal token groups - used by the preprocessor only
        InternalTokenGroup = 510,
        // 511 -> 522 : Formalized Comments Tokens
        FormalizedCommentsFamily = 511,
        // 523 -> 524 : Multilines Comments Tokens
        MultilinesCommentsFamily = 523
    }

    // INFO : the list below is generated from the file Documentation/Studies/CobolLexer.tokens.xls
    //Or See TokenType.xlsx file that contains data to regenerate the TokenType enum 
    // WARNING : the list of tokens in CobolWords.g4 must stay in sync

    public enum TokenType
    {
        EndOfFile=-1,
        InvalidToken=0,
        SpaceSeparator=1,
        CommaSeparator=2,
        SemicolonSeparator=3,
        FloatingComment=4,
        CommentLine=5,
        PeriodSeparator=6,
        ColonSeparator=7,
        QualifiedNameSeparator=8,
        LeftParenthesisSeparator=9,
        RightParenthesisSeparator=10,
        PseudoTextDelimiter=11,
        PlusOperator=12,
        MinusOperator=13,
        DivideOperator=14,
        MultiplyOperator=15,
        PowerOperator=16,
        LessThanOperator=17,
        GreaterThanOperator=18,
        LessThanOrEqualOperator=19,
        GreaterThanOrEqualOperator=20,
        EqualOperator=21,
        AlphanumericLiteral=22,
        HexadecimalAlphanumericLiteral=23,
        NullTerminatedAlphanumericLiteral=24,
        NationalLiteral=25,
        HexadecimalNationalLiteral=26,
        DBCSLiteral=27,
        LevelNumber=28,
        IntegerLiteral=29,
        DecimalLiteral=30,
        FloatingPointLiteral=31,
        PictureCharacterString=32,
        CommentEntry=33,
        ExecStatementText=34,
        SectionParagraphName=35,
        IntrinsicFunctionName=36,
        ExecTranslatorName=37,
        PartialCobolWord=38,
        UserDefinedWord=39,
        ASTERISK_CBL=40,
        ASTERISK_CONTROL=41,
        BASIS=42,
        CBL=43,
        COPY=44,
        DELETE_CD=45,
        EJECT=46,
        ENTER=47,
        EXEC_SQL=48,
        INSERT=49,
        PROCESS=50,
        READY=51,
        RESET=52,
        REPLACE=53,
        SERVICE_CD=54,
        SKIP1=55,
        SKIP2=56,
        SKIP3=57,
        TITLE=58,
        ACCEPT=59,
        ADD=60,
        ALTER=61,
        APPLY=62,
        CALL=63,
        CANCEL=64,
        CLOSE=65,
        COMPUTE=66,
        CONFIGURATION=67,
        CONTINUE=68,
        DATA=69,
        DECLARATIVES=70,
        DECLARE=71,
        DELETE=72,
        DISPLAY=73,
        DIVIDE=74,
        ELSE=75,
        END=76,
        END_ADD=77,
        END_CALL=78,
        END_COMPUTE=79,
        END_DECLARE=80,
        END_DELETE=81,
        END_DIVIDE=82,
        END_EVALUATE=83,
        END_EXEC=84,
        END_IF=85,
        END_INVOKE=86,
        END_MULTIPLY=87,
        END_PERFORM=88,
        END_READ=89,
        END_RETURN=90,
        END_REWRITE=91,
        END_SEARCH=92,
        END_START=93,
        END_STRING=94,
        END_SUBTRACT=95,
        END_UNSTRING=96,
        END_WRITE=97,
        END_XML=98,
        ENTRY=99,
        ENVIRONMENT=100,
        EVALUATE=101,
        EXEC=102,
        EXECUTE=103,
        EXIT=104,
        FD=105,
        FILE=106,
        FILE_CONTROL=107,
        GO=108,
        GOBACK=109,
        I_O_CONTROL=110,
        ID=111,
        IDENTIFICATION=112,
        IF=113,
        INITIALIZE=114,
        INPUT_OUTPUT=115,
        INSPECT=116,
        INVOKE=117,
        LINKAGE=118,
        LOCAL_STORAGE=119,
        MERGE=120,
        MOVE=121,
        MULTIPLE=122,
        MULTIPLY=123,
        NEXT=124,
        OBJECT_COMPUTER=125,
        OPEN=126,
        PERFORM=127,
        PROCEDURE=128,
        READ=129,
        RELEASE=130,
        REPOSITORY=131,
        RERUN=132,
        RETURN=133,
        REWRITE=134,
        SAME=135,
        SD=136,
        SEARCH=137,
        SELECT=138,
        SERVICE=139,
        SET=140,
        SORT=141,
        SOURCE_COMPUTER=142,
        SPECIAL_NAMES=143,
        START=144,
        STOP=145,
        STRING=146,
        SUBTRACT=147,
        UNSTRING=148,
        USE=149,
        WHEN=150,
        WORKING_STORAGE=151,
        WRITE=152,
        XML=153,
        GLOBAL_STORAGE=154,
        ADDRESS=155,
        DEBUG_CONTENTS=156,
        DEBUG_ITEM=157,
        DEBUG_LINE=158,
        DEBUG_NAME=159,
        DEBUG_SUB_1=160,
        DEBUG_SUB_2=161,
        DEBUG_SUB_3=162,
        JNIENVPTR=163,
        LENGTH=164,
        LINAGE_COUNTER=165,
        RETURN_CODE=166,
        SHIFT_IN=167,
        SHIFT_OUT=168,
        SORT_CONTROL=169,
        SORT_CORE_SIZE=170,
        SORT_FILE_SIZE=171,
        SORT_MESSAGE=172,
        SORT_MODE_SIZE=173,
        SORT_RETURN=174,
        TALLY=175,
        WHEN_COMPILED=176,
        XML_CODE=177,
        XML_EVENT=178,
        XML_INFORMATION=179,
        XML_NAMESPACE=180,
        XML_NAMESPACE_PREFIX=181,
        XML_NNAMESPACE=182,
        XML_NNAMESPACE_PREFIX=183,
        XML_NTEXT=184,
        XML_TEXT=185,
        HIGH_VALUE=186,
        HIGH_VALUES=187,
        LOW_VALUE=188,
        LOW_VALUES=189,
        NULL=190,
        NULLS=191,
        QUOTE=192,
        QUOTES=193,
        SPACE=194,
        SPACES=195,
        ZERO=196,
        ZEROES=197,
        ZEROS=198,
        SymbolicCharacter=199,
        SELF=200,
        SUPER=201,
        ACCESS=202,
        ADVANCING=203,
        AFTER=204,
        ALL=205,
        ALPHABET=206,
        ALPHABETIC=207,
        ALPHABETIC_LOWER=208,
        ALPHABETIC_UPPER=209,
        ALPHANUMERIC=210,
        ALPHANUMERIC_EDITED=211,
        ALSO=212,
        ALTERNATE=213,
        AND=214,
        ANY=215,
        ARE=216,
        AREA=217,
        AREAS=218,
        ASCENDING=219,
        ASSIGN=220,
        AT=221,
        AUTHOR=222,
        BEFORE=223,
        BEGINNING=224,
        BINARY=225,
        BLANK=226,
        BLOCK=227,
        BOTTOM=228,
        BY=229,
        CHARACTER=230,
        CHARACTERS=231,
        CLASS=232,
        CLASS_ID=233,
        COBOL=234,
        CODE=235,
        CODE_SET=236,
        COLLATING=237,
        COM_REG=238,
        COMMA=239,
        COMMON=240,
        COMP=241,
        COMP_1=242,
        COMP_2=243,
        COMP_3=244,
        COMP_4=245,
        COMP_5=246,
        COMPUTATIONAL=247,
        COMPUTATIONAL_1=248,
        COMPUTATIONAL_2=249,
        COMPUTATIONAL_3=250,
        COMPUTATIONAL_4=251,
        COMPUTATIONAL_5=252,
        CONTAINS=253,
        CONTENT=254,
        CONVERTING=255,
        CORR=256,
        CORRESPONDING=257,
        COUNT=258,
        CURRENCY=259,
        DATE=260,
        DATE_COMPILED=261,
        DATE_WRITTEN=262,
        DAY=263,
        DAY_OF_WEEK=264,
        DBCS=265,
        DEBUGGING=266,
        DECIMAL_POINT=267,
        DELIMITED=268,
        DELIMITER=269,
        DEPENDING=270,
        DESCENDING=271,
        DISPLAY_1=272,
        DIVISION=273,
        DOWN=274,
        DUPLICATES=275,
        DYNAMIC=276,
        EGCS=277,
        END_OF_PAGE=278,
        ENDING=279,
        EOP=280,
        EQUAL=281,
        ERROR=282,
        EVERY=283,
        EXCEPTION=284,
        EXTEND=285,
        EXTERNAL=286,
        FACTORY=287,
        FALSE=288,
        FILLER=289,
        FIRST=290,
        FOOTING=291,
        FOR=292,
        FROM=293,
        FUNCTION=294,
        FUNCTION_POINTER=295,
        GENERATE=296,
        GIVING=297,
        GLOBAL=298,
        GREATER=299,
        GROUP_USAGE=300,
        I_O=301,
        IN=302,
        INDEX=303,
        INDEXED=304,
        INHERITS=305,
        INITIAL=306,
        INPUT=307,
        INSTALLATION=308,
        INTO=309,
        INVALID=310,
        IS=311,
        JUST=312,
        JUSTIFIED=313,
        KANJI=314,
        KEY=315,
        LABEL=316,
        LEADING=317,
        LEFT=318,
        LESS=319,
        LINAGE=320,
        LINE=321,
        LINES=322,
        LOCK=323,
        MEMORY=324,
        METHOD=325,
        METHOD_ID=326,
        MODE=327,
        MODULES=328,
        MORE_LABELS=329,
        NATIONAL=330,
        NATIONAL_EDITED=331,
        NATIVE=332,
        NEGATIVE=333,
        NEW=334,
        NO=335,
        NOT=336,
        NUMERIC=337,
        NUMERIC_EDITED=338,
        OBJECT=339,
        OCCURS=340,
        OF=341,
        OFF=342,
        OMITTED=343,
        ON=344,
        OPTIONAL=345,
        OR=346,
        ORDER=347,
        ORGANIZATION=348,
        OTHER=349,
        OUTPUT=350,
        OVERFLOW=351,
        OVERRIDE=352,
        PACKED_DECIMAL=353,
        PADDING=354,
        PAGE=355,
        PASSWORD=356,
        PIC=357,
        PICTURE=358,
        POINTER=359,
        POSITION=360,
        POSITIVE=361,
        PROCEDURE_POINTER=362,
        PROCEDURES=363,
        PROCEED=364,
        PROCESSING=365,
        PROGRAM=366,
        PROGRAM_ID=367,
        RANDOM=368,
        RECORD=369,
        RECORDING=370,
        RECORDS=371,
        RECURSIVE=372,
        REDEFINES=373,
        REEL=374,
        REFERENCE=375,
        REFERENCES=376,
        RELATIVE=377,
        RELOAD=378,
        REMAINDER=379,
        REMOVAL=380,
        RENAMES=381,
        REPLACING=382,
        RESERVE=383,
        RETURNING=384,
        REVERSED=385,
        REWIND=386,
        RIGHT=387,
        ROUNDED=388,
        RUN=389,
        SECTION=390,
        SECURITY=391,
        SEGMENT_LIMIT=392,
        SENTENCE=393,
        SEPARATE=394,
        SEQUENCE=395,
        SEQUENTIAL=396,
        SIGN=397,
        SIZE=398,
        SORT_MERGE=399,
        SQL=400,
        SQLIMS=401,
        STANDARD=402,
        STANDARD_1=403,
        STANDARD_2=404,
        STATUS=405,
        SUPPRESS=406,
        SYMBOL=407,
        SYMBOLIC=408,
        SYNC=409,
        SYNCHRONIZED=410,
        TALLYING=411,
        TAPE=412,
        TEST=413,
        THAN=414,
        THEN=415,
        THROUGH=416,
        THRU=417,
        TIME=418,
        TIMES=419,
        TO=420,
        TOP=421,
        TRACE=422,
        TRAILING=423,
        TRUE=424,
        TYPE=425,
        UNBOUNDED=426,
        UNIT=427,
        UNTIL=428,
        UP=429,
        UPON=430,
        USAGE=431,
        USING=432,
        VALUE=433,
        VALUES=434,
        VARYING=435,
        WITH=436,
        WORDS=437,
        WRITE_ONLY=438,
        XML_SCHEMA=439,
        ALLOCATE=440,
        CD=441,
        CF=442,
        CH=443,
        CLOCK_UNITS=444,
        COLUMN=445,
        COMMUNICATION=446,
        CONTROL=447,
        CONTROLS=448,
        DE=449,
        DEFAULT=450,
        DESTINATION=451,
        DETAIL=452,
        DISABLE=453,
        EGI=454,
        EMI=455,
        ENABLE=456,
        END_RECEIVE=457,
        ESI=458,
        FINAL=459,
        FREE=460,
        GROUP=461,
        HEADING=462,
        INDICATE=463,
        INITIATE=464,
        LAST=465,
        LIMIT=466,
        LIMITS=467,
        LINE_COUNTER=468,
        MESSAGE=469,
        NUMBER=470,
        PAGE_COUNTER=471,
        PF=472,
        PH=473,
        PLUS=474,
        PRINTING=475,
        PURGE=476,
        QUEUE=477,
        RD=478,
        RECEIVE=479,
        REPORT=480,
        REPORTING=481,
        REPORTS=482,
        RF=483,
        RH=484,
        SEGMENT=485,
        SEND=486,
        SOURCE=487,
        SUB_QUEUE_1=488,
        SUB_QUEUE_2=489,
        SUB_QUEUE_3=490,
        SUM=491,
        TABLE=492,
        TERMINAL=493,
        TERMINATE=494,
        TEXT=495,
        END_JSON=496,
        JSON=497,
        VOLATILE=498,
        TYPEDEF=499,
        STRONG=500,
        UNSAFE=501,
        PUBLIC=502,
        PRIVATE=503,
        IN_OUT=504,
        STRICT=505,
        QUESTION_MARK=506,
        COMPILER_DIRECTIVE=507,
        COPY_IMPORT_DIRECTIVE=508,
        REPLACE_DIRECTIVE=509,
        CONTINUATION_TOKEN_GROUP=510,
        FORMALIZED_COMMENTS_START = 511,
        FORMALIZED_COMMENTS_STOP = 512,
        FORMALIZED_COMMENTS_DESCRIPTION = 513,
        FORMALIZED_COMMENTS_PARAMETERS = 514,
        FORMALIZED_COMMENTS_DEPRECATED = 515,
        FORMALIZED_COMMENTS_REPLACED_BY = 516,
        FORMALIZED_COMMENTS_RESTRICTION = 517,
        FORMALIZED_COMMENTS_NEED = 518,
        FORMALIZED_COMMENTS_SEE = 519,
        FORMALIZED_COMMENTS_TODO = 520,
        FORMALIZED_COMMENTS_VALUE = 521,
        AT_SIGN = 522,
        MULTILINES_COMMENTS_START = 523,
        MULTILINES_COMMENTS_STOP = 524,



    }

    public static class TokenConst {
        private static readonly TokenType[] TypeCobolTokenType =
        {
            TokenType.DECLARE, TokenType.END_DECLARE, TokenType.PUBLIC, TokenType.PRIVATE, TokenType.IN_OUT,
            TokenType.UNSAFE, TokenType.STRICT, TokenType.QUESTION_MARK
        };

        private static readonly TokenType[] Cobol2002TokenType = {TokenType.STRONG, TokenType.TYPEDEF};

        public static CobolLanguageLevel GetCobolLanguageLevel(TokenType tokenType) {
            if (TypeCobolTokenType.Contains(tokenType))
            {
                return CobolLanguageLevel.TypeCobol;
            }
            if (Cobol2002TokenType.Contains(tokenType))
            {
                return CobolLanguageLevel.Cobol2002;
            }
            return CobolLanguageLevel.Cobol85;
        }
    }
}