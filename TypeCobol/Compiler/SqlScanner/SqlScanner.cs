namespace TypeCobol.Compiler.SqlScanner
{
using System.Collections.Generic;
using System.Linq;
using System;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.Text;
using TypeCobol.Compiler.SqlParser;
/* test */


public partial class SqlScanner
{
private const int YY_BUFFER_SIZE = 512;
private const int YY_F = -1;
private const int YY_NO_STATE = -1;
private const int YY_NOT_ACCEPT = 0;
private const int YY_START = 1;
private const int YY_END = 2;
private const int YY_NO_ANCHOR = 4;
delegate SqlToken AcceptMethod();
AcceptMethod[] accept_dispatch;
private const int YY_BOL = 128;
private const int YY_EOF = 129;

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
private System.IO.TextReader yy_reader;
private int yy_buffer_index;
private int yy_buffer_read;
private int yy_buffer_start;
private int yy_buffer_end;
private char[] yy_buffer;
private int yychar;
private int yyline;
private bool yy_at_bol;
private int yy_lexical_state;

public SqlScanner(System.IO.TextReader reader) : this()
  {
  if (null == reader)
    {
    throw new System.ApplicationException("Error: Bad input stream initializer.");
    }
  yy_reader = reader;
  }

public SqlScanner(System.IO.FileStream instream) : this()
  {
  if (null == instream)
    {
    throw new System.ApplicationException("Error: Bad input stream initializer.");
    }
  yy_reader = new System.IO.StreamReader(instream);
  }

private SqlScanner()
  {
  yy_buffer = new char[YY_BUFFER_SIZE];
  yy_buffer_read = 0;
  yy_buffer_index = 0;
  yy_buffer_start = 0;
  yy_buffer_end = 0;
  yychar = 0;
  yyline = 0;
  yy_at_bol = true;
  yy_lexical_state = YYINITIAL;
accept_dispatch = new AcceptMethod[] 
 {
  new AcceptMethod(this.Accept_0),
  null,
  new AcceptMethod(this.Accept_2),
  new AcceptMethod(this.Accept_3),
  new AcceptMethod(this.Accept_4),
  new AcceptMethod(this.Accept_5),
  new AcceptMethod(this.Accept_6),
  new AcceptMethod(this.Accept_7),
  new AcceptMethod(this.Accept_8),
  new AcceptMethod(this.Accept_9),
  new AcceptMethod(this.Accept_10),
  new AcceptMethod(this.Accept_11),
  new AcceptMethod(this.Accept_12),
  new AcceptMethod(this.Accept_13),
  new AcceptMethod(this.Accept_14),
  new AcceptMethod(this.Accept_15),
  new AcceptMethod(this.Accept_16),
  new AcceptMethod(this.Accept_17),
  new AcceptMethod(this.Accept_18),
  new AcceptMethod(this.Accept_19),
  new AcceptMethod(this.Accept_20),
  new AcceptMethod(this.Accept_21),
  new AcceptMethod(this.Accept_22),
  new AcceptMethod(this.Accept_23),
  new AcceptMethod(this.Accept_24),
  new AcceptMethod(this.Accept_25),
  new AcceptMethod(this.Accept_26),
  new AcceptMethod(this.Accept_27),
  new AcceptMethod(this.Accept_28),
  new AcceptMethod(this.Accept_29),
  new AcceptMethod(this.Accept_30),
  null,
  new AcceptMethod(this.Accept_32),
  new AcceptMethod(this.Accept_33),
  new AcceptMethod(this.Accept_34),
  new AcceptMethod(this.Accept_35),
  new AcceptMethod(this.Accept_36),
  new AcceptMethod(this.Accept_37),
  new AcceptMethod(this.Accept_38),
  new AcceptMethod(this.Accept_39),
  new AcceptMethod(this.Accept_40),
  new AcceptMethod(this.Accept_41),
  new AcceptMethod(this.Accept_42),
  new AcceptMethod(this.Accept_43),
  new AcceptMethod(this.Accept_44),
  null,
  new AcceptMethod(this.Accept_46),
  new AcceptMethod(this.Accept_47),
  new AcceptMethod(this.Accept_48),
  null,
  new AcceptMethod(this.Accept_50),
  new AcceptMethod(this.Accept_51),
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  };

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
  }

SqlToken Accept_0()
    { // begin accept action #0
{
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
    } // end accept action #0

SqlToken Accept_2()
    { // begin accept action #2
{ return newToken(SqlSymbols.SQL_OP_DOT, "."); }
    } // end accept action #2

SqlToken Accept_3()
    { // begin accept action #3
{ return newToken(SqlSymbols.SQL_OP_COLON, ":"); }
    } // end accept action #3

SqlToken Accept_4()
    { // begin accept action #4
{ return newToken(SqlSymbols.SQL_OP_SEMICOLON, ";"); }
    } // end accept action #4

SqlToken Accept_5()
    { // begin accept action #5
{ return newToken(SqlSymbols.SQL_OP_COMMA, ","); }
    } // end accept action #5

SqlToken Accept_6()
    { // begin accept action #6
{ return newToken(SqlSymbols.SQL_OP_STAR, "*"); }
    } // end accept action #6

SqlToken Accept_7()
    { // begin accept action #7
{ return newToken(SqlSymbols.SQL_OP_LPAREN, "("); }
    } // end accept action #7

SqlToken Accept_8()
    { // begin accept action #8
{ return newToken(SqlSymbols.SQL_OP_RPAREN, ")"); }
    } // end accept action #8

SqlToken Accept_9()
    { // begin accept action #9
{ return newToken(SqlSymbols.SQL_OP_LBRACKET, "["); }
    } // end accept action #9

SqlToken Accept_10()
    { // begin accept action #10
{ return newToken(SqlSymbols.SQL_OP_RBRACKET, "]"); }
    } // end accept action #10

SqlToken Accept_11()
    { // begin accept action #11
{ return newToken(SqlSymbols.SQL_OP_DIVIDE, "/"); }
    } // end accept action #11

SqlToken Accept_12()
    { // begin accept action #12
{ return newToken(SqlSymbols.SQL_OP_MOD, "%"); }
    } // end accept action #12

SqlToken Accept_13()
    { // begin accept action #13
{ return newToken(SqlSymbols.SQL_OP_ADD, "+"); }
    } // end accept action #13

SqlToken Accept_14()
    { // begin accept action #14
{ return newToken(SqlSymbols.SQL_OP_SUBTRACT, "-"); }
    } // end accept action #14

SqlToken Accept_15()
    { // begin accept action #15
{ return newToken(SqlSymbols.SQL_OP_BITAND, "&"); }
    } // end accept action #15

SqlToken Accept_16()
    { // begin accept action #16
{ return newToken(SqlSymbols.SQL_OP_BITOR, "|"); }
    } // end accept action #16

SqlToken Accept_17()
    { // begin accept action #17
{ return newToken(SqlSymbols.SQL_OP_BITXOR, "^"); }
    } // end accept action #17

SqlToken Accept_18()
    { // begin accept action #18
{ return newToken(SqlSymbols.SQL_OP_BITNOT, "~"); }
    } // end accept action #18

SqlToken Accept_19()
    { // begin accept action #19
{ return newToken(SqlSymbols.SQL_OP_EQUAL, "="); }
    } // end accept action #19

SqlToken Accept_20()
    { // begin accept action #20
{ return newToken(SqlSymbols.SQL_OP_NOT, "!"); }
    } // end accept action #20

SqlToken Accept_21()
    { // begin accept action #21
{ return newToken(SqlSymbols.SQL_OP_LESSTHAN, "<"); }
    } // end accept action #21

SqlToken Accept_22()
    { // begin accept action #22
{ return newToken(SqlSymbols.SQL_OP_GREATERTHAN, ">"); }
    } // end accept action #22

SqlToken Accept_23()
    { // begin accept action #23
{ return newToken(SqlSymbols.SQL_UNMATCHED_STRING_LITERAL, "\""); }
    } // end accept action #23

SqlToken Accept_24()
    { // begin accept action #24
{ return newToken(SqlSymbols.SQL_UNMATCHED_STRING_LITERAL, "'"); }
    } // end accept action #24

SqlToken Accept_25()
    { // begin accept action #25
{ return newToken(SqlSymbols.SQL_UNMATCHED_STRING_LITERAL, "`"); }
    } // end accept action #25

SqlToken Accept_26()
    { // begin accept action #26
{
  try {
    return newToken(SqlSymbols.SQL_INTEGER_LITERAL, Decimal.Parse(yytext()));
  } catch (FormatException) {
    return newToken(SqlSymbols.SQL_NUMERIC_OVERFLOW, yytext());
  }
}
    } // end accept action #26

SqlToken Accept_27()
    { // begin accept action #27
{
  try {
    return newToken(SqlSymbols.SQL_DECIMAL_LITERAL, Decimal.Parse(yytext()));
  } catch (FormatException) {
    return newToken(SqlSymbols.SQL_NUMERIC_OVERFLOW, yytext());
  }
}
    } // end accept action #27

SqlToken Accept_28()
    { // begin accept action #28
{ return newToken(SqlSymbols.SQL_UNEXPECTED_CHAR, yytext()); }
    } // end accept action #28

SqlToken Accept_29()
    { // begin accept action #29
{ return null; }
    } // end accept action #29

SqlToken Accept_30()
    { // begin accept action #30
{ return null; }
    } // end accept action #30

SqlToken Accept_32()
    { // begin accept action #32
{
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_END, null);
}
    } // end accept action #32

SqlToken Accept_33()
    { // begin accept action #33
{
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_START, null);
}
    } // end accept action #33

SqlToken Accept_34()
    { // begin accept action #34
{
  yybegin(EOLHINT);
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_START, null);
}
    } // end accept action #34

