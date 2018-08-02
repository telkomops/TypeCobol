using System.Collections.Generic;
using System.Linq;
using System;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.Text;
using TypeCobol.Compiler.SqlParser;

%%
%class SqlScanner
%function GetToken
%namespace TypeCobol.Compiler.SqlScanner
%type SqlToken
%public
%eofval{
  return newToken(SqlSymbols.EOF, null);
%eofval}
%line
%char
%init{
		keywordMap = new Dictionary<string, int>
            {
                {"&&", SqlSymbols.SQL_KW_AND},
                {"add", SqlSymbols.SQL_KW_ADD},
                {"aggregate", SqlSymbols.SQL_KW_AGGREGATE},
                {"all", SqlSymbols.SQL_KW_ALL},
                {"alter", SqlSymbols.SQL_KW_ALTER},
                {"analytic", SqlSymbols.SQL_KW_ANALYTIC},
                {"and", SqlSymbols.SQL_OP_AND},
                {"anti", SqlSymbols.SQL_KW_ANTI},
                {"api_version", SqlSymbols.SQL_KW_API_VERSION},
                {"array", SqlSymbols.SQL_KW_ARRAY},
                {"as", SqlSymbols.SQL_KW_AS},
                {"asc", SqlSymbols.SQL_KW_ASC},
                {"avro", SqlSymbols.SQL_KW_AVRO},
                {"between", SqlSymbols.SQL_KW_BETWEEN},
                {"bigint", SqlSymbols.SQL_KW_BIGINT},
                {"binary", SqlSymbols.SQL_KW_BINARY},
                {"block_size", SqlSymbols.SQL_KW_BLOCKSIZE},
                {"boolean", SqlSymbols.SQL_KW_BOOLEAN},
                {"by", SqlSymbols.SQL_KW_BY},
                {"cached", SqlSymbols.SQL_KW_CACHED},
                {"case", SqlSymbols.SQL_KW_CASE},
                {"cascade", SqlSymbols.SQL_KW_CASCADE},
                {"cast", SqlSymbols.SQL_KW_CAST},
                {"change", SqlSymbols.SQL_KW_CHANGE},
                {"char", SqlSymbols.SQL_KW_CHAR},
                {"class", SqlSymbols.SQL_KW_CLASS},
                {"close_fn", SqlSymbols.SQL_KW_CLOSE_FN},
                {"column", SqlSymbols.SQL_KW_COLUMN},
                {"columns", SqlSymbols.SQL_KW_COLUMNS},
                {"comment", SqlSymbols.SQL_KW_COMMENT},
                {"compression", SqlSymbols.SQL_KW_COMPRESSION},
                {"compute", SqlSymbols.SQL_KW_COMPUTE},
                {"create", SqlSymbols.SQL_KW_CREATE},
                {"cross", SqlSymbols.SQL_KW_CROSS},
                {"current", SqlSymbols.SQL_KW_CURRENT},
                {"data", SqlSymbols.SQL_KW_DATA},
                {"database", SqlSymbols.SQL_KW_DATABASE},
                {"databases", SqlSymbols.SQL_KW_DATABASES},
                {"date", SqlSymbols.SQL_KW_DATE},
                {"datetime", SqlSymbols.SQL_KW_DATETIME},
                {"decimal", SqlSymbols.SQL_KW_DECIMAL},
                {"default", SqlSymbols.SQL_KW_DEFAULT},
                {"delete", SqlSymbols.SQL_KW_DELETE},
                {"delimited", SqlSymbols.SQL_KW_DELIMITED},
                {"desc", SqlSymbols.SQL_KW_DESC},
                {"describe", SqlSymbols.SQL_KW_DESCRIBE},
                {"distinct", SqlSymbols.SQL_KW_DISTINCT},
                {"div", SqlSymbols.SQL_KW_DIV},
                {"double", SqlSymbols.SQL_OP_DOUBLE},
                {"drop", SqlSymbols.SQL_KW_DROP},
                {"else", SqlSymbols.SQL_KW_ELSE},
                {"encoding", SqlSymbols.SQL_KW_ENCODING},
                {"end", SqlSymbols.SQL_KW_END},
                {"escaped", SqlSymbols.SQL_KW_ESCAPED},
                {"exists", SqlSymbols.SQL_KW_EXISTS},
                {"explain", SqlSymbols.SQL_KW_EXPLAIN},
                {"extended", SqlSymbols.SQL_KW_EXTENDED},
                {"external", SqlSymbols.SQL_KW_EXTERNAL},
                {"false", SqlSymbols.SQL_KW_FALSE},
                {"fields", SqlSymbols.SQL_KW_FIELDS},
                {"fileformat", SqlSymbols.SQL_KW_FILEFORMAT},
                {"files", SqlSymbols.SQL_KW_FILES},
                {"finalize_fn", SqlSymbols.SQL_KW_FINALIZE_FN},
                {"first", SqlSymbols.SQL_KW_FIRST},
                {"float", SqlSymbols.SQL_KW_FLOAT},
                {"following", SqlSymbols.SQL_KW_FOLLOWING},
                {"for", SqlSymbols.SQL_KW_FOR},
                {"format", SqlSymbols.SQL_KW_FORMAT},
                {"formatted", SqlSymbols.SQL_KW_FORMATTED},
                {"from", SqlSymbols.SQL_KW_FROM},
                {"full", SqlSymbols.SQL_KW_FULL},
                {"function", SqlSymbols.SQL_KW_FUNCTION},
                {"functions", SqlSymbols.SQL_KW_FUNCTIONS},
                {"grant", SqlSymbols.SQL_KW_GRANT},
                {"group", SqlSymbols.SQL_KW_GROUP},
                {"hash", SqlSymbols.SQL_KW_HASH},
                {"having", SqlSymbols.SQL_KW_HAVING},
                {"if", SqlSymbols.SQL_KW_IF},
                {"ilike", SqlSymbols.SQL_KW_ILIKE},
                {"ignore", SqlSymbols.SQL_KW_IGNORE},
                {"in", SqlSymbols.SQL_KW_IN},
                {"incremental", SqlSymbols.SQL_KW_INCREMENTAL},
                {"init_fn", SqlSymbols.SQL_KW_INIT_FN},
                {"inner", SqlSymbols.SQL_KW_INNER},
                {"inpath", SqlSymbols.SQL_KW_INPATH},
                {"insert", SqlSymbols.SQL_KW_INSERT},
                {"int", SqlSymbols.SQL_OP_INT},
                {"integer", SqlSymbols.SQL_KW_INT},
                {"intermediate", SqlSymbols.SQL_KW_INTERMEDIATE},
                {"interval", SqlSymbols.SQL_KW_INTERVAL},
                {"into", SqlSymbols.SQL_KW_INTO},
                {"invalidate", SqlSymbols.SQL_KW_INVALIDATE},
                {"iregexp", SqlSymbols.SQL_KW_IREGEXP},
                {"is", SqlSymbols.SQL_KW_IS},
                {"join", SqlSymbols.SQL_KW_JOIN},
                {"kudu", SqlSymbols.SQL_KW_KUDU},
                {"last", SqlSymbols.SQL_KW_LAST},
                {"left", SqlSymbols.SQL_KW_LEFT},
                {"like", SqlSymbols.SQL_KW_LIKE},
                {"limit", SqlSymbols.SQL_KW_LIMIT},
                {"lines", SqlSymbols.SQL_KW_LINES},
                {"load", SqlSymbols.SQL_KW_LOAD},
                {"location", SqlSymbols.SQL_KW_LOCATION},
                {"map", SqlSymbols.SQL_KW_MAP},
                {"merge_fn", SqlSymbols.SQL_KW_MERGE_FN},
                {"metadata", SqlSymbols.SQL_KW_METADATA},
                {"not", SqlSymbols.SQL_KW_NOT},
                {"null", SqlSymbols.SQL_KW_NULL},
                {"nulls", SqlSymbols.SQL_KW_NULLS},
                {"offset", SqlSymbols.SQL_KW_OFFSET},
                {"on", SqlSymbols.SQL_KW_ON},
                {"||", SqlSymbols.SQL_OP_OR},
                {"or", SqlSymbols.SQL_KW_OR},
                {"orc", SqlSymbols.SQL_KW_ORC},
                {"order", SqlSymbols.SQL_KW_ORDER},
                {"outer", SqlSymbols.SQL_KW_OUTER},
                {"over", SqlSymbols.SQL_KW_OVER},
                {"overwrite", SqlSymbols.SQL_KW_OVERWRITE},
                {"parquet", SqlSymbols.SQL_KW_PARQUET},
                {"parquetfile", SqlSymbols.SQL_KW_PARQUETFILE},
                {"partition", SqlSymbols.SQL_KW_PARTITION},
                {"partitioned", SqlSymbols.SQL_KW_PARTITIONED},
                {"partitions", SqlSymbols.SQL_KW_PARTITIONS},
                {"preceding", SqlSymbols.SQL_KW_PRECEDING},
                {"prepare_fn", SqlSymbols.SQL_KW_PREPARE_FN},
                {"primary", SqlSymbols.SQL_KW_PRIMARY},
                {"produced", SqlSymbols.SQL_KW_PRODUCED},
                {"purge", SqlSymbols.SQL_KW_PURGE},
                {"range", SqlSymbols.SQL_KW_RANGE},
                {"rcfile", SqlSymbols.SQL_KW_RCFILE},
                {"real", SqlSymbols.SQL_KW_DOUBLE},
                {"recover", SqlSymbols.SQL_KW_RECOVER},
                {"refresh", SqlSymbols.SQL_KW_REFRESH},
                {"regexp", SqlSymbols.SQL_KW_REGEXP},
                {"rename", SqlSymbols.SQL_KW_RENAME},
                {"repeatable", SqlSymbols.SQL_KW_REPEATABLE},
                {"replace", SqlSymbols.SQL_KW_REPLACE},
                {"replication", SqlSymbols.SQL_KW_REPLICATION},
                {"restrict", SqlSymbols.SQL_KW_RESTRICT},
                {"returns", SqlSymbols.SQL_KW_RETURNS},
                {"revoke", SqlSymbols.SQL_KW_REVOKE},
                {"right", SqlSymbols.SQL_KW_RIGHT},
                {"rlike", SqlSymbols.SQL_KW_RLIKE},
                {"role", SqlSymbols.SQL_KW_ROLE},
                {"roles", SqlSymbols.SQL_KW_ROLES},
                {"row", SqlSymbols.SQL_KW_ROW},
                {"rows", SqlSymbols.SQL_KW_ROWS},
                {"schema", SqlSymbols.SQL_KW_SCHEMA},
                {"schemas", SqlSymbols.SQL_KW_SCHEMAS},
                {"select", SqlSymbols.SQL_KW_SELECT},
                {"semi", SqlSymbols.SQL_KW_SEMI},
                {"sequencefile", SqlSymbols.SQL_KW_SEQUENCEFILE},
                {"serdeproperties", SqlSymbols.SQL_KW_SERDEPROPERTIES},
                {"serialize_fn", SqlSymbols.SQL_KW_SERIALIZE_FN},
                {"set", SqlSymbols.SQL_KW_SET},
                {"show", SqlSymbols.SQL_KW_SHOW},
                {"smallint", SqlSymbols.SQL_KW_SMALLINT},
                {"sort", SqlSymbols.SQL_KW_SORT},
                {"stats", SqlSymbols.SQL_KW_STATS},
                {"stored", SqlSymbols.SQL_KW_STORED},
                {"straight_join", SqlSymbols.SQL_KW_STRAIGHT_JOIN},
                {"string", SqlSymbols.SQL_KW_STRING},
                {"struct", SqlSymbols.SQL_KW_STRUCT},
                {"symbol", SqlSymbols.SQL_KW_SYMBOL},
                {"table", SqlSymbols.SQL_KW_TABLE},
                {"tables", SqlSymbols.SQL_KW_TABLES},
                {"tablesample", SqlSymbols.SQL_KW_TABLESAMPLE},
                {"tblproperties", SqlSymbols.SQL_KW_TBLPROPERTIES},
                {"terminated", SqlSymbols.SQL_KW_TERMINATED},
                {"textfile", SqlSymbols.SQL_KW_TEXTFILE},
                {"then", SqlSymbols.SQL_KW_THEN},
                {"timestamp", SqlSymbols.SQL_KW_TIMESTAMP},
                {"tinyint", SqlSymbols.SQL_KW_TINYINT},
                {"to", SqlSymbols.SQL_KW_TO},
                {"true", SqlSymbols.SQL_KW_TRUE},
                {"truncate", SqlSymbols.SQL_KW_TRUNCATE},
                {"unbounded", SqlSymbols.SQL_KW_UNBOUNDED},
                {"uncached", SqlSymbols.SQL_KW_UNCACHED},
                {"union", SqlSymbols.SQL_KW_UNION},
                {"unknown", SqlSymbols.SQL_KW_UNKNOWN},
                {"update", SqlSymbols.SQL_KW_UPDATE},
                {"update_fn", SqlSymbols.SQL_KW_UPDATE_FN},
                {"upsert", SqlSymbols.SQL_KW_UPSERT},
                {"use", SqlSymbols.SQL_KW_USE},
                {"using", SqlSymbols.SQL_KW_USING},
                {"values", SqlSymbols.SQL_KW_VALUES},
                {"varchar", SqlSymbols.SQL_KW_VARCHAR},
                {"view", SqlSymbols.SQL_KW_VIEW},
                {"when", SqlSymbols.SQL_KW_WHEN},
                {"where", SqlSymbols.SQL_KW_WHERE},
                {"with", SqlSymbols.SQL_KW_WITH}
            };
		tokenIdMap = new Dictionary<int,string>();
		foreach (var elem in keywordMap)
		{
			tokenIdMap.Add(elem.Value,elem.Key.ToUpperInvariant());
		}
		
		tokenIdMap.Add(SqlSymbols.EOF, "EOF");
		tokenIdMap.Add(SqlSymbols.SQL_DOTDOTDOT, "...");
		tokenIdMap.Add(SqlSymbols.SQL_OP_COLON, ":");
		tokenIdMap.Add(SqlSymbols.SQL_OP_SEMICOLON, ";");
		tokenIdMap.Add(SqlSymbols.SQL_OP_COMMA, "COMMA");
		tokenIdMap.Add(SqlSymbols.SQL_OP_DOT, ".");
		tokenIdMap.Add(SqlSymbols.SQL_OP_STAR, "*");
		tokenIdMap.Add(SqlSymbols.SQL_OP_LPAREN, "(");
		tokenIdMap.Add(SqlSymbols.SQL_OP_RPAREN, ")");
		tokenIdMap.Add(SqlSymbols.SQL_OP_LBRACKET, "[");
		tokenIdMap.Add(SqlSymbols.SQL_OP_RBRACKET, "]");
		tokenIdMap.Add(SqlSymbols.SQL_OP_DIVIDE, "/");
		tokenIdMap.Add(SqlSymbols.SQL_OP_MOD, "%");
		tokenIdMap.Add(SqlSymbols.SQL_OP_ADD, "+");
		tokenIdMap.Add(SqlSymbols.SQL_OP_SUBTRACT, "-");
		tokenIdMap.Add(SqlSymbols.SQL_OP_BITAND, "&");
		tokenIdMap.Add(SqlSymbols.SQL_OP_BITOR, "|");
		tokenIdMap.Add(SqlSymbols.SQL_OP_BITXOR, "^");
		tokenIdMap.Add(SqlSymbols.SQL_OP_BITNOT, "~");
		tokenIdMap.Add(SqlSymbols.SQL_OP_EQUAL, "=");
		tokenIdMap.Add(SqlSymbols.SQL_OP_NOT, "!");
		tokenIdMap.Add(SqlSymbols.SQL_OP_LESSTHAN, "<");
		tokenIdMap.Add(SqlSymbols.SQL_OP_GREATERTHAN, ">");
		tokenIdMap.Add(SqlSymbols.SQL_UNMATCHED_STRING_LITERAL, "UNMATCHED STRING LITERAL");
		tokenIdMap.Add(SqlSymbols.SQL_OP_NOTEQUAL, "!=");
		tokenIdMap.Add(SqlSymbols.SQL_INTEGER_LITERAL, "INTEGER LITERAL");
		tokenIdMap.Add(SqlSymbols.SQL_NUMERIC_OVERFLOW, "NUMERIC OVERFLOW");
		tokenIdMap.Add(SqlSymbols.SQL_DECIMAL_LITERAL, "DECIMAL LITERAL");
		tokenIdMap.Add(SqlSymbols.SQL_EMPTY_IDENT, "EMPTY IDENTIFIER");
		tokenIdMap.Add(SqlSymbols.SQL_OP_IDENT, "IDENTIFIER");
		tokenIdMap.Add(SqlSymbols.SQL_STRING_LITERAL, "STRING LITERAL");
		tokenIdMap.Add(SqlSymbols.SQL_COMMENTED_PLAN_HINT_START,
			"COMMENTED_PLAN_HINT_START");
		tokenIdMap.Add(SqlSymbols.SQL_COMMENTED_PLAN_HINT_END, "COMMENTED_PLAN_HINT_END");
		tokenIdMap.Add(SqlSymbols.SQL_UNEXPECTED_CHAR, "Unexpected character");
		
		reservedWords = new List<string>();
		reservedWords.AddRange(keywordMap.Keys.ToArray());
		
		reservedWords.AddRange(new string[]{
			"abs", "acos", "allocate", "any", "are", "array_agg", "array_max_cardinality",
			"asensitive", "asin", "asymmetric", "at", "atan", "atomic", "authorization",
			"avg", "begin", "begin_frame", "begin_partition", "blob", "both", "call",
			"called", "cardinality", "cascaded", "ceil", "ceiling", "char_length",
			"character", "character_length", "check", "classifier", "clob", "close",
			"coalesce", "collate", "collect", "commit", "condition", "connect", "constraint",
			"contains", "convert", "copy", "corr", "corresponding", "cos", "cosh", "count",
			"covar_pop", "covar_samp", "cube", "cume_dist", "current_catalog", "current_date",
			"current_default_transform_group", "current_path", "current_path", "current_role",
			"current_role", "current_row", "current_schema", "current_time",
			"current_timestamp", "current_transform_group_for_type", "current_user", "cursor",
			"cycle", "day", "deallocate", "dec", "decfloat", "declare", "define",
			"dense_rank", "deref", "deterministic", "disconnect", "dynamic", "each",
			"element", "empty", "end-exec", "end_frame", "end_partition", "equals", "escape",
			"every", "except", "exec", "execute", "exp", "extract", "fetch", "filter",
			"first_value", "floor", "foreign", "frame_row", "free", "fusion", "get", "global",
			"grouping", "groups", "hold", "hour", "identity", "indicator", "initial", "inout",
			"insensitive", "integer", "intersect", "intersection", "json_array",
			"json_arrayagg", "json_exists", "json_object", "json_objectagg", "json_query",
			"json_table", "json_table_primitive", "json_value", "lag", "language", "large",
			"last_value", "lateral", "lead", "leading", "like_regex", "listagg", "ln",
			"local", "localtime", "localtimestamp", "log", "log10 ", "lower", "match",
			"match_number", "match_recognize", "matches", "max", "member", "merge", "method",
			"min", "minute", "mod", "modifies", "module", "month", "multiset", "national",
			"natural", "nchar", "nclob", "new", "no", "none", "normalize", "nth_value",
			"ntile", "nullif", "numeric", "occurrences_regex", "octet_length", "of", "old",
			"omit", "one", "only", "open", "out", "overlaps", "overlay", "parameter",
			"pattern", "per", "percent", "percent_rank", "percentile_cont", "percentile_disc",
			"period", "portion", "position", "position_regex", "power", "precedes",
			"precision", "prepare", "procedure", "ptf", "rank", "reads", "real", "recursive",
			"ref", "references", "referencing", "regr_avgx", "regr_avgy", "regr_count",
			"regr_intercept", "regr_r2", "regr_slope", "regr_sxx", "regr_sxy", "regr_syy",
			"release", "result", "return", "rollback", "rollup", "row_number", "running",
			"savepoint", "scope", "scroll", "search", "second", "seek", "sensitive",
			"session_user", "similar", "sin", "sinh", "skip", "some", "specific",
			"specifictype", "sql", "sqlexception", "sqlstate", "sqlwarning", "sqrt", "start",
			"static", "stddev_pop", "stddev_samp", "submultiset", "subset", "substring",
			"substring_regex", "succeeds", "sum", "symmetric", "system", "system_time",
			"system_user", "tan", "tanh", "time", "timezone_hour", "timezone_minute",
			"trailing", "translate", "translate_regex", "translation", "treat", "trigger",
			"trim", "trim_array", "uescape", "unique", "unknown", "unnest", "update  ",
			"upper", "user", "value", "value_of", "var_pop", "var_samp", "varbinary",
			"varying", "versioning", "whenever", "width_bucket", "window", "within",
			"without", "year"
		});
		
		reservedWords.RemoveAll(elem =>
            {
                var matchedValued = new string[]
                {
                    "year", "month", "day", "hour", "minute", "second",
                    "begin", "call", "check", "classifier", "close", "identity", "language",
                    "localtime", "member", "module", "new", "nullif", "old", "open", "parameter",
                    "period", "result", "return", "sql", "start", "system", "time", "user", "value"
                };
                return matchedValued.Select(values => elem.Equals(values)).FirstOrDefault();
            } );
%init}

