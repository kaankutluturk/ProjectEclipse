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

		public YamlStream(params YamlDocument[] KODNOAMCEJE)
			: this((IEnumerable<YamlDocument>)KODNOAMCEJE)
		{
		}

		public YamlStream(IEnumerable<YamlDocument> KODNOAMCEJE)
		{
			foreach (YamlDocument item in KODNOAMCEJE)
			{
				documents.Add(item);
			}
		}

		public void Add(YamlDocument DPMKHPJABAF)
		{
			documents.Add(DPMKHPJABAF);
		}

		public void Load(TextReader NILNDHEKNLJ)
		{
			documents.Clear();
			YamlEventParser bPGMNGAJMKK = new YamlEventParser(NILNDHEKNLJ);
			EventReader dCDJJJDPACI = new EventReader(bPGMNGAJMKK);
			dCDJJJDPACI.Expect<StreamStart>();
			while (!dCDJJJDPACI.Accept<StreamEndEvent>())
			{
				YamlDocument item = new YamlDocument(dCDJJJDPACI);
				documents.Add(item);
			}
			dCDJJJDPACI.Expect<StreamEndEvent>();
		}

		public void Save(TextWriter output, bool EENMGCCBIHF = true)
		{
			IEmitter nEKGJNOFOFN = new Emitter(output);
			nEKGJNOFOFN.Emit(new StreamStart());
			foreach (YamlDocument document in documents)
			{
				document.Save(nEKGJNOFOFN, EENMGCCBIHF);
			}
			nEKGJNOFOFN.Emit(new StreamEndEvent());
		}

		public void Accept(IYamlVisitor NKECMANOOEM)
		{
			NKECMANOOEM.Visit(this);
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
