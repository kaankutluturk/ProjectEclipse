using System;
using System.IO;

public class BinTree : InWindow, IInWindowStream, IMatchFinder
{
	private uint _cyclicBufferPos;

	private uint _cyclicBufferSize;

	private uint _matchMaxLen;

	private uint[] _son;

	private uint[] _hash;

	private uint _cutValue = 255u;

	private uint _hashMask;

	private uint _hashSizeSum;

	private bool HASH_ARRAY = true;

	private const uint kHash2Size = 1024u;

	private const uint kHash3Size = 65536u;

	private const uint kBT2HashSize = 65536u;

	private const uint kStartMaxLen = 1u;

	private const uint kHash3Offset = 1024u;

	private const uint kEmptyHashValue = 0u;

	private const uint kMaxValForNormalize = 2147483647u;

	private uint kNumHashDirectBytes;

	private uint kMinMatchCheck = 4u;

	private uint kFixHashSize = 66560u;

	public void SetType(int EOKCENIBPJD)
	{
		HASH_ARRAY = EOKCENIBPJD > 2;
		if (HASH_ARRAY)
		{
			kNumHashDirectBytes = 0u;
			kMinMatchCheck = 4u;
			kFixHashSize = 66560u;
		}
		else
		{
			kNumHashDirectBytes = 2u;
			kMinMatchCheck = 3u;
			kFixHashSize = 0u;
		}
	}

	public new void SetStream(Stream ABJIEFMMIEK)
	{
		base.SetStream(ABJIEFMMIEK);
	}

	public new void ReleaseStream()
	{
		base.ReleaseStream();
	}

	public new void Init()
	{
		base.Init();
		for (uint num = 0u; num < _hashSizeSum; num++)
		{
			_hash[num] = 0u;
		}
		_cyclicBufferPos = 0u;
		ReduceOffsets(-1);
	}

	public new void MovePos()
	{
		if (++_cyclicBufferPos >= _cyclicBufferSize)
		{
			_cyclicBufferPos = 0u;
		}
		base.MovePos();
		if (_pos == int.MaxValue)
		{
			Normalize();
		}
	}

	public new byte GetIndexByte(int index)
	{
		return base.GetIndexByte(index);
	}

	public new uint GetMatchLen(int index, uint OIOMNNFMDOO, uint LOHCIKNKDEI)
	{
		return base.GetMatchLen(index, OIOMNNFMDOO, LOHCIKNKDEI);
	}

	public new uint GetNumAvailableBytes()
	{
		return base.GetNumAvailableBytes();
	}

	public void Create(uint PGNMIJNBAAJ, uint JHKHNGLLCLK, uint CCKFKNACIIN, uint CDINDGLFPKA)
	{
		if (PGNMIJNBAAJ > 2147483391)
		{
			throw new Exception();
		}
		_cutValue = 16 + (CCKFKNACIIN >> 1);
		uint iKHIOAIPBNL = (PGNMIJNBAAJ + JHKHNGLLCLK + CCKFKNACIIN + CDINDGLFPKA) / 2 + 256;
		Create(PGNMIJNBAAJ + JHKHNGLLCLK, CCKFKNACIIN + CDINDGLFPKA, iKHIOAIPBNL);
		_matchMaxLen = CCKFKNACIIN;
		uint num = PGNMIJNBAAJ + 1;
		if (_cyclicBufferSize != num)
		{
			_son = new uint[(_cyclicBufferSize = num) * 2];
		}
		uint num2 = 65536u;
		if (HASH_ARRAY)
		{
			num2 = PGNMIJNBAAJ - 1;
			num2 |= num2 >> 1;
			num2 |= num2 >> 2;
			num2 |= num2 >> 4;
			num2 |= num2 >> 8;
			num2 >>= 1;
			num2 |= 0xFFFF;
			if (num2 > 16777216)
			{
				num2 >>= 1;
			}
			_hashMask = num2;
			num2++;
			num2 += kFixHashSize;
		}
		if (num2 != _hashSizeSum)
		{
			_hash = new uint[_hashSizeSum = num2];
		}
	}

