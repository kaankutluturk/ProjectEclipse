using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using YamlDotNet.Core;

public sealed class MergingParser : IParser
{
	private class ParsingEventCloner : IParsingEventVisitor
	{
		private ParsingEvent clonedEvent;

		public ParsingEvent Clone(ParsingEvent parsingEvent)
		{
			parsingEvent.Accept(this);
			return clonedEvent;
		}

		void IParsingEventVisitor.Visit(AnchorAlias anchorAlias)
		{
			clonedEvent = new AnchorAlias(anchorAlias.GetValue(), anchorAlias.GetStart(), anchorAlias.GetEnd());
		}

		void IParsingEventVisitor.Visit(StreamStart streamStart)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(StreamEndEvent streamEnd)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(DocumentStart documentStart)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(DocumentEnd documentEnd)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(Scalar scalar)
		{
			clonedEvent = new Scalar(null, scalar.GetTag(), scalar.GetValue(), scalar.GetStyle(), scalar.GetIsPlainImplicit(), scalar.GetIsQuotedImplicit(), scalar.GetStart(), scalar.GetEnd());
		}

		void IParsingEventVisitor.Visit(SequenceStart sequenceStart)
		{
			clonedEvent = new SequenceStart(null, sequenceStart.GetTag(), sequenceStart.GetIsImplicit(), sequenceStart.GetStyle(), sequenceStart.GetStart(), sequenceStart.GetEnd());
		}

		void IParsingEventVisitor.Visit(SequenceEnd sequenceEnd)
		{
			clonedEvent = new SequenceEnd(sequenceEnd.GetStart(), sequenceEnd.GetEnd());
		}

		void IParsingEventVisitor.Visit(MappingStart mappingStart)
		{
			clonedEvent = new MappingStart(null, mappingStart.GetTag(), mappingStart.GetIsImplicit(), mappingStart.GetStyle(), mappingStart.GetStart(), mappingStart.GetEnd());
		}

		void IParsingEventVisitor.Visit(MappingEnd mappingEnd)
		{
			clonedEvent = new MappingEnd(mappingEnd.GetStart(), mappingEnd.GetEnd());
		}

		void IParsingEventVisitor.Visit(Comment comment)
		{
			throw new NotSupportedException();
		}
	}

	private readonly List<ParsingEvent> events = new List<ParsingEvent>();

	private readonly IParser innerParser;

	private int _currentIndex = -1;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ParsingEvent current;

	public ParsingEvent CurrentEvent
	{
		get
		{
			return GetCurrent();
		}
		private set
		{
			SetCurrent(value);
		}
	}

	public MergingParser(IParser parser)
	{
		innerParser = parser;
	}

	public ParsingEvent GetCurrent()
	{
		return current;
	}

	private void SetCurrent(ParsingEvent value)
	{
		current = value;
	}

	public bool MoveNext()
	{
		if (_currentIndex < 0)
		{
			while (innerParser.MoveNext())
			{
				events.Add(innerParser.GetCurrent());
			}
			for (int num = events.Count - 2; num >= 0; num--)
			{
				Scalar scalar = events[num] as Scalar;
				if (scalar == null || !(scalar.GetValue() == "<<"))
				{
					continue;
				}
				AnchorAlias anchorAlias = events[num + 1] as AnchorAlias;
				if (anchorAlias != null)
				{
					IEnumerable<ParsingEvent> collection = GetMappingEvents(anchorAlias.GetValue());
					events.RemoveRange(num, 2);
					events.InsertRange(num, collection);
					continue;
				}
				SequenceStart sequenceStart = events[num + 1] as SequenceStart;
				if (sequenceStart != null)
				{
					List<IEnumerable<ParsingEvent>> list = new List<IEnumerable<ParsingEvent>>();
					bool flag = false;
					for (int i = num + 2; i < events.Count; i++)
					{
						anchorAlias = events[i] as AnchorAlias;
						if (anchorAlias != null)
						{
							list.Add(GetMappingEvents(anchorAlias.GetValue()));
						}
						else if (events[i] is SequenceEnd)
						{
							events.RemoveRange(num, i - num + 1);
							events.InsertRange(num, list.SelectMany((IEnumerable<ParsingEvent> mappingEvents) => mappingEvents));
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				throw new SemanticErrorException(scalar.GetStart(), scalar.GetEnd(), "Unrecognized merge key pattern");
			}
		}
		int num2 = _currentIndex + 1;
		if (num2 < events.Count)
		{
			SetCurrent(events[num2]);
			_currentIndex = num2;
			return true;
		}
		return false;
	}

	private IEnumerable<ParsingEvent> GetMappingEvents(string mappingAlias)
	{
		ParsingEventCloner cloner = new ParsingEventCloner();
		int nesting = 0;
		return (from parsingEvent in events.SkipWhile((ParsingEvent parsingEvent) =>
			{
				MappingStart mappingStart = parsingEvent as MappingStart;
				return mappingStart == null || mappingStart.GetAnchor() != mappingAlias;
			}).Skip(1).TakeWhile((ParsingEvent parsingEvent) => (nesting += parsingEvent.GetNestingIncrease()) >= 0)
			select cloner.Clone(parsingEvent)).ToList();
	}
}
