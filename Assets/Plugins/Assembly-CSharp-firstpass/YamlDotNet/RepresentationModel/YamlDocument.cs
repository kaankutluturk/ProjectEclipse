using System;
using System.Collections.Generic;
using System.Globalization;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel
{
	[Serializable]
	public class YamlDocument
	{
		private class AnchorAssigningVisitor : YamlVisitor
		{
			private readonly HashSet<string> existingAnchors = new HashSet<string>();

			private readonly Dictionary<YamlNode, bool> visitedNodes = new Dictionary<YamlNode, bool>(new YamlNodeIdentityEqualityComparer());

			public void AssignAnchors(YamlDocument document)
			{
				existingAnchors.Clear();
				visitedNodes.Clear();
				document.Accept(this);
				Random random = new Random();
				foreach (KeyValuePair<YamlNode, bool> item in visitedNodes)
				{
					if (item.Value)
					{
						string text;
						do
						{
							text = random.Next().ToString(CultureInfo.InvariantCulture);
						}
						while (existingAnchors.Contains(text));
						existingAnchors.Add(text);
						item.Key.Anchor = text;
					}
				}
			}

			private void VisitNode(YamlNode node)
			{
				if (string.IsNullOrEmpty(node.Anchor))
				{
					bool value;
					if (visitedNodes.TryGetValue(node, out value))
					{
						if (!value)
						{
							visitedNodes[node] = true;
						}
					}
					else
					{
						visitedNodes.Add(node, false);
					}
				}
				else
				{
					existingAnchors.Add(node.Anchor);
				}
			}

			protected override void Visit(YamlScalarNode scalar)
			{
				VisitNode(scalar);
			}

			protected override void Visit(YamlMappingNode mapping)
			{
				VisitNode(mapping);
			}

			protected override void Visit(YamlSequenceNode sequence)
			{
				VisitNode(sequence);
			}
		}

		public YamlNode RootNode { get; private set; }

		public IEnumerable<YamlNode> AllNodes
		{
			get
			{
				return RootNode.AllNodes;
			}
		}

		public YamlDocument(YamlNode rootNode)
		{
			RootNode = rootNode;
		}

		public YamlDocument(string text)
		{
			RootNode = new YamlScalarNode(text);
		}

		internal YamlDocument(EventReader reader)
		{
			DocumentLoadingState loadingState = new DocumentLoadingState();
			reader.Expect<DocumentStart>();
			while (!reader.Accept<DocumentEnd>())
			{
				RootNode = YamlNode.ParseNode(reader, loadingState);
				if (RootNode is YamlAliasNode)
				{
					throw new YamlException();
				}
			}
			loadingState.ResolveAliases();
			reader.Expect<DocumentEnd>();
		}

		private void AssignAnchors()
		{
			AnchorAssigningVisitor anchorVisitor = new AnchorAssigningVisitor();
			anchorVisitor.AssignAnchors(this);
		}

		internal void Save(IEmitter emitter, bool assignAnchors = true)
		{
			if (assignAnchors)
			{
				AssignAnchors();
			}
			emitter.Emit(new DocumentStart());
			RootNode.Save(emitter, new EmitterState());
			emitter.Emit(new DocumentEnd(false));
		}

		public void Accept(IYamlVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