%{
// Map from keyword string to token id.
static Dictionary<string,int> keywordMap;
static List<string> reservedWords;
static Dictionary<int,string> tokenIdMap;

	static bool isReserved(string token)
    {
            int value;
            return !string.IsNullOrEmpty(token) && keywordMap.TryGetValue(token.ToLower(),out value);
    }
        static bool isKeyword(SqlSymbols tokenId)
        {
            int value;
            return keywordMap.TryGetValue(tokenId.ToString().ToLower(),out value);
        }
	
	private SqlToken newToken(int type, object value) {
		int startIndex = yychar;
        int endIndex;
        if (type == SqlSymbols.SQL_COMMENTED_PLAN_HINT_END || type == SqlSymbols.SQL_COMMENTED_PLAN_HINT_START ||
            type == SqlSymbols.SQL_OP_NOTEQUAL)
        {
            endIndex = startIndex + 2;
        }
        else
        {
            if (value != null)
            {
			    if (type == SqlSymbols.SQL_STRING_LITERAL)
                    startIndex++;
                endIndex = startIndex + value.ToString().Length - 1;
            }
            else
            {
                endIndex = startIndex + 1;
            }
        }
        
        ITextLine textLine = new TextLineSnapshot(yyline,new string(yy_buffer).Trim('\0'),null);
        ITokensLine line = new TokensLine(textLine,ColumnsLayout.FreeTextFormat);
        return new SqlToken((TokenType)((int)type+550), startIndex, endIndex, line ,yyline);
	}
%}

