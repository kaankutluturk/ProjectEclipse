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

		public ParsingEvent Clone(ParsingEvent FOPOKALJIIJ)
		{
			FOPOKALJIIJ.Accept(this);
			return clonedEvent;
		}

		void IParsingEventVisitor.Visit(AnchorAlias FOPOKALJIIJ)
		{
			clonedEvent = new AnchorAlias(FOPOKALJIIJ.GetValue(), FOPOKALJIIJ.GetStart(), FOPOKALJIIJ.GetEnd());
		}

		void IParsingEventVisitor.Visit(StreamStart FOPOKALJIIJ)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(StreamEndEvent FOPOKALJIIJ)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(DocumentStart FOPOKALJIIJ)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(DocumentEnd FOPOKALJIIJ)
		{
			throw new NotSupportedException();
		}

		void IParsingEventVisitor.Visit(Scalar FOPOKALJIIJ)
		{
			clonedEvent = new Scalar(null, FOPOKALJIIJ.GetTag(), FOPOKALJIIJ.GetValue(), FOPOKALJIIJ.GetStyle(), FOPOKALJIIJ.GetIsPlainImplicit(), FOPOKALJIIJ.GetIsQuotedImplicit(), FOPOKALJIIJ.GetStart(), FOPOKALJIIJ.GetEnd());
		}

		void IParsingEventVisitor.Visit(SequenceStart FOPOKALJIIJ)
		{
			clonedEvent = new SequenceStart(null, FOPOKALJIIJ.GetTag(), FOPOKALJIIJ.GetIsImplicit(), FOPOKALJIIJ.GetStyle(), FOPOKALJIIJ.GetStart(), FOPOKALJIIJ.GetEnd());
		}

		void IParsingEventVisitor.Visit(SequenceEnd FOPOKALJIIJ)
		{
			clonedEvent = new SequenceEnd(FOPOKALJIIJ.GetStart(), FOPOKALJIIJ.GetEnd());
		}

		void IParsingEventVisitor.Visit(MappingStart FOPOKALJIIJ)
		{
			clonedEvent = new MappingStart(null, FOPOKALJIIJ.GetTag(), FOPOKALJIIJ.GetIsImplicit(), FOPOKALJIIJ.GetStyle(), FOPOKALJIIJ.GetStart(), FOPOKALJIIJ.GetEnd());
		}

		void IParsingEventVisitor.Visit(MappingEnd FOPOKALJIIJ)
		{
			clonedEvent = new MappingEnd(FOPOKALJIIJ.GetStart(), FOPOKALJIIJ.GetEnd());
		}

		void IParsingEventVisitor.Visit(Comment FOPOKALJIIJ)
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

	public MergingParser(IParser FIMPGLKJDKK)
	{
		innerParser = FIMPGLKJDKK;
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
				Scalar lEACOCDHICF = events[num] as Scalar;
				if (lEACOCDHICF == null || !(lEACOCDHICF.GetValue() == "<<"))
				{
					continue;
				}
				AnchorAlias mBEGNNDMDKH = events[num + 1] as AnchorAlias;
				if (mBEGNNDMDKH != null)
				{
					IEnumerable<ParsingEvent> collection = GetMappingEvents(mBEGNNDMDKH.GetValue());
					events.RemoveRange(num, 2);
					events.InsertRange(num, collection);
					continue;
				}
				SequenceStart jODGINIKFJF = events[num + 1] as SequenceStart;
				if (jODGINIKFJF != null)
				{
					List<IEnumerable<ParsingEvent>> list = new List<IEnumerable<ParsingEvent>>();
					bool flag = false;
					for (int i = num + 2; i < events.Count; i++)
					{
						mBEGNNDMDKH = events[i] as AnchorAlias;
						if (mBEGNNDMDKH != null)
						{
							list.Add(GetMappingEvents(mBEGNNDMDKH.GetValue()));
						}
						else if (events[i] is SequenceEnd)
						{
							events.RemoveRange(num, i - num + 1);
							events.InsertRange(num, list.SelectMany((IEnumerable<ParsingEvent> FOPOKALJIIJ) => FOPOKALJIIJ));
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				throw new SemanticErrorException(lEACOCDHICF.GetStart(), lEACOCDHICF.GetEnd(), "Unrecognized merge key pattern");
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
		ParsingEventCloner PFCAJIFNHMC = new ParsingEventCloner();
		int nesting = 0;
		return (from FOPOKALJIIJ in events.SkipWhile((ParsingEvent FOPOKALJIIJ) =>
			{
				MappingStart oGMPNFCPPDH = FOPOKALJIIJ as MappingStart;
				return oGMPNFCPPDH == null || oGMPNFCPPDH.GetAnchor() != mappingAlias;
			}).Skip(1).TakeWhile((ParsingEvent FOPOKALJIIJ) => (nesting += FOPOKALJIIJ.GetNestingIncrease()) >= 0)
			select PFCAJIFNHMC.Clone(FOPOKALJIIJ)).ToList();
	}
}