SqlToken Accept_35()
    { // begin accept action #35
{ return newToken(SqlSymbols.SQL_OP_NOTEQUAL, "!="); }
    } // end accept action #35

SqlToken Accept_36()
    { // begin accept action #36
{
  return newToken(SqlSymbols.SQL_STRING_LITERAL, yytext().Substring(1, yytext().Length-2));
}
    } // end accept action #36

SqlToken Accept_37()
    { // begin accept action #37
{
  return newToken(SqlSymbols.SQL_STRING_LITERAL, yytext().Substring(1, yytext().Length-2));
}
    } // end accept action #37

SqlToken Accept_38()
    { // begin accept action #38
{
  // Remove the quotes and trim whitespace.
  string trimmedIdent = yytext().Replace("\"","").Replace("'","").Trim();
  if (string.IsNullOrEmpty(trimmedIdent)) {
    return newToken(SqlSymbols.SQL_EMPTY_IDENT, yytext());
  }
  return newToken(SqlSymbols.SQL_OP_IDENT, trimmedIdent);
}
    } // end accept action #38

SqlToken Accept_39()
    { // begin accept action #39
{ return newToken(SqlSymbols.SQL_DOTDOTDOT, "..."); }
    } // end accept action #39

SqlToken Accept_40()
    { // begin accept action #40
{ return null; }
    } // end accept action #40

