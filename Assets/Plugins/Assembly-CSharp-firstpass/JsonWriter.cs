using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public class JsonWriter
{
	private static NumberFormatInfo number_format;

	private WriterContext context;

	private Stack<WriterContext> ctx_stack;

	private bool has_reached_end;

	private char[] hex_seq;

	private int indentation;

	private int indent_value;

	private StringBuilder inst_string_builder;

	private bool pretty_print;

	private bool validate;

	private TextWriter writer;

	public int IndentSize
	{
		get
		{
			return GetIndentValue();
		}
		set
		{
			set_IndentValue(value);
		}
	}

	public bool PrettyPrint
	{
		get
		{
			return GetPrettyPrint();
		}
		set
		{
			SetPrettyPrint(value);
		}
	}

	public TextWriter TextWriter
	{
		get
		{
			return GetTextWriter();
		}
	}

	public bool Validate
	{
		get
		{
			return GetValidate();
		}
		set
		{
			SetValidate(value);
		}
	}

	static JsonWriter()
	{
		number_format = NumberFormatInfo.InvariantInfo;
	}

	public JsonWriter()
	{
		inst_string_builder = new StringBuilder();
		writer = new StringWriter(inst_string_builder);
		Init();
	}

	public JsonWriter(StringBuilder NGPACMILENE)
		: this(new StringWriter(NGPACMILENE))
	{
	}

	public JsonWriter(TextWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		this.writer = writer;
		Init();
	}

	public int GetIndentValue()
	{
		return indent_value;
	}

	public void set_IndentValue(int value)
	{
		indentation = indentation / indent_value * value;
		indent_value = value;
	}

	public bool GetPrettyPrint()
	{
		return pretty_print;
	}

	public void SetPrettyPrint(bool value)
	{
		pretty_print = value;
	}

	public TextWriter GetTextWriter()
	{
		return writer;
	}

	public bool GetValidate()
	{
		return validate;
	}

	public void SetValidate(bool value)
	{
		validate = value;
	}

	private void DoValidation(JsonWriterCondition AJEPDBPHNCM)
	{
		if (!context.ExpectingValue)
		{
			context.Count++;
		}
		if (!validate)
		{
			return;
		}
		if (has_reached_end)
		{
			throw new JsonException("A complete JSON symbol has already been written");
		}
		switch (AJEPDBPHNCM)
		{
		case JsonWriterCondition.InArray:
			if (!context.InArray)
			{
				throw new JsonException("Can't close an array here");
			}
			break;
		case JsonWriterCondition.InObject:
			if (!context.InObject || context.ExpectingValue)
			{
				throw new JsonException("Can't close an object here");
			}
			break;
		case JsonWriterCondition.NotAProperty:
			if (context.InObject && !context.ExpectingValue)
			{
				throw new JsonException("Expected a property");
			}
			break;
		case JsonWriterCondition.Property:
			if (!context.InObject || context.ExpectingValue)
			{
				throw new JsonException("Can't add a property here");
			}
			break;
		case JsonWriterCondition.Value:
			if (!context.InArray && (!context.InObject || !context.ExpectingValue))
			{
				throw new JsonException("Can't add a value here");
			}
			break;
		}
	}

	private void Init()
	{
		has_reached_end = false;
		hex_seq = new char[4];
		indentation = 0;
		indent_value = 4;
		pretty_print = false;
		validate = true;
		ctx_stack = new Stack<WriterContext>();
		context = new WriterContext();
		ctx_stack.Push(context);
	}

	private static void IntToHex(int HDKKKCDKFEE, char[] IJGJLEJKMBJ)
	{
		for (int i = 0; i < 4; i++)
		{
			int num = HDKKKCDKFEE % 16;
			if (num < 10)
			{
				IJGJLEJKMBJ[3 - i] = (char)(48 + num);
			}
			else
			{
				IJGJLEJKMBJ[3 - i] = (char)(65 + (num - 10));
			}
			HDKKKCDKFEE >>= 4;
		}
	}

	private void Indent()
	{
		if (pretty_print)
		{
			indentation += indent_value;
		}
	}

	private void Put(string IGGFGLLIGCG)
	{
		if (pretty_print && !context.ExpectingValue)
		{
			for (int i = 0; i < indentation; i++)
			{
				writer.Write(' ');
			}
		}
		writer.Write(IGGFGLLIGCG);
	}

	private void PutNewline()
	{
		PutNewline(true);
	}

	private void PutNewline(bool GOLEKPDOAAP)
	{
		if (GOLEKPDOAAP && !context.ExpectingValue && context.Count > 1)
		{
			writer.Write(',');
		}
		if (pretty_print && !context.ExpectingValue)
		{
			writer.Write('\n');
		}
	}

	private void PutString(string IGGFGLLIGCG)
	{
		Put(string.Empty);
		writer.Write('"');
		int length = IGGFGLLIGCG.Length;
		for (int i = 0; i < length; i++)
		{
			switch (IGGFGLLIGCG[i])
			{
			case '\n':
				writer.Write("\\n");
				continue;
			case '\r':
				writer.Write("\\r");
				continue;
			case '\t':
				writer.Write("\\t");
				continue;
			case '"':
			case '\\':
				writer.Write('\\');
				writer.Write(IGGFGLLIGCG[i]);
				continue;
			case '\f':
				writer.Write("\\f");
				continue;
			case '\b':
				writer.Write("\\b");
				continue;
			}
			if (IGGFGLLIGCG[i] >= ' ' && IGGFGLLIGCG[i] <= '~')
			{
				writer.Write(IGGFGLLIGCG[i]);
				continue;
			}
			IntToHex(IGGFGLLIGCG[i], hex_seq);
			writer.Write("\\u");
			writer.Write(hex_seq);
		}
		writer.Write('"');
	}

	private void Unindent()
	{
		if (pretty_print)
		{
			indentation -= indent_value;
		}
	}

	public override string ToString()
	{
		if (inst_string_builder == null)
		{
			return string.Empty;
		}
		return inst_string_builder.ToString();
	}

	public void Reset()
	{
		has_reached_end = false;
		ctx_stack.Clear();
		context = new WriterContext();
		ctx_stack.Push(context);
		if (inst_string_builder != null)
		{
			inst_string_builder.Remove(0, inst_string_builder.Length);
		}
	}

	public void Write(bool CIGMFMBICLJ)
	{
		DoValidation(JsonWriterCondition.Value);
		PutNewline();
		Put((!CIGMFMBICLJ) ? "false" : "true");
		context.ExpectingValue = false;
	}

	public void Write(decimal number)
	{
		DoValidation(JsonWriterCondition.Value);
		PutNewline();
		Put(Convert.ToString(number, number_format));
		context.ExpectingValue = false;
	}

	public void Write(double number)
	{
		DoValidation(JsonWriterCondition.Value);
		PutNewline();
		string text = Convert.ToString(number, number_format);
		Put(text);
		if (text.IndexOf('.') == -1 && text.IndexOf('E') == -1)
		{
			writer.Write(".0");
		}
		context.ExpectingValue = false;
	}

	public void Write(int number)
	{
		DoValidation(JsonWriterCondition.Value);
		PutNewline();
		Put(Convert.ToString(number, number_format));
		context.ExpectingValue = false;
	}

	public void Write(long number)
	{
		DoValidation(JsonWriterCondition.Value);
		PutNewline();
		Put(Convert.ToString(number, number_format));
		context.ExpectingValue = false;
	}

	public void Write(string IGGFGLLIGCG)
	{
		DoValidation(JsonWriterCondition.Value);
		PutNewline();
		if (IGGFGLLIGCG == null)
		{
			Put("null");
		}
		else
		{
			PutString(IGGFGLLIGCG);
		}
		context.ExpectingValue = false;
	}

	public void Write(ulong number)
	{
		DoValidation(JsonWriterCondition.Value);
		PutNewline();
		Put(Convert.ToString(number, number_format));
		context.ExpectingValue = false;
	}

	public void WriteArrayEnd()
	{
		DoValidation(JsonWriterCondition.InArray);
		PutNewline(false);
		ctx_stack.Pop();
		if (ctx_stack.Count == 1)
		{
			has_reached_end = true;
		}
		else
		{
			context = ctx_stack.Peek();
			context.ExpectingValue = false;
		}
		Unindent();
		Put("]");
	}

	public void WriteArrayStart()
	{
		DoValidation(JsonWriterCondition.NotAProperty);
		PutNewline();
		Put("[");
		context = new WriterContext();
		context.InArray = true;
		ctx_stack.Push(context);
		Indent();
	}

	public void WriteObjectEnd()
	{
		DoValidation(JsonWriterCondition.InObject);
		PutNewline(false);
		ctx_stack.Pop();
		if (ctx_stack.Count == 1)
		{
			has_reached_end = true;
		}
		else
		{
			context = ctx_stack.Peek();
			context.ExpectingValue = false;
		}
		Unindent();
		Put("}");
	}

	public void WriteObjectStart()
	{
		DoValidation(JsonWriterCondition.NotAProperty);
		PutNewline();
		Put("{");
		context = new WriterContext();
		context.InObject = true;
		ctx_stack.Push(context);
		Indent();
	}

	public void WritePropertyName(string MHJMMIJKOGH)
	{
		DoValidation(JsonWriterCondition.Property);
		PutNewline();
		PutString(MHJMMIJKOGH);
		if (pretty_print)
		{
			if (MHJMMIJKOGH.Length > context.Padding)
			{
				context.Padding = MHJMMIJKOGH.Length;
			}
			for (int num = context.Padding - MHJMMIJKOGH.Length; num >= 0; num--)
			{
				writer.Write(' ');
			}
			writer.Write(": ");
		}
		else
		{
			writer.Write(':');
		}
		context.ExpectingValue = true;
	}
}
