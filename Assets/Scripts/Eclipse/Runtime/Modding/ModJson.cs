using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Eclipse.Modding
{
    public enum ModJsonKind { Null, Boolean, Number, String, Array, Object }

    /// <summary>
    /// A parsed JSON value for strict mod data files. Objects keep member order and
    /// reject duplicate names; numbers remember whether they were written as integers.
    /// Dependency-free so headless fixtures compile it with the rest of the runtime.
    /// </summary>
    public sealed class ModJsonNode
    {
        public ModJsonKind Kind { get; private set; }
        public bool Boolean { get; private set; }
        public double Number { get; private set; }
        /// <summary>True when the number token had no fraction or exponent.</summary>
        public bool IsInteger { get; private set; }
        public string String { get; private set; }
        public List<ModJsonNode> Items { get; private set; }
        public List<KeyValuePair<string, ModJsonNode>> Members { get; private set; }

        public int Count => Kind == ModJsonKind.Array ? Items.Count : Kind == ModJsonKind.Object ? Members.Count : 0;
        public ModJsonNode this[string name]
        {
            get
            {
                if (Kind != ModJsonKind.Object) return null;
                foreach (var member in Members) if (member.Key == name) return member.Value;
                return null;
            }
        }

        public static ModJsonNode Null() => new ModJsonNode { Kind = ModJsonKind.Null };
        public static ModJsonNode Of(bool value) => new ModJsonNode { Kind = ModJsonKind.Boolean, Boolean = value };
        public static ModJsonNode Of(string value) => new ModJsonNode { Kind = ModJsonKind.String, String = value ?? throw new ArgumentNullException(nameof(value)) };
        public static ModJsonNode Of(int value) => new ModJsonNode { Kind = ModJsonKind.Number, Number = value, IsInteger = true };
        public static ModJsonNode Of(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value), "JSON numbers must be finite.");
            return new ModJsonNode { Kind = ModJsonKind.Number, Number = value };
        }
        public static ModJsonNode NewArray() => new ModJsonNode { Kind = ModJsonKind.Array, Items = new List<ModJsonNode>() };
        public static ModJsonNode NewObject() => new ModJsonNode { Kind = ModJsonKind.Object, Members = new List<KeyValuePair<string, ModJsonNode>>() };

        public ModJsonNode Add(ModJsonNode item) { Items.Add(item); return this; }
        public ModJsonNode Set(string name, ModJsonNode value)
        {
            if (this[name] != null) throw new InvalidOperationException("Duplicate JSON member: " + name);
            Members.Add(new KeyValuePair<string, ModJsonNode>(name, value));
            return this;
        }

        // ---- Strict parser ----

        public static ModJsonNode Parse(string text, int maxDepth = 32)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var parser = new Parser(text, maxDepth);
            parser.SkipSpace();
            if (parser.Position < text.Length && text[parser.Position] == '﻿') { parser.Position++; parser.SkipSpace(); }
            var value = parser.Value(0);
            parser.SkipSpace();
            if (parser.Position != text.Length) throw parser.Error("unexpected text after the document");
            return value;
        }

        private sealed class Parser
        {
            private readonly string _text;
            private readonly int _maxDepth;
            public int Position;
            public Parser(string text, int maxDepth) { _text = text; _maxDepth = maxDepth; }

            public FormatException Error(string message)
            {
                int line = 1, column = 1;
                for (int i = 0; i < Position && i < _text.Length; i++) { if (_text[i] == '\n') { line++; column = 1; } else column++; }
                return new FormatException(message + " (line " + line + ", column " + column + ")");
            }

            public void SkipSpace()
            {
                while (Position < _text.Length && (_text[Position] == ' ' || _text[Position] == '\t' || _text[Position] == '\n' || _text[Position] == '\r')) Position++;
            }

            private char Peek() => Position < _text.Length ? _text[Position] : '\0';

            public ModJsonNode Value(int depth)
            {
                if (depth > _maxDepth) throw Error("nesting is deeper than " + _maxDepth);
                SkipSpace();
                char c = Peek();
                if (c == '{') return ObjectValue(depth);
                if (c == '[') return ArrayValue(depth);
                if (c == '"') return Of(StringValue());
                if (c == '-' || (c >= '0' && c <= '9')) return NumberValue();
                if (Literal("true")) return Of(true);
                if (Literal("false")) return Of(false);
                if (Literal("null")) return Null();
                throw Error(Position >= _text.Length ? "unexpected end of document" : "unexpected character '" + c + "'");
            }

            private bool Literal(string word)
            {
                if (string.CompareOrdinal(_text, Position, word, 0, word.Length) != 0) return false;
                Position += word.Length;
                return true;
            }

            private ModJsonNode ObjectValue(int depth)
            {
                Position++;
                var node = NewObject();
                SkipSpace();
                if (Peek() == '}') { Position++; return node; }
                while (true)
                {
                    SkipSpace();
                    if (Peek() != '"') throw Error("expected a member name");
                    int at = Position;
                    string name = StringValue();
                    if (node[name] != null) { Position = at; throw Error("duplicate member '" + name + "'"); }
                    SkipSpace();
                    if (Peek() != ':') throw Error("expected ':'");
                    Position++;
                    node.Members.Add(new KeyValuePair<string, ModJsonNode>(name, Value(depth + 1)));
                    SkipSpace();
                    char c = Peek();
                    Position++;
                    if (c == '}') return node;
                    if (c != ',') { Position--; throw Error("expected ',' or '}'"); }
                }
            }

            private ModJsonNode ArrayValue(int depth)
            {
                Position++;
                var node = NewArray();
                SkipSpace();
                if (Peek() == ']') { Position++; return node; }
                while (true)
                {
                    node.Items.Add(Value(depth + 1));
                    SkipSpace();
                    char c = Peek();
                    Position++;
                    if (c == ']') return node;
                    if (c != ',') { Position--; throw Error("expected ',' or ']'"); }
                }
            }

            private string StringValue()
            {
                Position++;
                var builder = new StringBuilder();
                while (true)
                {
                    if (Position >= _text.Length) throw Error("unterminated string");
                    char c = _text[Position++];
                    if (c == '"') return builder.ToString();
                    if (c < ' ') { Position--; throw Error("control character in string"); }
                    if (c != '\\') { builder.Append(c); continue; }
                    if (Position >= _text.Length) throw Error("unterminated escape");
                    char e = _text[Position++];
                    switch (e)
                    {
                        case '"': builder.Append('"'); break;
                        case '\\': builder.Append('\\'); break;
                        case '/': builder.Append('/'); break;
                        case 'b': builder.Append('\b'); break;
                        case 'f': builder.Append('\f'); break;
                        case 'n': builder.Append('\n'); break;
                        case 'r': builder.Append('\r'); break;
                        case 't': builder.Append('\t'); break;
                        case 'u':
                            if (Position + 4 > _text.Length || !int.TryParse(_text.Substring(Position, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int code))
                                throw Error("invalid \\u escape");
                            builder.Append((char)code); Position += 4; break;
                        default: Position--; throw Error("invalid escape '\\" + e + "'");
                    }
                }
            }

            private ModJsonNode NumberValue()
            {
                int start = Position;
                if (Peek() == '-') Position++;
                if (Peek() == '0') Position++;
                else if (Peek() >= '1' && Peek() <= '9') while (Peek() >= '0' && Peek() <= '9') Position++;
                else throw Error("invalid number");
                bool integer = true;
                if (Peek() == '.')
                {
                    integer = false; Position++;
                    if (!(Peek() >= '0' && Peek() <= '9')) throw Error("invalid number");
                    while (Peek() >= '0' && Peek() <= '9') Position++;
                }
                if (Peek() == 'e' || Peek() == 'E')
                {
                    integer = false; Position++;
                    if (Peek() == '+' || Peek() == '-') Position++;
                    if (!(Peek() >= '0' && Peek() <= '9')) throw Error("invalid number");
                    while (Peek() >= '0' && Peek() <= '9') Position++;
                }
                string token = _text.Substring(start, Position - start);
                double value = double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (double.IsInfinity(value)) throw Error("number is out of range");
                return new ModJsonNode { Kind = ModJsonKind.Number, Number = value, IsInteger = integer };
            }
        }

        // ---- Canonical writer: two-space indent, LF, invariant numbers ----

        public string ToJson()
        {
            var builder = new StringBuilder();
            Write(builder, 0);
            return builder.Append('\n').ToString();
        }

        private void Write(StringBuilder builder, int indent)
        {
            switch (Kind)
            {
                case ModJsonKind.Null: builder.Append("null"); return;
                case ModJsonKind.Boolean: builder.Append(Boolean ? "true" : "false"); return;
                case ModJsonKind.Number:
                    if (IsInteger) { builder.Append(((long)Number).ToString(CultureInfo.InvariantCulture)); return; }
                    string text = Number.ToString("R", CultureInfo.InvariantCulture);
                    if (text.IndexOf('.') < 0 && text.IndexOf('E') < 0) text += ".0";
                    builder.Append(text); return;
                case ModJsonKind.String: Quote(builder, String); return;
                case ModJsonKind.Array:
                    if (Items.Count == 0) { builder.Append("[]"); return; }
                    // Arrays of scalars stay on one line (edges, impulse); nested values break.
                    bool scalars = Items.TrueForAll(item => item.Kind != ModJsonKind.Array && item.Kind != ModJsonKind.Object);
                    builder.Append('[');
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (i > 0) builder.Append(scalars ? ", " : ",");
                        if (!scalars) { builder.Append('\n'); builder.Append(' ', (indent + 1) * 2); }
                        Items[i].Write(builder, indent + 1);
                    }
                    if (!scalars) { builder.Append('\n'); builder.Append(' ', indent * 2); }
                    builder.Append(']');
                    return;
                default:
                    if (Members.Count == 0) { builder.Append("{}"); return; }
                    builder.Append('{');
                    for (int i = 0; i < Members.Count; i++)
                    {
                        if (i > 0) builder.Append(',');
                        builder.Append('\n'); builder.Append(' ', (indent + 1) * 2);
                        Quote(builder, Members[i].Key); builder.Append(": ");
                        Members[i].Value.Write(builder, indent + 1);
                    }
                    builder.Append('\n'); builder.Append(' ', indent * 2); builder.Append('}');
                    return;
            }
        }

        private static void Quote(StringBuilder builder, string value)
        {
            builder.Append('"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (c < ' ') builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else builder.Append(c);
                        break;
                }
            }
            builder.Append('"');
        }
    }
}