	public uint GetMatches(uint[] PIPLHPNGIPF)
	{
		uint num;
		if (_pos + _matchMaxLen <= _streamPos)
		{
			num = _matchMaxLen;
		}
		else
		{
			num = _streamPos - _pos;
			if (num < kMinMatchCheck)
			{
				MovePos();
				return 0u;
			}
		}
		uint num2 = 0u;
		uint num3 = ((_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0u);
		uint num4 = _bufferOffset + _pos;
		uint num5 = 1u;
		uint num6 = 0u;
		uint num7 = 0u;
		uint num9;
		if (HASH_ARRAY)
		{
			uint num8 = CRC.Table[_bufferBase[num4]] ^ _bufferBase[num4 + 1];
			num6 = num8 & 0x3FF;
			num8 ^= (uint)(_bufferBase[num4 + 2] << 8);
			num7 = num8 & 0xFFFF;
			num9 = (num8 ^ (CRC.Table[_bufferBase[num4 + 3]] << 5)) & _hashMask;
		}
		else
		{
			num9 = (uint)(_bufferBase[num4] ^ (_bufferBase[num4 + 1] << 8));
		}
		uint num10 = _hash[kFixHashSize + num9];
		if (HASH_ARRAY)
		{
			uint num11 = _hash[num6];
			uint num12 = _hash[1024 + num7];
			_hash[num6] = _pos;
			_hash[1024 + num7] = _pos;
			if (num11 > num3 && _bufferBase[_bufferOffset + num11] == _bufferBase[num4])
			{
				num5 = (PIPLHPNGIPF[num2++] = 2u);
				PIPLHPNGIPF[num2++] = _pos - num11 - 1;
			}
			if (num12 > num3 && _bufferBase[_bufferOffset + num12] == _bufferBase[num4])
			{
				if (num12 == num11)
				{
					num2 -= 2;
				}
				num5 = (PIPLHPNGIPF[num2++] = 3u);
				PIPLHPNGIPF[num2++] = _pos - num12 - 1;
				num11 = num12;
			}
			if (num2 != 0 && num11 == num10)
			{
				num2 -= 2;
				num5 = 1u;
			}
		}
		_hash[kFixHashSize + num9] = _pos;
		uint num13 = (_cyclicBufferPos << 1) + 1;
		uint num14 = _cyclicBufferPos << 1;
		uint val2;
		uint val = (val2 = kNumHashDirectBytes);
		if (kNumHashDirectBytes != 0 && num10 > num3 && _bufferBase[_bufferOffset + num10 + kNumHashDirectBytes] != _bufferBase[num4 + kNumHashDirectBytes])
		{
			num5 = (PIPLHPNGIPF[num2++] = kNumHashDirectBytes);
			PIPLHPNGIPF[num2++] = _pos - num10 - 1;
		}
		uint pNKNDHJACDC = _cutValue;
		while (true)
		{
			if (num10 <= num3 || pNKNDHJACDC-- == 0)
			{
				_son[num13] = (_son[num14] = 0u);
				break;
			}
			uint num15 = _pos - num10;
			uint num16 = ((num15 > _cyclicBufferPos) ? (_cyclicBufferPos - num15 + _cyclicBufferSize) : (_cyclicBufferPos - num15)) << 1;
			uint num17 = _bufferOffset + num10;
			uint num18 = Math.Min(val, val2);
			if (_bufferBase[num17 + num18] == _bufferBase[num4 + num18])
			{
				while (++num18 != num && _bufferBase[num17 + num18] == _bufferBase[num4 + num18])
				{
				}
				if (num5 < num18)
				{
					num5 = (PIPLHPNGIPF[num2++] = num18);
					PIPLHPNGIPF[num2++] = num15 - 1;
					if (num18 == num)
					{
						_son[num14] = _son[num16];
						_son[num13] = _son[num16 + 1];
						break;
					}
				}
			}
			if (_bufferBase[num17 + num18] < _bufferBase[num4 + num18])
			{
				_son[num14] = num10;
				num14 = num16 + 1;
				num10 = _son[num14];
				val2 = num18;
			}
			else
			{
				_son[num13] = num10;
				num13 = num16;
				num10 = _son[num13];
				val = num18;
			}
		}
		MovePos();
		return num2;
	}

	public void Skip(uint OMEDGJMNGKE)
	{
		do
		{
			uint num;
			if (_pos + _matchMaxLen <= _streamPos)
			{
				num = _matchMaxLen;
			}
			else
			{
				num = _streamPos - _pos;
				if (num < kMinMatchCheck)
				{
					MovePos();
					continue;
				}
			}
			uint num2 = ((_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0u);
			uint num3 = _bufferOffset + _pos;
			uint num7;
			if (HASH_ARRAY)
			{
				uint num4 = CRC.Table[_bufferBase[num3]] ^ _bufferBase[num3 + 1];
				uint num5 = num4 & 0x3FF;
				_hash[num5] = _pos;
				num4 ^= (uint)(_bufferBase[num3 + 2] << 8);
				uint num6 = num4 & 0xFFFF;
				_hash[1024 + num6] = _pos;
				num7 = (num4 ^ (CRC.Table[_bufferBase[num3 + 3]] << 5)) & _hashMask;
			}
			else
			{
				num7 = (uint)(_bufferBase[num3] ^ (_bufferBase[num3 + 1] << 8));
			}
			uint num8 = _hash[kFixHashSize + num7];
			_hash[kFixHashSize + num7] = _pos;
			uint num9 = (_cyclicBufferPos << 1) + 1;
			uint num10 = _cyclicBufferPos << 1;
			uint val2;
			uint val = (val2 = kNumHashDirectBytes);
			uint pNKNDHJACDC = _cutValue;
			while (true)
			{
				if (num8 <= num2 || pNKNDHJACDC-- == 0)
				{
					_son[num9] = (_son[num10] = 0u);
					break;
				}
				uint num11 = _pos - num8;
				uint num12 = ((num11 > _cyclicBufferPos) ? (_cyclicBufferPos - num11 + _cyclicBufferSize) : (_cyclicBufferPos - num11)) << 1;
				uint num13 = _bufferOffset + num8;
				uint num14 = Math.Min(val, val2);
				if (_bufferBase[num13 + num14] == _bufferBase[num3 + num14])
				{
					while (++num14 != num && _bufferBase[num13 + num14] == _bufferBase[num3 + num14])
					{
					}
					if (num14 == num)
					{
						_son[num10] = _son[num12];
						_son[num9] = _son[num12 + 1];
						break;
					}
				}
				if (_bufferBase[num13 + num14] < _bufferBase[num3 + num14])
				{
					_son[num10] = num8;
					num10 = num12 + 1;
					num8 = _son[num10];
					val2 = num14;
				}
				else
				{
					_son[num9] = num8;
					num9 = num12;
					num8 = _son[num9];
					val = num14;
				}
			}
			MovePos();
		}
		while (--OMEDGJMNGKE != 0);
	}

	private void NormalizeLinks(uint[] HELFDCAIJNE, uint DDLKICOHOGG, uint BALBEBAOPMP)
	{
		for (uint num = 0u; num < DDLKICOHOGG; num++)
		{
			uint num2 = HELFDCAIJNE[num];
			num2 = ((num2 > BALBEBAOPMP) ? (num2 - BALBEBAOPMP) : 0u);
			HELFDCAIJNE[num] = num2;
		}
	}

	private void Normalize()
	{
		uint bALBEBAOPMP = _pos - _cyclicBufferSize;
		NormalizeLinks(_son, _cyclicBufferSize * 2, bALBEBAOPMP);
		NormalizeLinks(_hash, _hashSizeSum, bALBEBAOPMP);
		ReduceOffsets((int)bALBEBAOPMP);
	}

	public void SetCutValue(uint PADNFMPEFDM)
	{
		_cutValue = PADNFMPEFDM;
	}
}
