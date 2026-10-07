public interface IParsingEventVisitor
{
	void Visit(AnchorAlias anchorAlias);

	void Visit(StreamStart streamStart);

	void Visit(StreamEndEvent streamEnd);

	void Visit(DocumentStart documentStart);

	void Visit(DocumentEnd documentEnd);

	void Visit(Scalar scalar);

	void Visit(SequenceStart sequenceStart);

	void Visit(SequenceEnd sequenceEnd);

	void Visit(MappingStart mappingStart);

	void Visit(MappingEnd mappingEnd);

	void Visit(Comment comment);
}
