public interface IParsingEventVisitor
{
	void Visit(AnchorAlias FOPOKALJIIJ);

	void Visit(StreamStart FOPOKALJIIJ);

	void Visit(StreamEndEvent FOPOKALJIIJ);

	void Visit(DocumentStart FOPOKALJIIJ);

	void Visit(DocumentEnd FOPOKALJIIJ);

	void Visit(Scalar FOPOKALJIIJ);

	void Visit(SequenceStart FOPOKALJIIJ);

	void Visit(SequenceEnd FOPOKALJIIJ);

	void Visit(MappingStart FOPOKALJIIJ);

	void Visit(MappingEnd FOPOKALJIIJ);

	void Visit(Comment FOPOKALJIIJ);
}
