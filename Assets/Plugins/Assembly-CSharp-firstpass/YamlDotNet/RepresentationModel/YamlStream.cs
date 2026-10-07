using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YamlDotNet.RepresentationModel
{
	[Serializable]
	public class YamlStream : IEnumerable<YamlDocument>, IEnumerable
	{
		private readonly IList<YamlDocument> documents = new List<YamlDocument>();

		public IList<YamlDocument> Documents
		{
			get
			{
				return documents;
			}
		}

		public YamlStream()
		{
		}

		public YamlStream(params YamlDocument[] initialDocuments)
			: this((IEnumerable<YamlDocument>)initialDocuments)
		{
		}

		public YamlStream(IEnumerable<YamlDocument> initialDocuments)
		{
			foreach (YamlDocument item in initialDocuments)
			{
				documents.Add(item);
			}
		}

		public void Add(YamlDocument document)
		{
			documents.Add(document);
		}

		public void Load(TextReader input)
		{
			documents.Clear();
			YamlEventParser parser = new YamlEventParser(input);
			EventReader eventReader = new EventReader(parser);
			eventReader.Expect<StreamStart>();
			while (!eventReader.Accept<StreamEndEvent>())
			{
				YamlDocument item = new YamlDocument(eventReader);
				documents.Add(item);
			}
			eventReader.Expect<StreamEndEvent>();
		}

		public void Save(TextWriter output, bool assignAnchors = true)
		{
			IEmitter emitter = new Emitter(output);
			emitter.Emit(new StreamStart());
			foreach (YamlDocument document in documents)
			{
				document.Save(emitter, assignAnchors);
			}
			emitter.Emit(new StreamEndEvent());
		}

		public void Accept(IYamlVisitor visitor)
		{
			visitor.Visit(this);
		}

		public IEnumerator<YamlDocument> GetEnumerator()
		{
			return documents.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