SqlToken Accept_41()
    { // begin accept action #41
{
  yybegin(YYINITIAL);
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_END, null);
}
    } // end accept action #41

SqlToken Accept_42()
    { // begin accept action #42
{
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
    } // end accept action #42

SqlToken Accept_43()
    { // begin accept action #43
{
  try {
    return newToken(SqlSymbols.SQL_DECIMAL_LITERAL, Decimal.Parse(yytext()));
  } catch (FormatException) {
    return newToken(SqlSymbols.SQL_NUMERIC_OVERFLOW, yytext());
  }
}
    } // end accept action #43

SqlToken Accept_44()
    { // begin accept action #44
{ return null; }
    } // end accept action #44

SqlToken Accept_46()
    { // begin accept action #46
{
  yybegin(YYINITIAL);
  return newToken(SqlSymbols.SQL_COMMENTED_PLAN_HINT_END, null);
}
    } // end accept action #46

SqlToken Accept_47()
    { // begin accept action #47
{
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
    } // end accept action #47

SqlToken Accept_48()
    { // begin accept action #48
{
  try {
    return newToken(SqlSymbols.SQL_DECIMAL_LITERAL, Decimal.Parse(yytext()));
  } catch (FormatException) {
    return newToken(SqlSymbols.SQL_NUMERIC_OVERFLOW, yytext());
  }
}
    } // end accept action #48

SqlToken Accept_50()
    { // begin accept action #50
{
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
    } // end accept action #50

SqlToken Accept_51()
    { // begin accept action #51
{
  try {
    return newToken(SqlSymbols.SQL_DECIMAL_LITERAL, Decimal.Parse(yytext()));
  } catch (FormatException) {
    return newToken(SqlSymbols.SQL_NUMERIC_OVERFLOW, yytext());
  }
}
    } // end accept action #51

private const int EOLHINT = 1;
private const int YYINITIAL = 0;
private static int[] yy_state_dtrans = new int[] 
  {   0,
  50
  };
private void yybegin (int state)
  {
  yy_lexical_state = state;
  }

private char yy_advance ()
  {
  int next_read;
  int i;
  int j;

  if (yy_buffer_index < yy_buffer_read)
    {
    return yy_buffer[yy_buffer_index++];
    }

  if (0 != yy_buffer_start)
    {
    i = yy_buffer_start;
    j = 0;
    while (i < yy_buffer_read)
      {
      yy_buffer[j] = yy_buffer[i];
      i++;
      j++;
      }
    yy_buffer_end = yy_buffer_end - yy_buffer_start;
    yy_buffer_start = 0;
    yy_buffer_read = j;
    yy_buffer_index = j;
    next_read = yy_reader.Read(yy_buffer,yy_buffer_read,
                  yy_buffer.Length - yy_buffer_read);
    if (next_read <= 0)
      {
      return (char) YY_EOF;
      }
    yy_buffer_read = yy_buffer_read + next_read;
    }
  while (yy_buffer_index >= yy_buffer_read)
    {
    if (yy_buffer_index >= yy_buffer.Length)
      {
      yy_buffer = yy_double(yy_buffer);
      }
    next_read = yy_reader.Read(yy_buffer,yy_buffer_read,
                  yy_buffer.Length - yy_buffer_read);
    if (next_read <= 0)
      {
      return (char) YY_EOF;
      }
    yy_buffer_read = yy_buffer_read + next_read;
    }
  return yy_buffer[yy_buffer_index++];
  }
private void yy_move_end ()
  {
  if (yy_buffer_end > yy_buffer_start && 
      '\n' == yy_buffer[yy_buffer_end-1])
    yy_buffer_end--;
  if (yy_buffer_end > yy_buffer_start &&
      '\r' == yy_buffer[yy_buffer_end-1])
    yy_buffer_end--;
  }
private bool yy_last_was_cr=false;
private void yy_mark_start ()
  {
  int i;
  for (i = yy_buffer_start; i < yy_buffer_index; i++)
    {
    if (yy_buffer[i] == '\n' && !yy_last_was_cr)
      {
      yyline++;
      }
    if (yy_buffer[i] == '\r')
      {
      yyline++;
      yy_last_was_cr=true;
      }
    else
      {
      yy_last_was_cr=false;
      }
    }
  yychar = yychar + yy_buffer_index - yy_buffer_start;
  yy_buffer_start = yy_buffer_index;
  }
private void yy_mark_end ()
  {
  yy_buffer_end = yy_buffer_index;
  }
private void yy_to_mark ()
  {
  yy_buffer_index = yy_buffer_end;
  yy_at_bol = (yy_buffer_end > yy_buffer_start) &&
    (yy_buffer[yy_buffer_end-1] == '\r' ||
    yy_buffer[yy_buffer_end-1] == '\n');
  }
internal string yytext()
  {
  return (new string(yy_buffer,
                yy_buffer_start,
                yy_buffer_end - yy_buffer_start)
         );
  }
private int yylength ()
  {
  return yy_buffer_end - yy_buffer_start;
  }
private char[] yy_double (char[] buf)
  {
  int i;
  char[] newbuf;
  newbuf = new char[2*buf.Length];
  for (i = 0; i < buf.Length; i++)
    {
    newbuf[i] = buf[i];
    }
  return newbuf;
  }
private const int YY_E_INTERNAL = 0;
private const int YY_E_MATCH = 1;
private static string[] yy_error_string = new string[]
  {
  "Error: Internal error.\n",
  "Error: Unmatched input.\n"
  };
private void yy_error (int code,bool fatal)
  {
  System.Console.Write(yy_error_string[code]);
  if (fatal)
    {
    throw new System.ApplicationException("Fatal Error.\n");
    }
  }
private static int[] yy_acpt = new int[]
  {
  /* 0 */   YY_NO_ANCHOR,
  /* 1 */   YY_NO_ANCHOR,
  /* 2 */   YY_NO_ANCHOR,
  /* 3 */   YY_NO_ANCHOR,
  /* 4 */   YY_NO_ANCHOR,
  /* 5 */   YY_NO_ANCHOR,
  /* 6 */   YY_NO_ANCHOR,
  /* 7 */   YY_NO_ANCHOR,
  /* 8 */   YY_NO_ANCHOR,
  /* 9 */   YY_NO_ANCHOR,
  /* 10 */   YY_NO_ANCHOR,
  /* 11 */   YY_NO_ANCHOR,
  /* 12 */   YY_NO_ANCHOR,
  /* 13 */   YY_NO_ANCHOR,
  /* 14 */   YY_NO_ANCHOR,
  /* 15 */   YY_NO_ANCHOR,
  /* 16 */   YY_NO_ANCHOR,
  /* 17 */   YY_NO_ANCHOR,
  /* 18 */   YY_NO_ANCHOR,
  /* 19 */   YY_NO_ANCHOR,
  /* 20 */   YY_NO_ANCHOR,
  /* 21 */   YY_NO_ANCHOR,
  /* 22 */   YY_NO_ANCHOR,
  /* 23 */   YY_NO_ANCHOR,
  /* 24 */   YY_NO_ANCHOR,
  /* 25 */   YY_NO_ANCHOR,
  /* 26 */   YY_NO_ANCHOR,
  /* 27 */   YY_NO_ANCHOR,
  /* 28 */   YY_NO_ANCHOR,
  /* 29 */   YY_NO_ANCHOR,
  /* 30 */   YY_NO_ANCHOR,
  /* 31 */   YY_NOT_ACCEPT,
  /* 32 */   YY_NO_ANCHOR,
  /* 33 */   YY_NO_ANCHOR,
  /* 34 */   YY_NO_ANCHOR,
  /* 35 */   YY_NO_ANCHOR,
  /* 36 */   YY_NO_ANCHOR,
  /* 37 */   YY_NO_ANCHOR,
  /* 38 */   YY_NO_ANCHOR,
  /* 39 */   YY_NO_ANCHOR,
  /* 40 */   YY_NO_ANCHOR,
  /* 41 */   YY_NO_ANCHOR,
  /* 42 */   YY_NO_ANCHOR,
  /* 43 */   YY_NO_ANCHOR,
  /* 44 */   YY_NO_ANCHOR,
  /* 45 */   YY_NOT_ACCEPT,
  /* 46 */   YY_NO_ANCHOR,
  /* 47 */   YY_NO_ANCHOR,
  /* 48 */   YY_NO_ANCHOR,
  /* 49 */   YY_NOT_ACCEPT,
  /* 50 */   YY_NO_ANCHOR,
  /* 51 */   YY_NO_ANCHOR,
  /* 52 */   YY_NOT_ACCEPT,
  /* 53 */   YY_NOT_ACCEPT,
  /* 54 */   YY_NOT_ACCEPT,
  /* 55 */   YY_NOT_ACCEPT,
  /* 56 */   YY_NOT_ACCEPT,
  /* 57 */   YY_NOT_ACCEPT,
  /* 58 */   YY_NOT_ACCEPT,
  /* 59 */   YY_NOT_ACCEPT,
  /* 60 */   YY_NOT_ACCEPT,
  /* 61 */   YY_NOT_ACCEPT,
  /* 62 */   YY_NOT_ACCEPT
  };
private static int[] yy_cmap = new int[]
  {
  29, 29, 29, 29, 29, 29, 29, 29,
  29, 34, 30, 29, 29, 32, 29, 29,
  29, 29, 29, 29, 29, 29, 29, 29,
  29, 29, 29, 29, 29, 29, 29, 29,
  33, 19, 22, 29, 29, 11, 14, 23,
  6, 7, 5, 12, 4, 13, 1, 10,
  26, 26, 26, 26, 26, 26, 26, 26,
  26, 26, 2, 3, 20, 18, 21, 29,
  29, 31, 31, 31, 31, 27, 31, 31,
  31, 31, 31, 31, 31, 31, 31, 31,
  31, 31, 31, 31, 31, 31, 31, 31,
  31, 31, 31, 8, 28, 9, 16, 29,
  24, 31, 31, 31, 25, 27, 31, 31,
  31, 31, 31, 31, 31, 31, 31, 31,
  31, 31, 31, 31, 31, 31, 31, 31,
  31, 31, 31, 29, 15, 29, 17, 29,
  0, 0 
  };
private static int[] yy_rmap = new int[]
  {
  0, 1, 2, 3, 1, 1, 4, 1,
  1, 1, 1, 5, 1, 1, 6, 7,
  8, 3, 1, 1, 9, 1, 1, 10,
  11, 12, 13, 14, 1, 1, 1, 15,
  1, 16, 1, 1, 1, 1, 1, 1,
  17, 1, 3, 18, 19, 10, 20, 1,
  3, 21, 22, 1, 11, 23, 12, 24,
  25, 26, 27, 28, 17, 29, 30 
  };
private static int[,] yy_nxt = new int[,]
  {
  { 1, 2, 3, 4, 5, 6, 7, 8,
   9, 10, 11, 12, 13, 14, 15, 16,
   17, 18, 19, 20, 21, 22, 23, 24,
   25, 26, 27, 42, 28, 28, 29, 42,
   44, 30, 29 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, 31, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, 43, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, -1, 42, -1, -1, 42, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   42, -1, -1, -1, -1, -1, -1, -1,
   -1, 42, 42, 42, -1, -1, -1, 42,
   -1, -1, -1 },
  { -1, -1, 42, -1, -1, 42, -1, -1,
   -1, -1, 32, -1, -1, -1, -1, -1,
   42, -1, -1, -1, -1, -1, -1, -1,
   -1, 42, 42, 42, -1, -1, -1, 42,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, 33, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, 34, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, 47, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, 47,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, 35, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, 45, 45, 45, 45, 45, 45, 45,
   45, 45, 45, 45, 45, 45, 45, 45,
   45, 45, 45, 45, 45, 45, 36, 45,
   45, 45, 45, 45, 49, 45, 45, 45,
   45, 45, 45 },
  { -1, 52, 52, 52, 52, 52, 52, 52,
   52, 52, 52, 52, 52, 52, 52, 52,
   52, 52, 52, 52, 52, 52, 52, 37,
   52, 52, 52, 52, 53, 52, 52, 52,
   52, 52, 52 },
  { -1, 54, 54, 54, 54, 54, 54, 54,
   54, 54, 54, 54, 54, 54, 54, 54,
   54, 54, 54, 54, 54, 54, 54, 54,
   38, 54, 54, 54, 55, 54, 54, 54,
   54, 54, 54 },
  { -1, -1, 42, -1, -1, 42, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   42, -1, -1, -1, -1, -1, -1, -1,
   -1, 26, 42, 42, -1, -1, -1, 42,
   -1, -1, -1 },
  { -1, 43, 42, -1, -1, 42, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   42, -1, -1, -1, -1, -1, -1, -1,
   -1, 42, 27, 48, -1, -1, -1, 42,
   -1, -1, -1 },
  { -1, 39, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, 56, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, 60, 60, 60, 60, 59, 60, 60,
   60, 60, 60, 60, 60, 60, 60, 60,
   60, 60, 60, 60, 60, 60, 60, 60,
   60, 60, 60, 60, 60, 60, 60, 60,
   60, 60, 60 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, 43, 51, -1, -1, -1, -1,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, 29, -1,
   -1, -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, 41, -1,
   -1, -1, -1 },
  { -1, 45, 45, 45, 45, 45, 45, 45,
   45, 45, 45, 45, 45, 45, 45, 45,
   45, 45, 45, 45, 45, 45, 45, 45,
   45, 45, 45, 45, 45, 45, -1, 45,
   -1, 45, 45 },
  { 1, 2, 3, 4, 5, 6, 7, 8,
   9, 10, 11, 12, 13, 14, 15, 16,
   17, 18, 19, 20, 21, 22, 23, 24,
   25, 26, 27, 42, 28, 28, 41, 42,
   46, 30, 29 },
  { -1, 52, 52, 52, 52, 52, 52, 52,
   52, 52, 52, 52, 52, 52, 52, 52,
   52, 52, 52, 52, 52, 52, 52, 52,
   52, 52, 52, 52, 52, 52, -1, 52,
   -1, 52, 52 },
  { -1, 54, 54, 54, 54, 54, 54, 54,
   54, 54, 54, 54, 54, 54, 54, 54,
   54, 54, 54, 54, 54, 54, 54, 54,
   54, 54, 54, 54, 54, 54, -1, 54,
   -1, 54, 54 },
  { -1, 61, 61, 61, 61, 62, 61, 61,
   61, 61, 61, 61, 58, 61, 61, 61,
   61, 61, 61, 61, 61, 61, 61, 61,
   61, 61, 61, 61, 61, 61, 61, 61,
   61, 56, 61 },
  { -1, 58, 58, 58, 58, 57, 58, 58,
   58, 58, 40, 58, 58, 58, 58, 58,
   58, 58, 58, 58, 58, 58, 58, 58,
   58, 58, 58, 58, 58, 58, 61, 58,
   61, 58, 58 },
  { -1, 58, 58, 58, 58, 57, 58, 58,
   58, 58, 58, 58, 58, 58, 58, 58,
   58, 58, 58, 58, 58, 58, 58, 58,
   58, 58, 58, 58, 58, 58, 61, 58,
   61, 58, 58 },
  { -1, 60, 60, 60, 60, 59, 60, 60,
   60, 60, 40, 60, 60, 60, 60, 60,
   60, 60, 60, 60, 60, 60, 60, 60,
   60, 60, 60, 60, 60, 60, 60, 60,
   60, 60, 60 },
  { -1, 61, 61, 61, 61, 62, 61, 61,
   61, 61, 61, 61, 61, 61, 61, 61,
   61, 61, 61, 61, 61, 61, 61, 61,
   61, 61, 61, 61, 61, 61, 61, 61,
   61, 61, 61 },
  { -1, 61, 61, 61, 61, 62, 61, 61,
   61, 61, 60, 61, 61, 61, 61, 61,
   61, 61, 61, 61, 61, 61, 61, 61,
   61, 61, 61, 61, 61, 61, 61, 61,
   61, 61, 61 }
  };
public SqlToken GetToken()
  {
  char yy_lookahead;
  int yy_anchor = YY_NO_ANCHOR;
  int yy_state = yy_state_dtrans[yy_lexical_state];
  int yy_next_state = YY_NO_STATE;
  int yy_last_accept_state = YY_NO_STATE;
  bool yy_initial = true;
  int yy_this_accept;

  yy_mark_start();
  yy_this_accept = yy_acpt[yy_state];
  if (YY_NOT_ACCEPT != yy_this_accept)
    {
    yy_last_accept_state = yy_state;
    yy_mark_end();
    }
  while (true)
    {
    if (yy_initial && yy_at_bol)
      yy_lookahead = (char) YY_BOL;
    else
      {
      yy_lookahead = yy_advance();
      }
    yy_next_state = yy_nxt[yy_rmap[yy_state],yy_cmap[yy_lookahead]];
    if (YY_EOF == yy_lookahead && yy_initial)
      {

  return newToken(SqlSymbols.EOF, null);
      }
    if (YY_F != yy_next_state)
      {
      yy_state = yy_next_state;
      yy_initial = false;
      yy_this_accept = yy_acpt[yy_state];
      if (YY_NOT_ACCEPT != yy_this_accept)
        {
        yy_last_accept_state = yy_state;
        yy_mark_end();
        }
      }
    else
      {
      if (YY_NO_STATE == yy_last_accept_state)
        {
        throw new System.ApplicationException("Lexical Error: Unmatched Input.");
        }
      else
        {
        yy_anchor = yy_acpt[yy_last_accept_state];
        if (0 != (YY_END & yy_anchor))
          {
          yy_move_end();
          }
        yy_to_mark();
        if (yy_last_accept_state < 0)
          {
          if (yy_last_accept_state < 63)
            yy_error(YY_E_INTERNAL, false);
          }
        else
          {
          AcceptMethod m = accept_dispatch[yy_last_accept_state];
          if (m != null)
            {
            SqlToken tmp = m();
            if (tmp != null)
              return tmp;
            }
          }
        yy_initial = true;
        yy_state = yy_state_dtrans[yy_lexical_state];
        yy_next_state = YY_NO_STATE;
        yy_last_accept_state = YY_NO_STATE;
        yy_mark_start();
        yy_this_accept = yy_acpt[yy_state];
        if (YY_NOT_ACCEPT != yy_this_accept)
          {
          yy_last_accept_state = yy_state;
          yy_mark_end();
          }
        }
      }
    }
  }
}

}
