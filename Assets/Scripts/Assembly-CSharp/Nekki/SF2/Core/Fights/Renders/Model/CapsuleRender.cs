using Eclipse.Rendering.Interpolation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nekki.SF2.Core.Fights.Renders.Model
{
	public class CapsuleRender : MonoBehaviour
	{
		private float _Stroke = 1f;

		protected Capsule _Base;

		private static Material coloredSpriteMaterial;

		private static Material defaultSpriteMaterial;

		private LineRenderer _LineRender;

		private readonly VectorSegmentInterpolation _Interpolation = new VectorSegmentInterpolation();

		private Eclipse.Rendering.ModelPresentation _Presentation;

		private int _TintVersion = -1;

		private Color _OriginalStartColor;

		private Color _OriginalEndColor;

		private LineRenderer _RimLine;
		private Eclipse.Rendering.FighterVolume _Volume;

		public float StrokeWidth
		{
			get
			{
				return get_Stroke();
			}
			set
			{
				set_Stroke(value);
			}
		}

		private static Material ColoredSpriteMaterial
		{
			get
			{
				return GetColoredSpriteMaterial();
			}
		}

		private static Material DefaultSpriteMaterial
		{
			get
			{
				return GetDefaultSpriteMaterial();
			}
		}

		public float get_Stroke()
		{
			return _Stroke;
		}

		public void set_Stroke(float value)
		{
			_Stroke = value;
		}

		public Capsule get_Base()
		{
			return _Base;
		}

		public void set_Base(Capsule value)
		{
			_Base = value;
		}

		// Experimental rim light: an offset, rim-coloured twin line behind this one.
		private void UpdateRim(Vector3 start, Vector3 end)
		{
			bool active = Eclipse.Rendering.RimLight.Active && (_Volume == null || !_Volume.gameObject.activeSelf);
			if (!active)
			{
				if (_RimLine != null && _RimLine.gameObject.activeSelf) _RimLine.gameObject.SetActive(false);
				return;
			}
			if (_RimLine == null)
			{
				GameObject rim = new GameObject("Rim");
				rim.transform.SetParent(base.transform, false);
				_RimLine = rim.AddComponent<LineRenderer>();
				_RimLine.numCapVertices = _LineRender.numCapVertices;
				_RimLine.useWorldSpace = false;
				_RimLine.sharedMaterial = _LineRender.sharedMaterial;
				_RimLine.shadowCastingMode = ShadowCastingMode.Off;
				_RimLine.receiveShadows = false;
				_RimLine.alignment = LineAlignment.TransformZ;
			}
			if (!_RimLine.gameObject.activeSelf) _RimLine.gameObject.SetActive(true);
			_RimLine.transform.localPosition = Eclipse.Rendering.RimLight.LocalOffset(base.transform);
			_RimLine.startWidth = _RimLine.endWidth = _Stroke;
			Color color = Eclipse.Rendering.RimLight.ColorFor(base.transform);
			_RimLine.startColor = _RimLine.endColor = color;
			_RimLine.SetPosition(0, start);
			_RimLine.SetPosition(1, end);
		}

		// Per-capsule tint from the owning fighter; the shared materials stay untouched.
		private void ApplyTint()
		{
			if (_Presentation == null || _TintVersion == _Presentation.TintVersion) return;
			_TintVersion = _Presentation.TintVersion;
			Color? tint = _Presentation.Tint;
			_LineRender.startColor = tint ?? _OriginalStartColor;
			_LineRender.endColor = tint ?? _OriginalEndColor;
		}

		private static Material GetColoredSpriteMaterial()
		{
			if (coloredSpriteMaterial == null)
			{
				coloredSpriteMaterial = new Material(Shader.Find("Sprites/Colored"));
			}
			return coloredSpriteMaterial;
		}

		private static Material GetDefaultSpriteMaterial()
		{
			if (defaultSpriteMaterial == null)
			{
				defaultSpriteMaterial = new Material(Shader.Find("Sprites/Default"));
			}
			return defaultSpriteMaterial;
		}

		private void Start()
		{
			_Stroke = _Base.GetThickness();
			_LineRender = base.gameObject.AddComponent<LineRenderer>();
			_LineRender.numCapVertices = 9;
			LineRenderer nLHJNOCKKGE = _LineRender;
			float pJMDIHLGNHB = _Stroke;
			_LineRender.endWidth = pJMDIHLGNHB;
			nLHJNOCKKGE.startWidth = pJMDIHLGNHB;
			_LineRender.useWorldSpace = false;
			_LineRender.sharedMaterial = GetDefaultSpriteMaterial();
			_LineRender.shadowCastingMode = ShadowCastingMode.Off;
			_LineRender.receiveShadows = false;
			_LineRender.alignment = LineAlignment.TransformZ;
			_OriginalStartColor = _LineRender.startColor;
			_OriginalEndColor = _LineRender.endColor;
			_Presentation = GetComponentInParent<Eclipse.Rendering.ModelPresentation>();
			Render();
		}

		private void Update()
		{
			Render();
		}

		public void Render()
		{
			if (_Base != null && !Vector2f.op_Equality(_Base.GetStart(), null) && !Vector2f.op_Equality(_Base.GetEnd(), null) && !(_LineRender == null))
			{
				Vector3f eMAFACPEPDK = _Base.GetStart();
				Vector3f eMAFACPEPDK2 = _Base.GetEnd();
				Vector3 rawStart = new Vector3(eMAFACPEPDK.GetX(), eMAFACPEPDK.GetY(), eMAFACPEPDK.GetZ());
				Vector3 rawEnd = new Vector3(eMAFACPEPDK2.GetX(), eMAFACPEPDK2.GetY(), eMAFACPEPDK2.GetZ());
				Vector3 start;
				Vector3 end;
				_Interpolation.Sample(rawStart, rawEnd, Eclipse.Rendering.ModelPresentation.AlphaFor(_Presentation), out start, out end);
				ApplyTint();
				float num = end.x - start.x;
				float num2 = end.y - start.y;
				float x = start.x + num * _Base.GetMargin1();
				float y = start.y + num2 * _Base.GetMargin1();
				float x2 = start.x + num * (1f - _Base.GetMargin2());
				float y2 = start.y + num2 * (1f - _Base.GetMargin2());
				if (_Stroke != _Base.GetThickness())
				{
					_Stroke = _Base.GetThickness();
					LineRenderer nLHJNOCKKGE = _LineRender;
					float pJMDIHLGNHB = _Stroke;
					_LineRender.endWidth = pJMDIHLGNHB;
					nLHJNOCKKGE.startWidth = pJMDIHLGNHB;
				}
				_LineRender.SetPosition(0, new Vector3(x, y, 0f));
				_LineRender.SetPosition(1, new Vector3(x2, y2, 0f));
				bool solid = Eclipse.Rendering.ExperimentalFighterCamera.ActiveFor(transform);
				bool bodyOutline = solid && Eclipse.Rendering.ProceduralFighterBody.IsBodyOutline(gameObject.name) &&
                    Eclipse.Rendering.ProceduralFighterBody.RenderFor(_Presentation, _LineRender.startColor);
                if (solid && !bodyOutline && _Volume == null) _Volume = Eclipse.Rendering.FighterVolume.Create(transform);
				solid = solid && (bodyOutline || _Volume != null);
				if (_Volume != null) _Volume.gameObject.SetActive(solid && !bodyOutline);
				_LineRender.enabled = !solid;
				if (solid && !bodyOutline) _Volume.Capsule(Vector3.LerpUnclamped(start, end, _Base.GetMargin1()),
					Vector3.LerpUnclamped(start, end, 1f - _Base.GetMargin2()), _Stroke, _LineRender.startColor);
				UpdateRim(new Vector3(x, y, 0f), new Vector3(x2, y2, 0f));
			}
		}
	}
}