LineTerminator = \r|\n|\r\n
Whitespace = " "
lineEnd = \t
CarrigeReturn = {LineTerminator}|{lineEnd}

IntegerLiteral = [\d][\d]*
FLit1 = [0-9]+\.[0-9]*
FLit2 = \.[0-9]+
FLit3 = [0-9]+
Exponent = [eE] [+-]? [0-9]+
DecimalLiteral = ({FLit1}|{FLit2}|{FLit3}){Exponent}?

IdentifierOrKw = [0-9]*[:*a-zA-Z0-9^\s]*|"&&"|"||"
QuotedIdentifier = \`(\\.|[^\\\`])*\`
SingleQuoteStringLiteral = \'(\\.|[^\\\'])*\'
DoubleQuoteStringLiteral = \"(\\.|[^\\\"])*\"

EolHintBegin = "--" " "*
CommentedHintBegin = "/*" " "*
CommentedHintEnd = "*/"

HintContent = (" "*"+"[^\r\n]*)

Comment = {TraditionalComment}|{EndOfLineComment}
ContainsCommentEnd = ([^]*"*/"[^]*)
ContainsLineTerminator = ([^]*{CarrigeReturn}[^]*)
TraditionalComment = ("/*"!({HintContent}|{ContainsCommentEnd})"*/")
EndOfLineComment = "--" ![\r\n]*

%state EOLHINT

%%
"..." { return newToken(SqlSymbols.SQL_DOTDOTDOT, "..."); }
":" { return newToken(SqlSymbols.SQL_OP_COLON, ":"); }
";" { return newToken(SqlSymbols.SQL_OP_SEMICOLON, ";"); }
"," { return newToken(SqlSymbols.SQL_OP_COMMA, ","); }
"." { return newToken(SqlSymbols.SQL_OP_DOT, "."); }
"*" { return newToken(SqlSymbols.SQL_OP_STAR, "*"); }
"(" { return newToken(SqlSymbols.SQL_OP_LPAREN, "("); }
")" { return newToken(SqlSymbols.SQL_OP_RPAREN, ")"); }
"[" { return newToken(SqlSymbols.SQL_OP_LBRACKET, "["); }
"]" { return newToken(SqlSymbols.SQL_OP_RBRACKET, "]"); }
"/" { return newToken(SqlSymbols.SQL_OP_DIVIDE, "/"); }
"%" { return newToken(SqlSymbols.SQL_OP_MOD, "%"); }
"+" { return newToken(SqlSymbols.SQL_OP_ADD, "+"); }
"-" { return newToken(SqlSymbols.SQL_OP_SUBTRACT, "-"); }
"&" { return newToken(SqlSymbols.SQL_OP_BITAND, "&"); }
"|" { return newToken(SqlSymbols.SQL_OP_BITOR, "|"); }
"^" { return newToken(SqlSymbols.SQL_OP_BITXOR, "^"); }
"~" { return newToken(SqlSymbols.SQL_OP_BITNOT, "~"); }
"=" { return newToken(SqlSymbols.SQL_OP_EQUAL, "="); }
"!" { return newToken(SqlSymbols.SQL_OP_NOT, "!"); }
"<" { return newToken(SqlSymbols.SQL_OP_LESSTHAN, "<"); }
">" { return newToken(SqlSymbols.SQL_OP_GREATERTHAN, ">"); }
"\"" { return newToken(SqlSymbols.SQL_UNMATCHED_STRING_LITERAL, "\""); }
"'" { return newToken(SqlSymbols.SQL_UNMATCHED_STRING_LITERAL, "'"); }
"`" { return newToken(SqlSymbols.SQL_UNMATCHED_STRING_LITERAL, "`"); }
"!=" { return newToken(SqlSymbols.SQL_OP_NOTEQUAL, "!="); }

{IntegerLiteral} {
  try {
    return newToken(SqlSymbols.SQL_INTEGER_LITERAL, Decimal.Parse(yytext()));
  } catch (FormatException) {
    return newToken(SqlSymbols.SQL_NUMERIC_OVERFLOW, yytext());
  }
}

{DecimalLiteral} {
  try {
    return newToken(SqlSymbols.SQL_DECIMAL_LITERAL, Decimal.Parse(yytext()));
  } catch (FormatException) {
    return newToken(SqlSymbols.SQL_NUMERIC_OVERFLOW, yytext());
  }
}

{QuotedIdentifier} {
  // Remove the quotes and trim whitespace.
  string trimmedIdent = yytext().Replace("\"","").Replace("'","").Trim();
  if (string.IsNullOrEmpty(trimmedIdent)) {
    return newToken(SqlSymbols.SQL_EMPTY_IDENT, yytext());
  }
  return newToken(SqlSymbols.SQL_OP_IDENT, trimmedIdent);
}

{IdentifierOrKw} {
  string text = yytext().Trim();
  int SQL_KW_id;
  if (keywordMap.TryGetValue(text.ToLower(),out SQL_KW_id)) {
    return newToken(SQL_KW_id, text);
  } else if (isReserved(text)) {
    return newToken(SqlSymbols.SQL_UNUSED_RESERVED_WORD, text);
  } else {
    return newToken(SqlSymbols.SQL_OP_IDENT, text);
  }
}

{SingleQuoteStringLiteral} {
  return newToken(SqlSymbols.SQL_STRING_LITERAL, yytext().Substring(1, yytext().Length-2));
}

{DoubleQuoteStringLiteral} {
  return newToken(SqlSymbols.SQL_STRING_LITERAL, yytext().Substring(1, yytext().Length-2));
}

{CommentedHintBegin} {
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_START, null);
}

{CommentedHintEnd} {
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_END, null);
}

{EolHintBegin} {
  yybegin(EOLHINT);
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_START, null);
}

<EOLHINT> {LineTerminator} {
  yybegin(YYINITIAL);
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_END, null);
}

{Whitespace} { return null; }
{Comment} { return null; }
{CarrigeReturn} { return null; }
{EndOfLineComment} { return null; }
[^] { return newToken(SqlSymbols.SQL_UNEXPECTED_CHAR, yytext()); }